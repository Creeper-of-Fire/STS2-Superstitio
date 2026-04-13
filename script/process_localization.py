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
        :param data: 聚合字典，格式为 {virtual_path: {top_key: {nested_data}}}
                     注意：这里的 virtual_path 可以是相对路径（如 'cards', 'sfw/cards'），
                     框架会根据这个路径自动在输出目录中建立相应的子目录。
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

class VariantSplitter(BaseProcessor):
    """
    变体分离器：
    专门识别特定前缀（如 sfw, nsfw, guro），将其数据从主分类中抽取出来，
    并重定向到新的目录地址（如从 cards 重定向到 sfw/cards）。
    """
    VARIANTS = {"sfw", "nsfw", "guro"}

    def process(self, data: Dict[str, Any], context: LocalizationContext) -> Dict[str, Any]:
        # 使用 list 包装 items 以允许在迭代时修改源字典
        for virtual_path, cat_data in list(data.items()):
            for top_key, fields in cat_data.items():
                if not isinstance(fields, dict):
                    continue

                for variant in self.VARIANTS:
                    # 场景 1：标准 TOML 嵌套。例如 sfw.title = "..." 会被解析为 {"sfw": {"title": "..."}}
                    if variant in fields and isinstance(fields[variant], dict):
                        variant_data = fields.pop(variant)
                        self._redirect_to_variant_path(data, virtual_path, variant, top_key, variant_data)

                    # 场景 2：后备方案，防御扁平字符串键。例如用户带引号写了 "sfw.title" = "..."
                    keys_to_delete = []
                    for k, v in fields.items():
                        if k.startswith(f"{variant}."):
                            sub_key = k[len(variant) + 1:]
                            self._redirect_to_variant_path(data, virtual_path, variant, top_key, {sub_key: v})
                            keys_to_delete.append(k)

                    for k in keys_to_delete:
                        fields.pop(k)

        return data

    def _redirect_to_variant_path(self, data, base_path, variant, top_key, variant_data):
        """修改目录地址：通过赋予新的虚拟路径将数据重定向"""
        # 构建新的相对目录，例如 'cards' -> 'sfw/cards'
        variant_path = f"{variant}/{base_path}"

        if variant_path not in data:
            data[variant_path] = {}
        if top_key not in data[variant_path]:
            data[variant_path][top_key] = {}

        data[variant_path][top_key].update(variant_data)


class CardPowerExtractor(BaseProcessor):
    """
    特殊处理：
    对于 cards 类别，如果某个卡牌下包含 power 字段，
    则将其提取到 powers 类别下，键名加上 'Power' 后缀。
    """

    def process(self, data: Dict[str, Any], context: LocalizationContext) -> Dict[str, Any]:
        # 匹配任何以 'cards' 结尾的虚拟路径 (例如 'cards', 'sfw/cards')
        categories_to_process = [path for path in data.keys() if path.split('/')[-1] == 'cards']

        for cat_path in categories_to_process:
            cards_category = data[cat_path]
            if not cards_category:
                continue

            # 推导对应的 powers 路径 (例如 'sfw/cards' -> 'sfw/powers')
            prefix = cat_path[:-len('cards')]
            powers_cat_path = f"{prefix}powers"
            powers_category = data.setdefault(powers_cat_path, {})

            for card_id, card_data in list(cards_category.items()):
                if isinstance(card_data, dict):
                    power_fields = [key for key in card_data.keys() if key.lower().endswith('power')]

                    for power_field in power_fields:
                        power_data = card_data.pop(power_field)
                        power_id = f"{card_id}Power" if power_field.lower() == 'power' else inflection.camelize(power_field)

                        if power_id not in powers_category:
                            powers_category[power_id] = {}

                        if isinstance(power_data, dict):
                            powers_category[power_id].update(power_data)
                        else:
                            powers_category[power_id]["NAME"] = power_data

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
                            except (UnicodeDecodeError):
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
    # 第一次分离：处理根节点上的 sfw.title 和 sfw.power
    pipeline.add_processor(VariantSplitter())
    # 提取能力：扫描 cards 和提取出的 sfw/cards，将其拆分至 powers 和 sfw/powers
    pipeline.add_processor(CardPowerExtractor())
    # 第二次分离：处理诸如 power.sfw.name 这种因 Extractor 提升了层级而暴露出的变体
    pipeline.add_processor(VariantSplitter())
    # 统一添加 Mod 前缀
    pipeline.add_processor(ModIdPrefixer())

    processed_data = {}
    for lang, lang_data in aggregated_data.items():
        ctx = LocalizationContext(mod_id=mod_id, lang=lang)
        # 流水线一次性处理当前语言下的所有分类数据
        processed_data[lang] = pipeline.execute(lang_data, ctx)

    # 最终扁平化并写入 JSON
    print("正在扁平化数据并生成 JSON...")
    for lang, categories_data in processed_data.items():
        # Step 1: 全部转化为扁平化字典
        flattened_data = {}
        for virtual_path, cat_data in categories_data.items():
            f_data = flatten_dict(cat_data)
            if f_data:
                flattened_data[virtual_path] = f_data

        # Step 2: 处理继承链合并
        # 定义继承顺序
        variants_order = ['nsfw', 'guro', 'sfw']

        # 找出当前语言中涉及的所有分类（如 cards, powers, relics）
        all_categories = set()
        for p in flattened_data.keys():
            all_categories.add(p.split('/')[-1])

        for cat in all_categories:
            # 获取根基底 (无前缀的文件内容)
            # 如果没有根基底，则从空字典开始
            current_base = flattened_data.get(cat, {}).copy()

            # 按顺序迭代：SFW -> NSFW -> Guro
            for var in variants_order:
                var_path = f"{var}/{cat}"

                # 继承逻辑：
                # 1. 复制上一级的累积内容作为底色
                # 2. 如果当前变体文件夹有特殊定义，则覆盖上去
                # 3. 更新累积内容供下一级继承

                if var_path in flattened_data:
                    # 如果变体有定义，将变体内容覆盖到基底上
                    merged_content = current_base.copy()
                    merged_content.update(flattened_data[var_path])
                    flattened_data[var_path] = merged_content
                    # 下一级将继承这个合并后的结果
                    current_base = merged_content
                else:
                    # 如果变体没有定义该文件，则它完全等同于上一级的内容
                    # 这样可以保证 sfw/cards.json 不存在时，nsfw/cards.json 也能拿到基础数据
                    flattened_data[var_path] = current_base.copy()

        # Step 3: 开始写入磁盘
        for virtual_path, final_json_data in flattened_data.items():
            # 确定输出路径并写入文件
            output_file = output_path / mod_id / "localization" / lang / f"{virtual_path}.json"
            output_file.parent.mkdir(parents=True, exist_ok=True)

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
