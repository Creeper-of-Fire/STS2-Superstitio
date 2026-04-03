# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire

import os
import sys

sys.dont_write_bytecode = True

import json
from pathlib import Path
import tomllib
import argparse
import inflection


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


def process_localization(assets_dir, mod_id, output_dir):
    loc_path = Path(assets_dir)
    if loc_path.name != "localization":  # 兼容性检查
        loc_path = loc_path / "localization"

    if not loc_path.exists(): return

    output_path = Path(output_dir)

    # 按 语言 -> 分类 聚合所有 TOML 文件
    aggregated_data = {}  # {lang: {category: {merged_toml_data}}}

    for lang_path in loc_path.iterdir():
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
                # key 格式如 "DefendMaso.title" 或 "DefendMaso.description"
                # 分割为第一段和剩余部分
                parts = key.split('.', 1)
                if len(parts) == 2:
                    first_part, rest = parts
                    # 将第一段转换为 UPPER_SNAKE_CASE
                    first_part_upper_snake = inflection.underscore(first_part).upper()
                    new_key = f"{mod_id.upper()}-{first_part_upper_snake}.{rest}"
                else:
                    # 如果没有点，则整个处理
                    new_key = f"{mod_id.upper()}-{inflection.underscore(parts[0]).upper()}"
                
                final_json_data[new_key] = value

            # 确定输出路径并写入文件
            output_dir = output_path / mod_id / "localization" / lang
            output_dir.mkdir(parents=True, exist_ok=True)
            output_file = output_dir / f"{category}.json"

            # 如果文件已存在，加载旧内容进行合并
            existing_data = {}
            if output_file.exists():
                with open(output_file, 'r', encoding='utf-8') as f:
                    existing_data = json.load(f)

            # 合并当前数据
            existing_data.update(final_json_data)

            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(existing_data, f, ensure_ascii=False, indent=2)
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
