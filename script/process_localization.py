# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire

import os
import sys

sys.dont_write_bytecode = True

import json
from pathlib import Path
import tomllib
import argparse


def flatten_dict(d, parent_key='', sep='.'):
    """将嵌套字典扁平化"""
    items = []
    for k, v in d.items():
        new_key = parent_key + sep + k if parent_key else k
        if isinstance(v, dict):
            items.extend(flatten_dict(v, new_key, sep=sep).items())
        else:
            items.append((new_key, v))
    return dict(items)


def process_localization(solution_dir, mod_id, output_dir):
    solution_path = Path(solution_dir)
    output_path = Path(output_dir)
    print(f"扫描解决方案: {solution_path}")
    print(f"Mod ID: {mod_id}")

    # 1. 查找所有 Localization 目录
    localization_dirs = list(solution_path.glob("**/Localization"))
    if not localization_dirs:
        print("未找到任何 'Localization' 目录，跳过处理。")
        return

    # 2. 按 语言 -> 分类 聚合所有 TOML 文件
    aggregated_data = {}  # {lang: {category: {merged_toml_data}}}

    for loc_dir in localization_dirs:
        print(f"处理目录: {loc_dir}")
        for lang_path in loc_dir.iterdir():
            if not lang_path.is_dir(): continue
            lang = lang_path.name
            if lang not in aggregated_data:
                aggregated_data[lang] = {}

            for category_path in lang_path.iterdir():
                # 分类名统一转为小写，作为最终文件名
                category_name = category_path.stem.lower()
                if category_name not in aggregated_data[lang]:
                    aggregated_data[lang][category_name] = {}

                toml_files_to_process = []
                if category_path.is_dir():
                    toml_files_to_process.extend(category_path.glob("*.toml"))
                elif category_path.suffix == ".toml":
                    toml_files_to_process.append(category_path)

                for toml_file in toml_files_to_process:
                    try:
                        # 先以二进制模式读取，处理可能的 BOM
                        with open(toml_file, 'rb') as f:
                            content = f.read()
                
                            # 检查并移除 UTF-8 BOM
                            if content.startswith(b'\xef\xbb\xbf'):
                                content = content[3:]
                
                            # 尝试多种编码
                            for encoding in ['utf-8', 'utf-8-sig', 'gbk', 'gb2312', 'latin-1']:
                                try:
                                    decoded_content = content.decode(encoding)
                                    data = tomllib.loads(decoded_content)
                                    aggregated_data[lang][category_name].update(data)
                                    break
                                except (UnicodeDecodeError, tomllib.TOMLDecodeError):
                                    continue
                            else:
                                # 所有编码都失败
                                print(f"解析 TOML 文件失败（编码问题）: {toml_file}", file=sys.stderr)
                                sys.exit(1)
                
                    except Exception as e:
                        print(f"解析 TOML 文件失败: {toml_file}\n{e}", file=sys.stderr)
                        sys.exit(1)

    # 3. 转换并写入 JSON
    print("\n正在转换 TOML 为 JSON 并添加 ModID 前缀...")
    for lang, categories in aggregated_data.items():
        for category, data in categories.items():
            # 扁平化并添加 ModID 前缀
            final_json_data = {}
            flat_data = flatten_dict(data)
            for key, value in flat_data.items():
                final_json_data[f"{mod_id.upper()}-{key}"] = value

            # 确定输出路径并写入文件
            output_dir = output_path / mod_id / "localization" / lang
            output_dir.mkdir(parents=True, exist_ok=True)
            output_file = output_dir / f"{category}.json"

            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(final_json_data, f, ensure_ascii=False, indent=2)
            print(f"已生成: {output_file.relative_to(output_path)}")

    print("\n本地化处理完成！")


if __name__ == "__main__":
    # 使用 argparse 定义语义化参数
    parser = argparse.ArgumentParser(description="处理并合并 TOML 本地化文件为游戏可用的 JSON 格式。")

    parser.add_argument("--solution-dir", required=True,
                        help="要扫描以查找 Localization 文件夹的解决方案目录路径。")

    parser.add_argument("--mod-id", required=True,
                        help="将添加到所有本地化键前缀的 Mod ID。")

    parser.add_argument("--output-dir", required=True,
                        help="生成的 JSON 文件的基础输出目录（例如：pck_src）。")

    args = parser.parse_args()

    # 使用解析后的参数调用 main 函数
    process_localization(args.solution_dir, args.mod_id, args.output_dir)
