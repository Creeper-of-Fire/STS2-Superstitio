import os
import re
from pathlib import Path
import tomlkit
import inflection

# 一个很随意的脚本，用硬编码同步 TOML 文件到 C# 文件，使用时一定要注意

# ==========================================
# 配置：保持与 sync_loc.py 一致
# ==========================================
FOLDER_MAPPING = {
    "Lupa/Cards/Base": "cards/lupa.toml",
    "Lupa/Cards/Blood": "cards/lupa.toml",
    "Lupa/Cards/Rage": "cards/lupa.toml",
    "Maso/Cards/Base": "cards/maso.toml",
    "Maso/Cards/Self": "cards/maso.toml",
    "Maso/Cards/CotiKoki": "cards/cotikoki.toml",
}

SCRIPT_DIR = Path(__file__).parent
SRC_ROOT = SCRIPT_DIR.parent / "SlayTheSpire2Mod.Superstitio" / "Superstitio.Main"
OUT_ROOT = SRC_ROOT / "assets/Localization/zhs"

# 需要排除的键（不生成注释的）
EXCLUDED_KEYS = {}  # 可以根据需要添加更多


def convert_to_pascal_case(key: str) -> str:
    """
    将任意格式的键转换为 PascalCase（大写驼峰）
    
    支持的输入格式：
    - snake_case -> SnakeCase
    - camelCase -> CamelCase
    - kebab-case -> KebabCase
    - UPPER_CASE -> UpperCase
    - 混合格式 -> 自动处理
    """
    # 处理点分隔的嵌套键（如 power.description）
    if '.' in key:
        parts = key.split('.')
        return '.'.join(convert_to_pascal_case(part) for part in parts)

    # 使用 inflection 进行转换
    # 先转换为 snake_case，再转换为 PascalCase
    return inflection.camelize(inflection.underscore(key), uppercase_first_letter=True)


def load_toml_data(toml_path: Path) -> dict:
    """加载 TOML 文件并展平为点分隔的键值对"""
    if not toml_path.exists(): return {}
    with open(toml_path, 'r', encoding='utf-8-sig') as f:
        content = f.read().strip()
        return dict(tomlkit.parse(content)) if content else {}


def flatten_for_display(card_data: dict, parent_key: str = '') -> dict:
    """
    将嵌套字典展平用于显示（仅用于生成注释内容）
    例如: {"power": {"description": "xxx"}} -> {"power.description": "xxx"}
    """
    items = []
    for k, v in card_data.items():
        new_key = f"{parent_key}.{k}" if parent_key else k
        if isinstance(v, dict):
            items.extend(flatten_for_display(v, new_key).items())
        else:
            items.append((new_key, v))
    return dict(items)


def format_value(val: str, indent: str) -> str:
    val_str = str(val).strip()

    if not val_str:
        return '""'

    if "\n" in val_str or "\\n" in val_str:
        content = val_str.replace("\\n", "\n")
        lines = content.split('\n')
        formatted = '"""\n'
        for line in lines:
            line = line.strip()
            if line:
                formatted += f"{indent} * {line}\n"
            else:
                formatted += f"{indent} *\n"  # 空行无尾随空格
        formatted += f"{indent} * \"\"\""
        return formatted
    else:
        return f'"{val_str}"'


def build_block_comment(card_data: dict, indent: str) -> str:
    """
    根据卡片数据构建块注释
    自动检测所有键并转换为 PascalCase
    """
    if not card_data:
        return ""

    # 展平用于显示
    flat_data = flatten_for_display(card_data)

    items = []
    for key, val in flat_data.items():
        # 跳过需要排除的键
        if key in EXCLUDED_KEYS:
            continue
        # # 跳过空值
        # if not val:
        #     continue

        # 转换键名: title -> Title, power.description -> Power.Description
        display_name = '.'.join(convert_to_pascal_case(part) for part in key.split('.'))
        items.append((display_name, val))

    if not items:
        return ""

    lines = [f"{indent}/**"]
    for i, (name, val) in enumerate(items):
        lines.append(f"{indent} * {name} = {format_value(val, indent)}")
        if i < len(items) - 1:
            lines.append(f"{indent} *")  # 分隔行无尾随空格
    lines.append(f"{indent} */")
    return "\n".join(lines)


def process_cs_file(cs_file: Path, loc_data: dict):
    with open(cs_file, 'r', encoding='utf-8-sig') as f:
        lines = f.readlines()

    # 1. 定位类声明行及其对应的类名
    # 正则只匹配类定义这一行，不会引起回溯
    class_pattern = re.compile(r'^([ \t]*)(?:\[[^\]]*\]\s*)*(?:public|private|internal|sealed|partial|static|\s)+class\s+([A-Za-z0-9_]+)')

    matches = []
    for i, line in enumerate(lines):
        match = class_pattern.search(line)
        if match:
            indent = match.group(1)
            class_name = match.group(2)
            if class_name in loc_data:
                matches.append({
                    'line_idx': i,
                    'indent': indent,
                    'class_name': class_name
                })

    if not matches:
        return False

    # 2. 倒序处理，防止行号偏移
    new_lines = list(lines)
    for m in reversed(matches):
        idx = m['line_idx']
        class_name = m['class_name']
        indent = m['indent']

        # 向上扫描旧注释
        # 范围：从类声明行上方一行开始，直到遇到代码或空行超过一定限制

        scan_idx = idx - 1
        keep_line = []

        # 只要上方是 注释、Attribute 或 空行，就持续向上扫描
        last_useful_idx = idx

        in_multiline_comment = False

        while scan_idx >= 0:
            line_raw = new_lines[scan_idx]
            line_strip = line_raw.strip()

            # 1. 如果是空行，记录位置并继续（但不存入 keep_line）
            if not line_strip:
                last_useful_idx = scan_idx
                scan_idx -= 1
                continue

            # 2. 如果是 Attribute (例如 [Card])
            # 注意：简单的 startswith('[') 可能会误判，但在类上方通常是 Attribute
            if line_strip.startswith('[') and line_strip.endswith(']'):
                keep_line.insert(0, line_raw) # 保留这一行原始文本
                last_useful_idx = scan_idx
                scan_idx -= 1
                continue

            # 3. 如果是块注释结束
            if line_strip.endswith("*/"):
                in_block_comment = True
                last_useful_idx = scan_idx
                scan_idx -= 1
                continue

            # 4. 如果是块注释开始
            if line_strip.startswith("/*") or line_strip.startswith("/**"):
                in_block_comment = False
                last_useful_idx = scan_idx
                scan_idx -= 1
                continue

            # 5. 如果在块注释内部或者是单行注释
            if in_block_comment or line_strip.startswith("//") or line_strip.startswith("///") or line_strip.startswith("*"):
                last_useful_idx = scan_idx
                scan_idx -= 1
                continue

            # 6. 碰到其他任何东西（如代码、namespace、using），停止扫描
            break
            
        # 3. 构造新内容
        new_comment = build_block_comment(loc_data[class_name], indent)

        # 替换范围 [comment_start_idx : idx] 为新注释
        # 注意：要在注释上方留一个空行，如果它前面有代码的话
        replacement = []
        if last_useful_idx > 0 and new_lines[last_useful_idx - 1].strip():
            replacement.append("\n")
            
        # 放入新注释
        replacement.append(new_comment + "\n")

        # 放入保留下来的 Attributes
        if keep_line:
            replacement.extend(keep_line)

        # 替换范围 [last_useful_idx : idx]
        new_lines[last_useful_idx : idx] = replacement

    # 4. 写回文件
    with open(cs_file, 'w', encoding='utf-8-sig') as f:
        f.writelines(new_lines)
    return True


def sync():
    print(f"开始反向同步 (TOML -> C#).")
    for folder_rel_path, toml_name in FOLDER_MAPPING.items():
        folder_path = SRC_ROOT / folder_rel_path
        toml_path = OUT_ROOT / toml_name

        if not folder_path.exists() or not toml_path.exists():
            continue

        print(f"正在同步: {toml_name} -> {folder_rel_path}")
        data = load_toml_data(toml_path)

        count = 0
        for cs_file in folder_path.glob("*.cs"):
            if process_cs_file(cs_file, data):
                count += 1
        print(f"  完成: {count} 个文件已更新")


if __name__ == "__main__":
    sync()
