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
from abc import ABC, abstractmethod
from typing import Dict, Any


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


# ==========================================
# 面向对象的本地化处理流水线
# ==========================================

class LocalizationContext:
    """提供给处理器的上下文信息"""

    def __init__(self, mod_id: str, lang: str):
        self.mod_id = mod_id
        self.lang = lang


class BaseProcessor(ABC):
    """处理器基类"""

    @abstractmethod
    def process(self, data: Dict[str, Any], context: LocalizationContext) -> Dict[str, Any]:
        """
        处理本地化数据
        :param data: 当前语言的所有数据聚合字典，格式为 {category: {top_key: {nested_data}}}
        :param context: 上下文（含 mod_id, lang 等）
        :return: 处理后的字典
        """
        pass


class LocalizationPipeline:
    """处理流水线"""

    def __init__(self):
        self.processors = []

    def add_processor(self, processor: BaseProcessor):
        self.processors.append(processor)
        return self

    def execute(self, data: Dict[str, Any], context: LocalizationContext) -> Dict[str, Any]:
        for processor in self.processors:
            data = processor.process(data, context)
        return data


# ==========================================
# 具体处理器实现
# ==========================================

class CardPowerExtractor(BaseProcessor):
    """
    特殊处理：
    对于 cards 类别，如果某个卡牌下包含 power 字段，
    则将其提取到 powers 类别下，键名加上 'Power' 后缀。
    """

    def process(self, data: Dict[str, Any], context: LocalizationContext) -> Dict[str, Any]:
        # 确保全部为小写分类名
        cards_category = data.get("cards", {})
        if not cards_category:
            return data

        # 确保 powers 类别存在
        powers_category = data.setdefault("powers", {})

        for card_id, card_data in cards_category.items():
            if isinstance(card_data, dict) and "power" in card_data:
                # 提取 power 数据并从原来的卡牌中移除
                power_data = card_data.pop("power")

                power_id = f"{card_id}Power"
                # 如果该 power_id 已经存在，则合并；否则新建
                if power_id not in powers_category:
                    powers_category[power_id] = {}
                powers_category[power_id].update(power_data)

        return data


class ModIdPrefixer(BaseProcessor):
    """
    基础处理：
    将第一段 Key 转换为 UPPER_SNAKE_CASE，并加上 ModID 前缀。
    """

    def process(self, data: Dict[str, Any], context: LocalizationContext) -> Dict[str, Any]:
        new_data = {}
        for category, cat_data in data.items():
            new_cat_data = {}
            for top_key, val in cat_data.items():
                # top_key 是最顶级的键，例如 "DefendMaso"
                top_key_upper_snake = inflection.underscore(top_key).upper()
                new_top_key = f"{context.mod_id.upper()}-{top_key_upper_snake}"

                new_cat_data[new_top_key] = val
            new_data[category] = new_cat_data

        return new_data


def process_localization(assets_dir, mod_id, output_dir):
    loc_path = Path(assets_dir)
    if loc_path.name != "localization":  # 兼容性检查
        loc_path = loc_path / "localization"

    if not loc_path.exists():
        print(f"本地化目录不存在: {loc_path}")
        return

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

    # 构建并执行流水线
    print("正在执行数据处理流水线...")
    pipeline = LocalizationPipeline()
    # 注册你的处理器（执行顺序很重要，先提取，再加前缀）
    pipeline.add_processor(CardPowerExtractor())
    pipeline.add_processor(ModIdPrefixer())

    processed_data = {}
    for lang, lang_data in aggregated_data.items():
        ctx = LocalizationContext(mod_id=mod_id, lang=lang)
        # 流水线一次性处理当前语言下的所有分类数据
        processed_data[lang] = pipeline.execute(lang_data, ctx)

    # 最终扁平化并写入 JSON
    print("正在扁平化数据并生成 JSON...")
    for lang, categories_data in processed_data.items():
        for category, cat_data in categories_data.items():
            # 在这里进行最后的扁平化
            # 经过 ModIdPrefixer，此时字典的顶层 Key 已经是加了前缀的格式，如 MODID-DEFEND_MASO
            final_json_data = flatten_dict(cat_data)

            # 如果分类没有任何数据（可能是空文件被处理），跳过
            if not final_json_data:
                continue

            # 确定输出路径并写入文件
            out_dir = output_path / mod_id / "localization" / lang
            out_dir.mkdir(parents=True, exist_ok=True)
            output_file = out_dir / f"{category}.json"

            # 如果文件已存在，加载旧内容进行合并
            existing_data = {}
            if output_file.exists():
                with open(output_file, 'r', encoding='utf-8') as f:
                    existing_data = json.load(f)

            # 合并当前数据
            existing_data.update(final_json_data)

            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(existing_data, f, ensure_ascii=False, indent=2)
            print(f"已生成/更新: {output_file.relative_to(output_path)}")

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
