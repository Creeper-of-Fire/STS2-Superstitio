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

    def run_all(self):
        """执行完整构建流程"""
        print("=" * 50)
        print(f"🚀 开始构建模组 [{self.mod_name}]")
        print(f"   解决方案: {self.solution_dir}")
        print(f"   配置: {self.config}")
        print("=" * 50)

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

    args = parser.parse_args()

    builder = ModBuilder(args.solution_dir, args.config)

    if args.clean:
        builder.clean()
    elif args.build_only:
        builder.build()
        builder.process_localization()
    elif args.pack_only:
        builder.pack(install=True)
    elif args.install_only:
        builder.install()
    else:
        # 默认：完整流程
        builder.run_all()


if __name__ == "__main__":
    main()
