import shutil
from pathlib import Path
from png2ctex import CtexConverter
from process_localization import process_localization

class BaseProcessor:
    def __init__(self, mod_id, assets_dir, staging_dir):
        self.mod_id = mod_id
        self.assets_dir = Path(assets_dir)
        self.staging_dir = Path(staging_dir)

    def process(self, folder_path: Path):
        """处理特定文件夹"""
        pass

class LocalizationProcessor(BaseProcessor):
    def process(self, folder_path: Path):
        print(f"  [Loc] 处理本地化: {folder_path.name}")
        # 直接调用现有的逻辑，source指向assets里的localization文件夹
        process_localization(
            assets_dir=self.assets_dir,
            mod_id=self.mod_id,
            output_dir=self.staging_dir # 内部会处理成 staging/modid/localization
        )

class ImageProcessor(BaseProcessor):
    def process(self, folder_path: Path):
        print(f"  [Img] 转换图片: {folder_path.name}")
        # 计算在 staging 中的相对路径 (保持层级，如 modid/images/xxx)
        rel_dir = folder_path.relative_to(self.assets_dir)
        dest_dir = self.staging_dir / self.mod_id / rel_dir
        dest_dir.mkdir(parents=True, exist_ok=True)

        for png_file in folder_path.rglob("*.png"):
            # 获取相对于当前处理目录的相对路径 (如 cards/attack.png)
            rel_path = png_file.relative_to(folder_path)
            # 构建虚拟资源路径用于生成MD5
            res_rel_path = f"{folder_path.name}/{rel_path}"

            # 转换并获取条目
            entries = CtexConverter.convert_and_get_entries(
                png_file,
                res_prefix=self.mod_id,
                rel_path=res_rel_path
            )

            # 写入 staging
            for vpath, data in entries:
                # vpath 可能是 res://modid/xxx 或 res://.godot/imported/xxx
                # 我们需要将其映射到磁盘物理路径
                if vpath.startswith("res://.godot/"):
                    # 放入 .pck_staging/.godot/...
                    disk_path = self.staging_dir / vpath.replace("res://", "")
                else:
                    # 放入 .pck_staging/modid/...
                    disk_path = self.staging_dir / vpath.replace("res://", "")

                disk_path.parent.mkdir(parents=True, exist_ok=True)
                if isinstance(data, str):
                    disk_path.write_text(data, encoding='utf-8')
                else:
                    disk_path.write_bytes(data)

class DefaultProcessor(BaseProcessor):
    def process(self, folder_path: Path):
        print(f"  [CP] 复制资源: {folder_path.name}")
        rel_dir = folder_path.relative_to(self.assets_dir)
        dest_dir = self.staging_dir / self.mod_id / rel_dir
        if dest_dir.exists():
            shutil.rmtree(dest_dir)
        shutil.copytree(folder_path, dest_dir)

class AssetOrchestrator:
    def __init__(self, mod_id, solution_dir, staging_dir):
        self.mod_id = mod_id
        self.solution_dir = Path(solution_dir)
        self.staging_dir = Path(staging_dir)

        # 目录名与处理器的映射
        self.processors = {
            "localization": LocalizationProcessor,
            "images": ImageProcessor,
            "audio": DefaultProcessor,
            "vfx": DefaultProcessor
        }

    def _find_asset_folders_in_dir(self, base_dir: Path):
        """在给定目录下寻找名为 'assets' (不区分大小写) 的文件夹"""
        results = []
        if not base_dir.exists(): return results

        for item in base_dir.iterdir():
            if item.is_dir() and item.name.lower() == "assets":
                results.append(item)
        return results

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

    def run(self):
        asset_roots = self._find_all_asset_roots()

        if not asset_roots:
            print(f"⚠️  在解决方案中未找到任何 assets 目录。")
            return

        print(f"🏗️  正在从 {len(asset_roots)} 个位置编排资源...")

        for assets_path in asset_roots:
            print(f"  📂 正在处理来自项目 [{assets_path.parent.name}] 的资源...")

            # 这里我们需要更新 assets_dir，让处理器知道当前相对的是哪个 assets
            for folder in assets_path.iterdir():
                if not folder.is_dir():
                    continue

                processor_cls = self.processors.get(folder.name.lower(), DefaultProcessor)

                # 注意：这里传入的是具体的 assets_path 而不是全局的 solution_dir
                processor = processor_cls(self.mod_id, assets_path, self.staging_dir)
                processor.process(folder)