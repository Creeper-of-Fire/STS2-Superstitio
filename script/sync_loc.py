import os
import re
from pathlib import Path
import tomlkit
from tomlkit import key as toml_key
from tomlkit import string as toml_string
from collections import defaultdict

# ==========================================
# 配置：维护 .cs 文件夹与 .toml 文件的映射关系
# 键：C# 代码相对于项目根目录的相对路径 (或其子目录)
# 值：生成的 TOML 路径 (相对于 assets/Localization/zhs)
# ==========================================
FOLDER_MAPPING = {
    "Lupa/Cards/Base": "cards/lupa.toml",
    "Lupa/Cards/Blood": "cards/lupa.toml",
    "Lupa/Cards/Rage": "cards/lupa.toml",
    "Maso/Cards/Base": "cards/maso.toml",
    "Maso/Cards/CotiKoki": "cards/cotikoki.toml",
}

# 项目根目录设置 (Superstitio.Main)
SRC_ROOT = Path("../SlayTheSpire2Mod.Superstitio/Superstitio.Main")
OUT_ROOT = SRC_ROOT / "assets/Localization/zhs"


def transform_key(tag_name: str) -> str:
    """
    处理 XML 标签名转换:
    1. 统一小写首字母
    2. 处理带点号的嵌套 (Power.Description -> power.description)
    """

    parts = tag_name.split('.')
    # 每一段的首字母小写
    transformed_parts = [p[0].lower() + p[1:] if p else "" for p in parts]
    return ".".join(transformed_parts)


def flatten_toml_data(data, prefix=""):
    """将嵌套的 TOML Table 平铺为点分键格式"""
    items = {}
    for k, v in data.items():
        new_key = f"{prefix}.{k}" if prefix else k
        if isinstance(v, dict):
            items.update(flatten_toml_data(v, new_key))
        else:
            items[new_key] = v
    return items


def parse_cs_block_comments(filepath: Path):
    results = {}
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        content = f.read()

    # 1. 匹配 /** ... */ 块
    # 2. \s* 匹配空格或换行
    # 3. (?:\[[^\]]*\]\s*)* 匹配任意数量的 [Attribute]
    # 4. (?:public|private|internal|sealed|partial|static|\s)+ 匹配类修饰符
    # 5. class\s+([A-Za-z0-9_]+) 匹配关键字 class 和类名
    pattern = r'/\*\*(.*?)\*/\s*(?:\[[^\]]*\]\s*)*(?:public|private|internal|sealed|partial|static|\s)+class\s+([A-Za-z0-9_]+)'

    blocks = re.findall(pattern, content, re.DOTALL)

    for comment_text, class_name in blocks:
        class_data = {}

        # 清理掉每行开头的星号 '*'
        lines = []
        for line in comment_text.split('\n'):
            line = line.strip()
            if line.startswith('*'):
                line = line[1:].strip()
            if line:
                lines.append(line)
        cleaned_block = "\n".join(lines)

        # --- 使用统一正则按顺序捕获 ---
        # 匹配 Key = """...""" OR Key = "..."
        # Group 1: Key
        # Group 2: Value (包括引号)
        assignment_pattern = r'([A-Za-z0-9_.]+)\s*=\s*("""[\s\S]*?"""|"[^"]*")'
        matches = re.findall(assignment_pattern, cleaned_block)

        for k, v in matches:
            key = transform_key(k)
            # 去掉前后的引号
            if v.startswith('"""'):
                val = v[3:-3].strip()
            else:
                val = v[1:-1].strip()

            class_data[key] = val

        if class_data:
            results[class_name] = class_data

    return results


def update_toml_table(target_table, tags_dict):
    """
    增量更新 Table 内容：
    1. 如果键已存在，更新值。
    2. 如果键不存在，添加值。
    3. 如果 TOML 中原有的键在 tags_dict 中没有，不做处理（即保留）。
    """
    for k_str, v in tags_dict.items():
        # 处理多行字符串格式
        if "\n" in v:
            # val = toml_string(v, multiline=True)
            val = v
        else:
            val = v

        if "." in k_str:
            # 处理嵌套键，如 "power.description"
            parts = k_str.split(".")
            curr = target_table
            for i, part in enumerate(parts[:-1]):
                if part not in curr or not isinstance(curr[part], dict):
                    curr[part] = tomlkit.table()
                curr = curr[part]
            curr[parts[-1]] = val
        else:
            # 普通键
            target_table[k_str] = val


def merge_and_sort_keys(old_keys, new_keys,  class_name):
    """
    实现 ABCDE + AEXB = AEXCDB 的排序逻辑
    """
    shared_in_new = [k for k in new_keys if k in old_keys]
    toml_only = [k for k in old_keys if k not in new_keys]

    if toml_only:
        print(f"  [提示] 类 {class_name} 在 TOML 中存在多余键: {toml_only}")

    # 1. 构建中间序列：将 TOML 原顺序中的共有键按 CS 出现的顺序替换
    # 例如：ABCDE 中 A,B,E 是共有键，在 CS 里顺序是 A,E,B
    # 替换后得到：A E C D B
    intermediate_list = []
    shared_ptr = 0
    for k in old_keys:
        if k in shared_in_new:
            intermediate_list.append(shared_in_new[shared_ptr])
            shared_ptr += 1
        else:
            intermediate_list.append(k)

    # 2. 处理 CS 中完全新增的键 (cs_only)
    # 按照 CS 里的顺序，将新键插入到它前驱键的后面
    final_list = intermediate_list[:]
    for i, k in enumerate(new_keys):
        if k not in old_keys:
            # 找到它在 CS 里的前驱位置
            if i == 0:
                final_list.insert(0, k)
            else:
                prev_key = new_keys[i - 1]
                # 插入到 final_list 中 prev_key 的后面
                idx = final_list.index(prev_key)
                final_list.insert(idx + 1, k)

    return final_list


def sync():
    # 准备聚合数据
    # { "cards/lupa.toml": { "xxx": {...}, "yyy": {...} } }
    toml_bundles = defaultdict(dict)

    print(f"开始扫描目录: {SRC_ROOT}")

    for folder_rel_path, target_toml in FOLDER_MAPPING.items():
        folder_path = SRC_ROOT / folder_rel_path
        if not folder_path.exists():
            print(f"跳过不存在的目录: {folder_rel_path}")
            continue

        for cs_file in folder_path.glob("*.cs"):
            # print(f"正在处理: {cs_file}")
            data = parse_cs_block_comments(cs_file)
            # print(f"  已找到 {len(data)} 个类")
            if data:
                toml_bundles[target_toml].update(data)

    # 写入 TOML
    for toml_rel_path, content in toml_bundles.items():
        target_path = OUT_ROOT / toml_rel_path
        target_path.parent.mkdir(parents=True, exist_ok=True)

        # 加载现有文件（如果存在）以合并，防止丢失手动维护的非代码文本
        doc = tomlkit.document()
        if target_path.exists():
            with open(target_path, 'r', encoding='utf-8-sig') as f:
                raw_content = f.read().strip()
                if raw_content:  # 只有内容非空时才解析
                    try:
                        doc = tomlkit.parse(raw_content)
                    except Exception as e:
                        print(f"解析 {target_path} 失败，将创建新文件。错误: {e}")
                        doc = tomlkit.document()

        # 更新内容
        for class_id, cs_tags in content.items():
            # 获取旧数据并平铺化
            old_raw_data = doc.get(class_id, {})
            old_flat_dict = flatten_toml_data(old_raw_data)
            old_keys = list(old_flat_dict.keys())

            new_keys = list(cs_tags.keys())

            # 计算合并后的平铺顺序
            sorted_keys = merge_and_sort_keys(old_keys, new_keys, class_id)

            # 合并实际内容
            merged_values = {**old_flat_dict, **cs_tags}

            # 创建 Table，但使用 dotted keys 风格
            new_table = tomlkit.table()
            for k in sorted_keys:
                v = merged_values[k]
                # val = toml_string(v, multiline=True) if "\n" in v else v
                val = v

                if "." in k:
                    # 只有包含点号的键才使用 toml_key 列表模式以生成 dotted key (a.b = ...)
                    new_table.add(toml_key(k.split('.')), val)
                else:
                    # 普通键直接赋值，避免 tomlkit 内部 NonExistentKey 错误
                    new_table[k] = val

            doc[class_id] = new_table

        with open(target_path, 'w', encoding='utf-8') as f:
            f.write(tomlkit.dumps(doc))
            print(f"同步完成 -> {toml_rel_path} ({len(content)} classes)")


if __name__ == "__main__":
    sync()
