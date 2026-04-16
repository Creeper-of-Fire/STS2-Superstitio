#!/usr/bin/env python3
# build_mod.py - 统一的模组构建脚本
#
# 功能：模组构建的主入口，统筹编译、资源处理、打包、安装、运行等完整流程
#
# 使用示例：
#   python build_mod.py                             # 默认完整流程：构建 -> 资源处理 -> 安装
#   python build_mod.py --build-only                # 仅构建 DLL 和处理资源
#   python build_mod.py --pack-only                 # 仅处理资源并打包 PCK
#   python build_mod.py --install-only              # 仅安装（打包并复制到游戏目录）
#   python build_mod.py --clean xxx                 # 构建前清理构建产物
#   python build_mod.py --kill xxx                  # 构建前杀死游戏进程
#   python build_mod.py --run xxx                   # 构建后启动游戏
#   python build_mod.py --loc-extract --toml xxx    # 构建后额外运行本地化文本提取器，并且以toml格式收集内容
#
# 环境变量 (.env 文件)：
#   STS2_GAME_DIR        - 杀戮尖塔2游戏安装目录
#
# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire

import os
import sys

sys.dont_write_bytecode = True

import subprocess
import argparse
import shutil
import tomllib
from pathlib import Path


class ModBuilder:
    def __init__(self, solution_dir, config='Debug'):
        self.solution_dir = Path(solution_dir).absolute()
        self.config = config
        self.output_dir = self._get_output_dir()
        self.script_dir = self.solution_dir.parent / "script"
        self.mod_name = self._get_mod_name()
        self.game_dir = self._get_game_dir()
        self.game_exe = self.game_dir / "SlayTheSpire2.exe"

        self.assets_dir = self.solution_dir / "assets"
        self.staging_dir = self.solution_dir / ".pck_staging"

    def _get_output_dir(self):
        """从 MSBuild 获取实际输出路径"""
        base_output = self.solution_dir / "bin" / self.config

        # 查找最新的构建输出目录
        if base_output.exists():
            # 如果直接有 DLL，就用这个
            if list(base_output.glob("*.dll")):
                return base_output

            # 否则找子目录（如 net10.0）
            subdirs = [d for d in base_output.iterdir() if d.is_dir()]
            if subdirs:
                # 返回最新的子目录（按修改时间）
                return max(subdirs, key=lambda d: d.stat().st_mtime)

        return base_output

    def _get_mod_name(self) -> str:
        """从 mod.toml 获取模组名称"""
        mod_toml = self.solution_dir / "mod.toml"
        if mod_toml.exists():
            with open(mod_toml, 'rb') as f:
                data = tomllib.load(f)
                return data['id']
        raise FileNotFoundError(f"未找到解决方案目录“{self.solution_dir}”下的 mod.toml文件")

    def _get_game_dir(self):
        """从环境变量获取游戏目录"""
        from dotenv import load_dotenv
        load_dotenv()
        return Path(os.environ.get('STS2_GAME_DIR'))

    def kill_game(self):
        """快速杀死正在运行的杀戮尖塔2进程"""
        import psutil
        import time

        if not self.game_exe or not self.game_exe.exists():
            print("⚠️  未配置游戏目录或找不到游戏可执行文件")
            return False

        game_name = f"{self.game_exe.stem}.exe".lower()
        game_path = str(self.game_exe).lower()
        killed = False

        # 单次快速扫描
        for proc in psutil.process_iter(['pid', 'name']):
            try:
                # 先快速检查进程名（最轻量）
                if proc.info['name'] and proc.info['name'].lower() == game_name:
                    proc.terminate()
                    killed = True
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue

        # 如果通过进程名没找到，再检查完整路径（更慢的fallback）
        if not killed:
            for proc in psutil.process_iter(['pid', 'exe']):
                try:
                    if proc.info['exe'] and proc.info['exe'].lower() == game_path:
                        proc.terminate()
                        killed = True
                except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
                    continue

        if killed:
            # 批量等待进程退出，使用短轮询
            timeout = 2.0
            start = time.time()
            while time.time() - start < timeout:
                try:
                    # 快速检查是否还有同名进程
                    still_alive = False
                    for proc in psutil.process_iter(['name']):
                        if proc.info['name'] and proc.info['name'].lower() == game_name:
                            still_alive = True
                            break
                    if not still_alive:
                        break
                    time.sleep(0.01)  # 短暂等待
                except:
                    break

            print("✅ 游戏进程已终止")
        else:
            print("ℹ️  未找到正在运行的游戏进程")

        return killed

    def build(self):
        """调用 dotnet build"""
        print("🔨 正在构建解决方案...")
        result = subprocess.run([
            "dotnet", "build", str(self.solution_dir),
            "-c", self.config,
            "-v", "minimal"
        ])
        if result.returncode != 0:
            print("❌ 构建失败")
            sys.exit(1)
        print("✅ 构建完成")

    def run_game(self, wait=False):
        """启动游戏
        
        Args:
            wait: 是否等待游戏退出。如果后续还要运行其他任务（如提取器），应设为 False
        """
        if not self.game_exe:
            print("⚠️  未配置游戏目录，请在 .env 中设置 STS2_GAME_DIR")
            return False

        if not self.game_exe.exists():
            print(f"❌ 找不到游戏可执行文件: {self.game_exe}")
            return False

        print(f"🎮 正在启动游戏: {self.game_exe}")

        # 使用 subprocess.Popen 启动游戏
        process = subprocess.Popen(
            [str(self.game_exe)],
            cwd=str(self.game_exe.parent),
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            creationflags=subprocess.CREATE_NEW_CONSOLE if sys.platform == 'win32' else 0
        )
        print("✅ 游戏已启动")

        if not wait:
            return True

        print("   等待游戏退出...")
        try:
            process.wait()
            print("✅ 游戏已退出")
        except KeyboardInterrupt:
            print("\n⚠️  收到中断信号，正在终止游戏...")
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
            print("✅ 游戏已终止")

    def run_extractor(self):
        """运行本地化文本提取器（在当前控制台）"""
        print("🔍 正在启动本地化文本提取器...")
        print("   提示: 按 Ctrl+C 可停止提取器")
        print("-" * 50)

        try:
            # 动态导入并运行提取器的主函数
            # 它会自动解析 sys.argv 中的 toml
            import STS2LocalizationTextExtractor as extractor_module

            extractor_module.main()

            return True
        except KeyboardInterrupt:
            print("\n" + "=" * 50)
            print("⏹️  提取器已停止")
            return True
        except Exception as e:
            print(f"❌ 运行提取器失败: {e}")
            import traceback
            traceback.print_exc()
            return False

    def process_assets(self):
        """处理资源"""
        from assets_processor import AssetOrchestrator

        print("🌐 正在处理资源文件...")

        # 确保暂存区存在
        if not self.staging_dir.exists():
            self.staging_dir.mkdir(parents=True)

        orchestrator = AssetOrchestrator(
            mod_id=self.mod_name,
            solution_dir=self.solution_dir,
            staging_dir=self.staging_dir
        )
        orchestrator.run()

    def pack(self, install=False):
        """打包 PCK（从输出目录取文件）"""
        from pack_pck import build_mod_pack, get_env_config

        print("📦 正在打包 PCK...")
        staging_dir = self.staging_dir

        # 输出文件都在 output_dir 中
        output_pck = self.output_dir / f"{self.mod_name}.pck"
        merged_dll = self.output_dir / f"{self.mod_name}.dll"

        # 检查必要文件
        if not merged_dll.exists():
            print(f"⚠️  找不到合并后的 DLL: {merged_dll}")
            print("  将尝试查找主 DLL...")
            main_dlls = list(self.output_dir.glob("*.Main.dll"))
            if main_dlls:
                merged_dll = main_dlls[0]
                print(f"  使用: {merged_dll.name}")
            else:
                print("❌ 找不到任何 DLL")
                sys.exit(1)

        # 获取 pck_tool 路径
        game_dir, pck_tool, _ = get_env_config()

        build_mod_pack(
            mod_toml=self.solution_dir / "mod.toml",
            output_pck=output_pck,
            dll_path=merged_dll,
            pck_src=staging_dir if staging_dir.exists() else None,
            pck_tool=pck_tool,
            game_dir=game_dir,
            copy_to_game=install
        )
        print("✅ PCK 打包完成")

    def clean(self):
        """清理构建产物"""
        print("🧹 正在清理...")

        # 清理输出目录中的 PCK 和合并 DLL
        for file in self.output_dir.glob("*.pck"):
            file.unlink()
            print(f"  已删除: {file}")

        merged_dll = self.output_dir / f"{self.mod_name}.dll"
        if merged_dll.exists():
            merged_dll.unlink()
            print(f"  已删除: {merged_dll}")

        # 也可以清理整个输出目录（可选）
        # if self.output_dir.exists():
        #     shutil.rmtree(self.output_dir)
        #     print(f"  已删除: {self.output_dir}")

        print("✅ 清理完成")

    def install(self):
        """仅安装（不打包），这是个语义化包装"""
        print("📋 正在安装模组...")
        self.pack(install=True)

    def print_start_banner(self, mode_name="构建"):
        print("=" * 60)
        print(f"🚀 开始执行模组任务 [{self.mod_name}] - 模式: {mode_name}")
        print(f"   解决方案: {self.solution_dir}")
        print(f"   配置: {self.config}")
        print(f"   暂存目录: {self.staging_dir}")
        print("=" * 60)

    def print_finish_banner(self):
        print("=" * 60)
        print("✨ 任务序列全部完成！")
        print(f"   输出目录: {self.output_dir}")
        if (self.output_dir / f"{self.mod_name}.pck").exists():
            print(f"   PCK 大小: {os.path.getsize(self.output_dir / f'{self.mod_name}.pck') // 1024} KB")
        print("=" * 60)


def main():
    parser = argparse.ArgumentParser(description='Superstitio 模组构建工具')
    parser.add_argument('--solution-dir', default='.', help='解决方案目录')
    parser.add_argument('--config', default='Debug', choices=['Debug', 'Release'], help='构建配置')

    # 操作模式（互斥组）
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument('--build-only', action='store_true', help='仅构建 DLL 与处理资源')
    mode.add_argument('--pack-only', action='store_true', help='仅处理资源并打包 PCK')
    mode.add_argument('--install-only', action='store_true', help='仅执行安装（打包并复制）')

    # 副作用操作
    parser.add_argument('--clean', action='store_true', help='执行任务前清理构建产物')
    parser.add_argument('--kill', action='store_true', help='构建前杀死游戏进程')
    parser.add_argument('--run', action='store_true', help='构建后启动游戏')
    parser.add_argument('--loc-extract', action='store_true', help='安装后运行本地化文本提取器（在当前控制台）')
    parser.add_argument('--toml', action='store_true', help='提取器使用 TOML 格式导出（需安装 tomlkit）')

    args = parser.parse_args()

    builder = ModBuilder(args.solution_dir, args.config)

    # 2. 编排任务流
    pipeline = []

    # --- 前置修饰任务 ---
    if args.clean:
        pipeline.append(builder.clean)
    if args.kill:
        pipeline.append(builder.kill_game)

    # --- 核心任务主体 ---
    if args.build_only:
        pipeline.extend([builder.build, builder.process_assets])
    elif args.pack_only:
        pipeline.extend([builder.process_assets, builder.pack])
    elif args.install_only:
        pipeline.append(builder.install)
    else:
        # 默认完整流程：构建 -> 资源处理 -> 打包并安装
        pipeline.extend([builder.build, builder.process_assets, builder.install])

    # 执行任务流
    # 确定当前执行模式的名称（用于 UI 显示）
    current_mode = "完整流程"
    if args.build_only:
        current_mode = "仅构建"
    elif args.pack_only:
        current_mode = "仅打包"
    elif args.install_only:
        current_mode = "仅安装"

    builder.print_start_banner(current_mode)

    try:
        for task in pipeline:
            # 打印每个小任务的开始提示
            task_name = task.__name__.replace('_', ' ').title()
            print(f"\n▶️ 执行任务: {task_name}...")
            task()
    except Exception as e:
        print(f"\n❌ 任务流执行失败: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)

    builder.print_finish_banner()

    # --- 后置副作用 ---
    if args.run:
        wait_for_game = not args.loc_extract
        builder.run_game(wait=wait_for_game)
    if args.loc_extract:
        builder.run_extractor()


if __name__ == "__main__":
    main()
