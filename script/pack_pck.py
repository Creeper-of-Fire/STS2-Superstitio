#!/usr/bin/env python3
# pack_pck.py - 资源/构建结果打包器
#
# 功能：将暂存区的文件打包成 Godot 的 PCK 包，并生成 mod.json 清单文件
# 依赖：需要 GodotPCKExplorer 工具
#
# 使用示例（命令行）：
#   python pack_pck.py --mod-toml mod.toml --output-pck output/mod.pck --pck-src .pck_staging
#
# 环境变量 (.env 文件)：
#   GODOT_PCK_EXPLORER   - GodotPCKExplorer.exe 的路径
#   STS2_GAME_DIR        - 游戏目录（用于 --copy-to-game）
#   COPY_TO_GAME         - 是否自动复制到游戏目录 (true/false)
#
# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire
import os
import sys

sys.dont_write_bytecode = True

import subprocess
import argparse
import shutil
from pathlib import Path
import re
import tomllib
import json
from dotenv import load_dotenv


def decode_godot_bytes(raw_bytes):
    """解码逻辑：处理GodotPCKExplorer特定字节分布的输出"""
    if not raw_bytes:
        return ""

    # 1. 找到第一个非零字节作为起始点
    start_idx = 0
    while start_idx < len(raw_bytes) and raw_bytes[start_idx] == 0:
        start_idx += 1

    # 2. 从第一个非零字节开始，每两个字节取第一个
    filtered_bytes = bytearray()
    i = start_idx
    while i < len(raw_bytes):
        filtered_bytes.append(raw_bytes[i])
        i += 2

    try:
        # 用 GBK 解码
        return filtered_bytes.decode('gbk', errors='ignore')
    except Exception:
        return ""


def filter_godot_output(process, verbose=False):
    """实时过滤 GodotPCKExplorer 的输出"""

    # 记录扫描到的最大文件数
    last_file_count = 0

    def should_show_and_process(line):
        nonlocal last_file_count
        line_clean = line.strip()
        if not line_clean:
            return False, None

        if verbose:
            return True, line_clean

        # 1. 拦截 Found files，只记数不打印
        count_match = re.search(r'Found files:\s+(\d+)', line_clean)
        if count_match:
            last_file_count = int(count_match.group(1))
            return False, None

        # 2. 拦截百分比、哈希、元数据等噪音
        noise_patterns = [
            r'\d+%',  # 进度百分比
            r'Calculated MD5:',  # MD5
            r'^\[.*?\]-\s+[0-9A-F\s]{2,}$',  # 哈希块
            r'Version:', r'Alignment:',  # 元数据
            r'Writing the PCK header',  # 过程琐碎描述
            r'generating an index',  # 过程琐碎描述
            r'Writing the content of files',  # 过程琐碎描述
            r'Starting\.',  # 琐碎开始标记
        ]
        for pattern in noise_patterns:
            if re.search(pattern, line_clean, re.IGNORECASE):
                return False, None

        # 3. 拦截单文件路径（.png, .import 等）
        if any(ext in line_clean.lower() for ext in ['.png', '.import', '.ctex', '.json']):
            return False, None

        # 4. 白名单：只有这些里程碑才准打印
        # 格式: (匹配正则, 是否替换为自定义中文, 是否显示原行)
        milestones = [
            (r'Started scanning', "  开始扫描文件...", False),
            (r'Scan completed!', None, True),
            (r'Pack PCK started', "  开始执行打包...", False),
            (r'Output file:', None, True),
            (r'Pack complete!', None, True),
            (r'Success|Error|Warning|Failed', None, True),
        ]

        for pattern, replacement, keep_original in milestones:
            if re.search(pattern, line_clean, re.IGNORECASE):
                # 提取时间戳 (如果有的话)
                timestamp_match = re.match(r'^(\[.*?\])', line_clean)
                ts = timestamp_match.group(1) + " " if timestamp_match else ""

                # 扫描完成时的特殊处理：合并统计数据
                if 'Scan completed!' in line_clean:
                    if last_file_count > 0:
                        msg = f"{ts}  >> 扫描完成: 共找到 {last_file_count} 个文件"
                        last_file_count = 0
                        return True, msg
                    return True, line_clean

                # 返回自定义中文或原行
                return True, (ts + replacement) if replacement else line_clean
            
        return True, line_clean

    last_progress_line = None

    while True:
        line_bytes = process.stdout.readline()
        if not line_bytes:
            break

        # 使用提取的原始解码逻辑
        decoded = decode_godot_bytes(line_bytes)

        for decoded_line in decoded.splitlines():
            show, final_line = should_show_and_process(decoded_line)
            if show:
                # 纯线性打印
                sys.stdout.write(final_line + '\n')
                sys.stdout.flush()

    # 处理错误输出
    stderr_bytes = process.stderr.read()
    if stderr_bytes:
        err_text = decode_godot_bytes(stderr_bytes)
        for line in err_text.splitlines():
            if line.strip():
                sys.stderr.write(line + '\n')
                sys.stderr.flush()


def load_mod_toml(toml_path):
    """加载 mod.toml 配置文件"""
    with open(toml_path, 'rb') as f:
        return tomllib.load(f)


def generate_mod_json(project_name, output_dir, toml_data):
    """根据 toml 数据生成模组 JSON 文件"""
    json_path = output_dir / f"{project_name}.json"

    # 检查 DLL 和 PCK 是否存在
    dll_exists = (output_dir / f"{project_name}.dll").exists()
    pck_exists = (output_dir / f"{project_name}.pck").exists()

    # 构建 JSON 数据
    mod_json = {
        "id": toml_data.get("id", project_name),
        "name": toml_data.get("name", project_name),
        "author": toml_data.get("author", "Unknown"),
        "description": toml_data.get("description", ""),
        "version": toml_data.get("version", "1.0.0"),
        "has_pck": pck_exists,
        "has_dll": dll_exists,
        "dependencies": toml_data.get("dependencies", []),
        "affects_gameplay": toml_data.get("affects_gameplay", True)
    }

    # 写入 JSON 文件
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(mod_json, f, ensure_ascii=False, indent=2)

    print(f"  生成: {json_path.name}")
    return json_path


def build_mod_pack(mod_toml, output_pck, dll_path=None, pck_src=None,
                   pck_tool=None, game_dir=None, copy_to_game=False):
    # 检查必要参数
    if not pck_tool:
        print("错误: 未指定 GodotPCKExplorer 路径", file=sys.stderr)
        sys.exit(1)

    pck_tool_path = Path(pck_tool)
    if not pck_tool_path.exists():
        print(f"错误: 找不到 GodotPCKExplorer: {pck_tool_path}", file=sys.stderr)
        sys.exit(1)

    # 处理 PCK 源目录（可选的）
    pck_src_path = None
    if pck_src:
        pck_src_path = Path(pck_src)
        if not pck_src_path.exists():
            print(f"错误: PCK源目录不存在: {pck_src_path}", file=sys.stderr)
            sys.exit(1)
    else:
        print("没有指定 PCK 源目录，跳过 PCK 打包")

    # 创建输出目录
    output_pck = Path(output_pck)
    output_pck.parent.mkdir(parents=True, exist_ok=True)

    # 项目名称（用于 JSON 和 DLL 文件名）
    project_name = output_pck.stem

    # 查找并加载 mod.toml
    mod_toml_path = Path(mod_toml)
    if not mod_toml_path.exists():
        print(f"错误: 找不到 mod.toml 文件: {mod_toml_path}", file=sys.stderr)
        sys.exit(1)

    print(f"找到 mod.toml: {mod_toml_path}")
    toml_data = load_mod_toml(mod_toml_path)

    # 验证必需字段
    required_fields = ["id", "name", "author", "version"]
    for field in required_fields:
        if field not in toml_data:
            print(f"错误: mod.toml 缺少必需字段: {field}", file=sys.stderr)
            sys.exit(1)

    # 打包 PCK（仅当有源目录时）
    if pck_src_path:
        print(f"正在打包 PCK: {output_pck.name}")
        print("-" * 50)

        # 调用 GodotPCKExplorer 并实时过滤输出
        cmd = [str(pck_tool_path), "-p", str(pck_src_path), str(output_pck), "3.4.5.1"]

        process = subprocess.Popen(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            shell=True
        )

        # 过滤输出
        filter_godot_output(process)
        process.wait()

        if process.returncode != 0:
            print(f"PCK打包失败，错误码: {process.returncode}", file=sys.stderr)
            sys.exit(1)

        print("-" * 50)
        print(f"PCK打包成功: {output_pck}")

    # 生成 mod JSON
    mod_json_path = generate_mod_json(project_name, output_pck.parent, toml_data)

    # 复制到游戏目录
    if copy_to_game and game_dir:
        if not Path(game_dir).exists():
            print(f"警告: 游戏目录不存在: {game_dir}", file=sys.stderr)
        else:
            project_name = output_pck.stem
            mod_dir = Path(game_dir) / "mods" / project_name

            print(f"正在安装到: {mod_dir}")
            mod_dir.mkdir(parents=True, exist_ok=True)

            # 复制 DLL - 检查是否存在
            dll_file = Path(dll_path)
            if dll_file.exists():
                shutil.copy2(dll_file, mod_dir)
                print(f"  复制: {dll_file.name}")

            # 复制对应的 PDB（如果存在）
            pdb_file = dll_file.with_suffix('.pdb')
            if pdb_file.exists():
                shutil.copy2(pdb_file, mod_dir)
                print(f"  复制: {pdb_file.name}")
            
            # 复制对应的 XML 文档（如果存在）
            xml_file = dll_file.with_suffix('.xml')
            if xml_file.exists():
                shutil.copy2(xml_file, mod_dir)
                print(f"  复制: {xml_file.name}")

            # 复制 PCK - 检查是否存在
            if output_pck.exists():
                shutil.copy2(output_pck, mod_dir)
                print(f"  复制: {output_pck.name}")

            # 复制生成的 JSON
            if mod_json_path.exists():
                shutil.copy2(mod_json_path, mod_dir)
                print(f"  复制: {mod_json_path.name}")

            print(f"模组已安装到: {mod_dir}")


def get_env_config(args=None):
    load_dotenv()

    # 获取配置（命令行参数优先，其次是.env）
    game_dir = args and args.game_dir or os.environ.get('STS2_GAME_DIR')
    pck_tool = args and args.pck_tool or os.environ.get('GODOT_PCK_EXPLORER')
    copy_to_game = args and args.copy_to_game or os.environ.get('COPY_TO_GAME', '').lower() == 'true'

    return game_dir, pck_tool, copy_to_game


def main():
    parser = argparse.ArgumentParser(description='打包 PCK 并安装模组')
    parser.add_argument('--game-dir', help='游戏目录（可选，会从.env读取）')
    parser.add_argument('--pck-tool', help='GodotPCKExplorer路径（可选，会从.env读取）')
    parser.add_argument('--copy-to-game', action='store_true', help='是否复制到游戏目录')
    parser.add_argument('--output-pck', required=True, help='输出的PCK文件路径')
    parser.add_argument('--mod-toml', required=True, help='mod.toml 配置文件路径')
    parser.add_argument('--dll-path', help='DLL文件的完整路径（可选）')
    parser.add_argument('--pck-src', help='PCK源目录路径（可选）')

    args = parser.parse_args()

    game_dir, pck_tool, copy_to_game = get_env_config(args)

    build_mod_pack(
        mod_toml=args.mod_toml,
        output_pck=args.output_pck,
        dll_path=args.dll_path,
        pck_src=args.pck_src,
        pck_tool=pck_tool,
        game_dir=game_dir,
        copy_to_game=copy_to_game
    )
    print("完成！")


if __name__ == "__main__":
    main()
