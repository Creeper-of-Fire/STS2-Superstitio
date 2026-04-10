import os
import re
from pathlib import Path
import tomlkit

# 一个很随意的脚本，用硬编码同步 TOML 文件到 C# 文件，使用时一定要注意

# ==========================================
# 配置：保持与 sync_loc.py 一致
# ==========================================
FOLDER_MAPPING = {
    "Lupa/Cards/Base": "cards/lupa.toml",
    "Lupa/Cards/Blood": "cards/lupa.toml",
    "Lupa/Cards/Rage": "cards/lupa.toml",
    "Maso/Cards/Base": "cards/maso.toml",
    "Maso/Cards/CotiKoki": "cards/cotikoki.toml",
}

SCRIPT_DIR = Path(__file__).parent
SRC_ROOT = SCRIPT_DIR.parent / "SlayTheSpire2Mod.Superstitio" / "Superstitio.Main"
OUT_ROOT = SRC_ROOT / "assets/Localization/zhs"

KEY_DISPLAY_MAP = {
    "title": "Title",
    "description": "Description",
    "hangingEffect": "HangingEffect",
    "power.description": "Power.Description",
    "power.smartDescription": "Power.SmartDescription",
    "flavor": "Flavor",
}


def load_toml_data(toml_path: Path) -> dict:
    if not toml_path.exists(): return {}
    with open(toml_path, 'r', encoding='utf-8-sig') as f:
        content = f.read().strip()
        return dict(tomlkit.parse(content)) if content else {}


def format_value(val: str, indent: str) -> str:
    val_str = str(val).strip()
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
    items = []
    for k, display_name in KEY_DISPLAY_MAP.items():
        val = None
        if "." in k:
            parts = k.split('.')
            val = card_data.get(parts[0], {}).get(parts[1])
        else:
            val = card_data.get(k)
        if val:
            items.append((display_name, val))

    if not items: return ""

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
        comment_start_idx = idx
        in_multiline_comment = False

        for j in range(idx - 1, -1, -1):
            prev_line = new_lines[j].strip()

            # 如果是空行，继续向上找（但我们要清理掉它）
            if not prev_line:
                comment_start_idx = j
                continue

            # 匹配块注释结束
            if prev_line.endswith("*/"):
                in_multiline_comment = True
                comment_start_idx = j
                continue
            # 匹配块注释开始
            if prev_line.startswith("/*") or prev_line.startswith("/**"):
                in_multiline_comment = False
                comment_start_idx = j
                continue
            # 在块注释内部
            if in_multiline_comment:
                comment_start_idx = j
                continue
            # 匹配单行注释
            if prev_line.startswith("///") or prev_line.startswith("//"):
                comment_start_idx = j
                continue

            # 如果碰到既不是注释也不是空行的东西，停止扫描
            break

        # 3. 构造新内容
        new_comment = build_block_comment(loc_data[class_name], indent)

        # 替换范围 [comment_start_idx : idx] 为新注释
        # 注意：要在注释上方留一个空行，如果它前面有代码的话
        replacement = []
        if comment_start_idx > 0 and new_lines[comment_start_idx - 1].strip():
            replacement.append("\n")  # 补一个空行

        replacement.append(new_comment + "\n")

        # 执行切片替换
        new_lines[comment_start_idx: idx] = replacement

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
