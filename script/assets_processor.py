from concurrent.futures import ThreadPoolExecutor, as_completed
from rich.progress import (
    Progress,
    SpinnerColumn,
    BarColumn,
    TextColumn,
    TimeRemainingColumn,
    MofNCompleteColumn
)
from rich.console import Console
import time
import os
import shutil
from pathlib import Path
from png2ctex import CtexConverter
from process_localization import process_localization

console = Console()


class BaseProcessor:
    def __init__(self, mod_id, assets_dir, staging_dir):
        self.mod_id = mod_id
        self.assets_dir = Path(assets_dir)
        self.staging_dir = Path(staging_dir)
        # 记录本次处理中产生/确认有效的文件物理路径
        self.valid_files = set()

    def _should_update(self, src: Path, dst: Path) -> bool:
        """检查是否需要更新：只要时间戳不严格相等，就认为需要更新"""
        if not dst.exists():
            return True
        # 使用 != 处理重命名、回滚和覆盖场景
        return src.stat().st_mtime != dst.stat().st_mtime

    def _sync_timestamp(self, src: Path, dst: Path):
        """将源文件的时间戳强制应用到目标文件"""
        st = src.stat()
        # 同时同步访问时间和修改时间
        os.utime(dst, (st.st_atime, st.st_mtime))

    def _mark_valid(self, path: Path):
        """记录该文件为有效文件，防止被作为孤儿清理"""
        self.valid_files.add(path.resolve())

    def _get_phys_path(self, vpath: str) -> Path:
        """将 res:// 路径映射为物理路径"""
        # vpath 格式如: res://modid/xxx 或 res://.godot/imported/xxx
        rel_path = vpath.replace("res://", "")
        return (self.staging_dir / rel_path).resolve()

    def process(self, folder_path: Path) -> set:
        """处理文件夹并返回有效文件集合"""
        return self.valid_files


class LocalizationProcessor(BaseProcessor):
    def process(self, folder_path: Path):
        # 本地化由于逻辑复杂（扫描代码），保持原有清空逻辑，但记录产出的文件
        target_loc_dir = self.staging_dir / self.mod_id / "localization"

        # 删除旧的本地化目录
        if target_loc_dir.exists():
            console.print(f"  [Loc] 清理旧目录: {target_loc_dir}")
            shutil.rmtree(target_loc_dir)

        console.print(f"  [Loc] 处理本地化: {folder_path.name}")
        process_localization(
            assets_dir=self.assets_dir,
            mod_id=self.mod_id,
            output_dir=self.staging_dir
        )

        # 将生成的所有本地化文件标记为有效
        if target_loc_dir.exists():
            for f in target_loc_dir.rglob("*"):
                if f.is_file():
                    self._mark_valid(f)
        return self.valid_files


class ImageProcessor(BaseProcessor):
    def process(self, folder_path: Path, max_workers=8):
        png_files = list(folder_path.rglob("*.png"))
        if not png_files:
            return self.valid_files

        # 预筛选：找出真正需要处理的文件
        tasks_to_run = []
        for png_file in png_files:
            rel_path = png_file.relative_to(folder_path)
            res_rel_path = f"{folder_path.name}/{rel_path.as_posix()}"

            # 使用轻量级方法获取路径
            info = CtexConverter.get_output_info(png_file, self.mod_id, res_rel_path)
            ctex_p = self._get_phys_path(info["ctex_vpath"])
            import_p = self._get_phys_path(info["import_vpath"])

            # 标记这些路径在本次任务中是“合法”的（不论是否需要重新生成）
            self._mark_valid(ctex_p)
            self._mark_valid(import_p)

            # 增量检查：如果 ctex 或 import 任意一个不存在或过期，则加入任务
            if self._should_update(png_file, ctex_p) or self._should_update(png_file, import_p):
                tasks_to_run.append((png_file, res_rel_path))

        if not tasks_to_run:
            console.print(f"  [dim][Img] {folder_path.name}: 所有图片均为最新，跳过。[/dim]")
            return self.valid_files

        console.print(f"  [cyan][Img] 转换图片:[/cyan] {folder_path.name} [dim](更新 {len(tasks_to_run)} / 总计 {len(png_files)})[/dim]")

        with Progress(
                SpinnerColumn(),
                TextColumn("[progress.description]{task.description}"),
                BarColumn(),
                MofNCompleteColumn(),
                TextColumn("•"),
                TextColumn("{task.fields[filename]}"),
                console=console,
                transient=True
        ) as progress:
            task = progress.add_task("[green]正在转换", total=len(tasks_to_run), filename="")

            with ThreadPoolExecutor(max_workers=max_workers) as executor:
                futures = {
                    executor.submit(CtexConverter.convert_and_get_entries, t[0], self.mod_id, t[1]): t[0]
                    for t in tasks_to_run
                }

                for f in as_completed(futures):
                    png_file = futures[f]
                    try:
                        entries = f.result()
                        for vpath, data in entries:
                            dst_p = self._get_phys_path(vpath)
                            dst_p.parent.mkdir(parents=True, exist_ok=True)
                            if isinstance(data, str):
                                dst_p.write_text(data, encoding='utf-8')
                            else:
                                dst_p.write_bytes(data)

                            # 同步时间戳
                            self._sync_timestamp(png_file, dst_p)
                        progress.update(task, advance=1, filename=png_file.name)
                    except Exception as e:
                        console.print(f"[red]错误: 处理 {png_file.name} 失败: {e}[/red]")

        return self.valid_files

    def _convert_logic(self, png_file: Path, res_rel_path: str):
        """处理单张图片的转换判断"""
        # 获取预期的产物列表 (ctex 和 .import)
        entries = CtexConverter.convert_and_get_entries(
            png_file,
            res_prefix=self.mod_id,
            rel_path=res_rel_path
        )

        phys_paths = []
        needs_write = False

        # 1. 检查是否需要更新（只要有一个产物丢失或过时，就全量重写该条目）
        for vpath, _ in entries:
            dst_p = self._get_phys_path(vpath)
            phys_paths.append(dst_p)
            if self._should_update(png_file, dst_p):
                needs_write = True

        # 2. 执行写入
        if needs_write:
            for vpath, data in entries:
                dst_p = self._get_phys_path(vpath)
                dst_p.parent.mkdir(parents=True, exist_ok=True)
                if isinstance(data, str):
                    dst_p.write_text(data, encoding='utf-8')
                else:
                    dst_p.write_bytes(data)

        return phys_paths


class DefaultProcessor(BaseProcessor):
    def process(self, folder_path: Path):
        console.print(f"  [CP] 同步资源: {folder_path.name}")

        for src_file in folder_path.rglob("*"):
            if src_file.is_dir(): continue

            rel_path = src_file.relative_to(self.assets_dir)
            dst_file = self.staging_dir / self.mod_id / rel_path

            if self._should_update(src_file, dst_file):
                dst_file.parent.mkdir(parents=True, exist_ok=True)
                # 使用 copy2 保留元数据（特别是 mtime）
                shutil.copy2(src_file, dst_file)

            self._mark_valid(dst_file)

        return self.valid_files


class AssetOrchestrator:
    def __init__(self, mod_id, solution_dir, staging_dir):
        self.mod_id = mod_id
        self.solution_dir = Path(solution_dir)
        self.staging_dir = Path(staging_dir)
        self.all_valid_files = set()

        # 目录名与处理器的映射
        self.processors = {
            "localization": LocalizationProcessor,
            "images": ImageProcessor,
            "audio": DefaultProcessor,
            "vfx": DefaultProcessor
        }

    def _find_all_asset_roots(self):
        """扫描所有子项目目录，寻找 assets 文件夹"""
        asset_roots = []

        # 1. 检查根目录是否有 assets
        root_assets = self.solution_dir / "assets"
        if root_assets.exists():
            asset_roots.append(root_assets)

        # 2. 遍历一级子目录 (代表各个子项目)
        for sub_dir in self.solution_dir.iterdir():
            # 排除以 . 开头的隐藏文件夹 (.git, .vs, .pck_staging)
            # 排除编译器生成的 bin 和 obj 文件夹
            if sub_dir.is_dir() and not sub_dir.name.startswith('.') and sub_dir.name not in ['bin', 'obj']:
                potential_assets = sub_dir / "assets"
                if potential_assets.exists():
                    asset_roots.append(potential_assets)

        return asset_roots

    def cleanup_orphans(self):
        """清理不再属于任何 asset 源的陈旧文件"""
        # 需要检查的两个主要输出目录
        target_dirs = [
            self.staging_dir / self.mod_id,
            self.staging_dir / ".godot" / "imported"
        ]

        removed_count = 0
        for root in target_dirs:
            if not root.exists(): continue

            # 必须转为 list 因为在遍历中会删除文件
            for item in list(root.rglob("*")):
                if item.is_file():
                    if item.resolve() not in self.all_valid_files:
                        item.unlink()
                        removed_count += 1
                elif item.is_dir():
                    # 清理空文件夹
                    if not any(item.iterdir()):
                        try:
                            item.rmdir()
                        except:
                            pass

        if removed_count > 0:
            console.print(f"  [yellow]🧹 已清理 {removed_count} 个孤儿文件 (已从源中删除/移动)[/yellow]")

    def run(self):
        asset_roots = self._find_all_asset_roots()

        if not asset_roots:
            console.print("⚠️  [yellow]未发现 assets 目录[/yellow]")
            return

        console.print(f"🏗️  [bold]开始编排资源...[/bold]")

        for assets_path in asset_roots:
            console.print(f"  📂 来自: [blue]{assets_path.parent.name}[/blue]")
            for folder in assets_path.iterdir():
                if not folder.is_dir(): continue

                proc_cls = self.processors.get(folder.name.lower(), DefaultProcessor)
                processor = proc_cls(self.mod_id, assets_path, self.staging_dir)

                # 执行处理并更新全局有效文件库
                valid_paths = processor.process(folder)
                self.all_valid_files.update(valid_paths)

        # 孤儿检查
        self.cleanup_orphans()
        console.print("✨ [bold green]资源编排同步完成[/bold green]")
