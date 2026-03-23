#!/usr/bin/env python3
# build_mod.py - 统一的模组构建脚本
# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire

import os
import sys

sys.dont_write_bytecode = True

import subprocess
import argparse
import shutil
import json
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

    def _get_mod_name(self):
        """从 mod.toml 获取模组名称"""
        mod_toml = self.solution_dir / "mod.toml"
        if mod_toml.exists():
            with open(mod_toml, 'rb') as f:
                data = tomllib.load(f)
                return data.get('id', 'Superstitio')
        return 'Superstitio'

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

    def run_game(self):
        """启动游戏"""
        if not self.game_exe:
            print("⚠️  未配置游戏目录，请在 .env 中设置 STS2_GAME_DIR")
            return False

        if not self.game_exe.exists():
            print(f"❌ 找不到游戏可执行文件: {self.game_exe}")
            return False

        print(f"🎮 正在启动游戏: {self.game_exe}")

        try:
            # 使用 subprocess.Popen 启动游戏，不等待退出
            subprocess.Popen(
                [str(self.game_exe)],
                cwd=str(self.game_exe.parent),
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
                creationflags=subprocess.CREATE_NEW_CONSOLE if sys.platform == 'win32' else 0
            )
            print("✅ 游戏已启动")
            return True
        except Exception as e:
            print(f"❌ 启动游戏失败: {e}")
            return False

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

    def process_localization(self):
        """处理本地化文件（可选）"""
        from process_localization import process_localization

        print("🌐 正在处理本地化文件...")

        pck_content = self.solution_dir / "pck_content"

        try:
            process_localization(
                solution_dir=self.solution_dir,
                mod_id=self.mod_name,
                output_dir=pck_content
            )
        except Exception as e:
            print(f"⚠️  本地化处理失败: {e}")
            # 继续执行，不退出

    def pack(self, install=False):
        """打包 PCK（从输出目录取文件）"""
        from pack_pck import build_mod_pack, get_env_config

        print("📦 正在打包 PCK...")
        script = self.script_dir / "pack_pck.py"
        mod_toml = self.solution_dir / "mod.toml"
        pck_content = self.solution_dir / "pck_content"

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
            pck_src=pck_content if pck_content.exists() else None,
            pck_tool=pck_tool,
            game_dir=game_dir,
            copy_to_game=install
        )
        print("✅ PCK 打包完成")

    def clean(self):
        """清理构建产物"""
        print("🧹 正在清理...")

        # 清理 pck_content
        pck_content = self.solution_dir / "pck_content"
        if pck_content.exists():
            shutil.rmtree(pck_content)
            print(f"  已删除: {pck_content}")

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
        """仅安装（不打包）"""
        print("📋 正在安装模组...")
        self.pack(install=True)

    def run_all(self, kill_first=False, run_after=False):
        """执行完整构建流程"""
        print("=" * 50)
        print(f"🚀 开始构建模组 [{self.mod_name}]")
        print(f"   解决方案: {self.solution_dir}")
        print(f"   配置: {self.config}")
        print("=" * 50)

        # 0. 可选的：先杀死游戏进程
        if kill_first:
            self.kill_game()
            print()

        # 1. 先构建（MSBuild 会负责 DLL 合并）
        self.build()

        # 2. 处理本地化（生成到 pck_content）
        self.process_localization()

        # 3. 打包 PCK（从 output_dir 取 DLL）
        self.pack(install=True)

        print("=" * 50)
        print("✨ 构建完成！")
        print(f"   输出目录: {self.output_dir}")
        print("=" * 50)

        # 4. 可选的：启动游戏
        if run_after:
            print()
            self.run_game()


def main():
    parser = argparse.ArgumentParser(description='Superstitio 模组构建工具')
    parser.add_argument('--solution-dir', default='.', help='解决方案目录')
    parser.add_argument('--config', default='Debug', choices=['Debug', 'Release'], help='构建配置')

    # 操作模式
    group = parser.add_mutually_exclusive_group()
    group.add_argument('--clean', action='store_true', help='清理构建产物')
    group.add_argument('--build-only', action='store_true', help='仅构建（不打包）')
    group.add_argument('--pack-only', action='store_true', help='仅打包（不构建）')
    group.add_argument('--install-only', action='store_true', help='仅安装（使用现有文件）')

    parser.add_argument('--kill', action='store_true', help='构建前杀死游戏进程')
    parser.add_argument('--run', action='store_true', help='构建后启动游戏')

    parser.add_argument('--loc-extract', action='store_true', help='安装后运行本地化文本提取器（在当前控制台）')
    parser.add_argument('--toml', action='store_true', help='提取器使用 TOML 格式导出（需安装 tomlkit）')

    args = parser.parse_args()

    builder = ModBuilder(args.solution_dir, args.config)

    if args.clean:
        builder.clean()
    elif args.build_only:
        if args.kill:
            builder.kill_game()
        builder.build()
        builder.process_localization()
        if args.run:
            builder.run_game()
    elif args.pack_only:
        if args.kill:
            builder.kill_game()
        builder.pack(install=True)
        if args.run:
            builder.run_game()
    elif args.install_only:
        if args.kill:
            builder.kill_game()
        builder.install()
        if args.run:
            builder.run_game()
    else:
        # 默认：完整流程，支持 --kill 和 --run
        builder.run_all(kill_first=args.kill, run_after=args.run)

    if args.loc_extract:
        builder.run_extractor()


if __name__ == "__main__":
    main()
