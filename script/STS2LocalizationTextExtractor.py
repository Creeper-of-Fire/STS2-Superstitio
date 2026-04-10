#!/usr/bin/env python3
# -----------------------------------------------------------------------------
# STS2 Localization Text Extractor
# 🚀 监控 Godot 日志，自动提取并分类由 BaseLib 监测并报告的缺失本地化项
#
# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire
#
# 功能说明:
# 1. 自动从日志提取由 BaseLib 的 MissingLocPatch 捕获并生成的缺失 Key 警告。
# 2. 支持 JSON/TOML 导出（--toml 模式）。
# 3. TOML 模式支持模组分包（Mod-Section.Key 结构）。
# -----------------------------------------------------------------------------

import os
import sys
import json
import time
import re
import signal
from pathlib import Path
from typing import Dict, Set, List, Tuple
from collections import deque
import logging
import inflection

from dotenv import load_dotenv
from rich.console import Console
from rich.live import Live
from rich.table import Table
from rich.panel import Panel
from rich.layout import Layout
from rich.text import Text
from rich import box
from rich.align import Align

USE_TOML_MODE = "--toml" in sys.argv

if USE_TOML_MODE:
    try:
        import tomlkit
    except ImportError:
        print("错误: --toml 模式需要第三方库 tomlkit。")
        print("请在终端运行: pip install tomlkit")
        sys.exit(1)

# 屏蔽标准日志输出到 console，避免干扰界面
logging.basicConfig(level=logging.CRITICAL)
console = Console()


class LocalizationExtractor:
    # 日志匹配模式
    PATTERN = re.compile(
        r'\[WARN\]\s+\[BaseLib\]\s+GetRawText:\s+Key\s+\'([^\']+)\'\s+not\s+found\s+in\s+table\s+\'([^\']+)\''
    )

    def __init__(self, log_file: str, output_dir: str):
        self.log_file = Path(log_file)
        self.output_dir = Path(output_dir)

        # 数据存储
        # JSON 模式下: Dict[table, Dict[key, str]]
        # TOML 模式下: Dict[(mod_name, table), Dict[section, Dict[key, str]]]
        self.missing_texts: Dict[Any, Any] = {}
        
        self.last_position = 0
        self.running = True

        # 数据存储
        self.recent_findings = deque(maxlen=10)  # 最近10个缺失项
        self.app_logs = deque(maxlen=8)  # 底部状态日志
        self.total_keys = 0
        self.start_time = time.time()

        self.output_dir.mkdir(parents=True, exist_ok=True)
        self.add_log(f"系统初始化管理目录: {self.output_dir}")
        self.load_existing_exports()

    def add_log(self, message: str, style: str = "white"):
        """添加一条应用日志"""
        ts = time.strftime("%H:%M:%S")
        self.app_logs.append(f"[{style}][{ts}] {message}[/]")

    def load_existing_exports(self):
        """加载已有的导出文件"""
        count = 0

        if USE_TOML_MODE:
            # TOML 模式：读取子文件夹中的 TOML
            for toml_file in self.output_dir.rglob("*_missing.toml"):
                mod_name = toml_file.parent.name
                table_name = toml_file.stem.replace("_missing", "")
                try:
                    with open(toml_file, 'r', encoding='utf-8') as f:
                        # --- 使用 .unwrap() 获取纯 Python 字典 ---
                        raw_data = tomlkit.load(f)
                        data = raw_data.unwrap() # 丢弃所有 tomlkit 的空行/格式对象

                        dict_key = (mod_name, table_name)
                        if dict_key not in self.missing_texts:
                            self.missing_texts[dict_key] = {}

                        for section, keys in data.items():
                            if section not in self.missing_texts[dict_key]:
                                self.missing_texts[dict_key][section] = {}
                            # 现在这里存的全部是纯 string，没有任何格式信息
                            self.missing_texts[dict_key][section].update(keys)
                            count += len(keys)
                except Exception as e:
                    self.add_log(f"加载失败 {toml_file.name}: {e}", "red")

            self.total_keys = count
            self.add_log(f"已加载现有 TOML 数据: {count} 条记录", "green")

        else:
            # JSON 模式
            for json_file in self.output_dir.glob("*_missing.json"):
                table_name = json_file.stem.replace("_missing", "")
                try:
                    with open(json_file, 'r', encoding='utf-8') as f:
                        data = json.load(f)
                        if table_name not in self.missing_texts:
                            self.missing_texts[table_name] = {}
                        self.missing_texts[table_name].update(data)
                        count += len(data)
                except Exception as e:
                    self.add_log(f"加载失败 {json_file.name}: {e}", "red")
            self.total_keys = count
            self.add_log(f"已加载现有 JSON 数据: {len(self.missing_texts)} 张表, {count} 条记录", "green")

    def add_missing_text(self, table: str, raw_key: str, force_ui_update: bool = False) -> bool:
        """
        添加缺失文本
        :param force_ui_update: 即使 Key 已存在，是否也更新到“最近发现”列表中
        """
        is_new_discovery = False

        if USE_TOML_MODE:
            # 解析 Key (例: SUPERSTITIO-DEFEND_MASO.description.xxx)
            mod_name = "DefaultMod"
            rest = raw_key

            # 1. 剥离模组名称
            if "-" in raw_key:
                mod_name, rest = raw_key.split("-", 1)

            # 2. 剥离节名称
            section = "DefaultSection"
            actual_key = rest
            if "." in rest:
                section, rest_key = rest.split(".", 1)
                # 将 section 转换为 camelCase（如 DESCRIPTION -> description, SMART_DESCRIPTION -> smartDescription）
                section = inflection.camelize(section.lower(), uppercase_first_letter=True)
            else:
                rest_key = rest
        
            # 3. 将 actual_key 也转换为 camelCase
            actual_key = inflection.camelize(rest_key.lower(), uppercase_first_letter=True)

            dict_key = (mod_name, table)
            if dict_key not in self.missing_texts:
                self.missing_texts[dict_key] = {}
            if section not in self.missing_texts[dict_key]:
                self.missing_texts[dict_key][section] = {}

            if actual_key not in self.missing_texts[dict_key][section]:
                self.missing_texts[dict_key][section][actual_key] = ""
                self.total_keys += 1
                is_new_discovery = True
                update_list = True
            else:
                update_list = force_ui_update

            display_table = f"{mod_name}/{table}"
            display_key = f"[{section}] {actual_key}"

        else:
            # 原版 JSON 逻辑
            if table not in self.missing_texts:
                self.missing_texts[table] = {}

            if raw_key not in self.missing_texts[table]:
                self.missing_texts[table][raw_key] = ""
                self.total_keys += 1
                is_new_discovery = True
                update_list = True
            else:
                update_list = force_ui_update

            display_table = table
            display_key = raw_key

        if update_list:
            timestamp = time.strftime("%H:%M:%S")
            new_entry = (timestamp, display_table, display_key)

            existing_items = [item for item in self.recent_findings if not (item[1] == display_table and item[2] == display_key)]
            self.recent_findings.clear()
            self.recent_findings.appendleft(new_entry)
            for item in existing_items:
                self.recent_findings.append(item)

        return is_new_discovery
    
    def open_output_dir(self):
        """跨平台打开文件夹"""
        try:
            path = str(self.output_dir.absolute())
            if platform.system() == "Windows":
                os.startfile(path)
            elif platform.system() == "Darwin":
                subprocess.Popen(["open", path])
            else:
                subprocess.Popen(["xdg-open", path])
            self.last_command_status = "[bold green]已打开输出目录[/]"
        except Exception as e:
            self.last_command_status = f"[bold red]打开失败: {e}[/]"

    def save_exports(self):
        """保存所有导出文件"""
        if USE_TOML_MODE:
            for (mod_name, table), sections in self.missing_texts.items():
                mod_dir = self.output_dir / mod_name
                mod_dir.mkdir(parents=True, exist_ok=True)
                toml_file = mod_dir / f"{table}_missing.toml"

                try:
                    # 使用 tomlkit 构建有序文档
                    # 1. 构造干净的嵌套字典（并排序）
                    clean_output = {}
                    for section_name in sorted(sections.keys()):
                        clean_output[section_name] = dict(sorted(sections[section_name].items()))

                    # 2. 生成 TOML 字符串
                    toml_string = tomlkit.dumps(clean_output)

                    # --- 正则清理多余空行 ---
                    # 将 3 个或以上的连续换行符替换为 2 个（即只保留一个空行）
                    # 同时清理行尾空格
                    import re
                    clean_toml = re.sub(r'\n{3,}', '\n\n', toml_string.strip())

                    with open(toml_file, 'w', encoding='utf-8') as f:
                        f.write(clean_toml + '\n')
                except Exception as e:
                    self.add_log(f"保存 TOML 出错 {mod_name}/{table}: {e}", "red")
        else:
            # 原版 JSON 逻辑
            for table, texts in self.missing_texts.items():
                json_file = self.output_dir / f"{table}_missing.json"
                try:
                    with open(json_file, 'w', encoding='utf-8') as f:
                        json.dump(dict(sorted(texts.items())), f, ensure_ascii=False, indent=2)
                except Exception as e:
                    self.add_log(f"保存出错 {table}: {e}", "red")

    def make_layout(self) -> Layout:
        """创建界面布局"""
        layout = Layout()
        layout.split(
            Layout(name="header", size=3),
            Layout(name="main", ratio=1),
            Layout(name="footer", size=12),
        )
        return layout

    def generate_ui(self) -> Layout:
        """生成实时 UI 内容"""
        layout = self.make_layout()

        # Header
        run_time = int(time.time() - self.start_time)
        header_text = Text.from_markup(
            f"🚀 [bold cyan]STS2 Localization Extractor[/] | 运行时间: {run_time}s | 监控文件: [dim]{self.log_file.name}[/]"
        )
        layout["header"].update(Panel(Align.center(header_text), border_style="cyan"))

        # Main Table (Recent Findings)
        table = Table(box=box.SIMPLE, expand=True, header_style="bold magenta")
        table.add_column("发现时间", style="dim", width=12)
        table.add_column("本地化表 (Table)", style="bold yellow")
        table.add_column("缺失键 (Key)", style="white")

        for ts, tbl, key in self.recent_findings:
            table.add_row(ts, tbl, key)

        # 补充空行保持表格高度稳定，防止抖动
        for _ in range(10 - len(self.recent_findings)):
            table.add_row("", "", "")

        layout["main"].update(Panel(table, title="[bold green] 最近发现的缺失文本 (Top 10) [/]", border_style="green"))

        # Footer (Stats & Logs)
        stats = Text.from_markup(
            f"📊 [bold]统计:[/] 总缺失数: [bold green]{self.total_keys}[/] | 表格总数: [bold blue]{len(self.missing_texts)}[/]\n"
        )
        log_content = Text.from_markup("\n".join(self.app_logs))

        layout["footer"].update(Panel(
            stats + Text("\n") + log_content,
            title="[bold] 系统状态与日志 [/]",
            border_style="bright_black"
        ))

        return layout

    def monitor(self):
        """主监控循环"""
        if not self.log_file.exists():
            console.print(f"[red]错误: 日志文件 {self.log_file} 不存在[/]")
            return

            # --- 提前开启 Live 界面 ---
            # 这样 Phase 1 的 add_log 就能被实时渲染出来
        with Live(self.generate_ui(), console=console, screen=True, refresh_per_second=4) as live:
            try:
                # --- 阶段 1: 初始扫描 ---
                self.add_log("正在解析现有日志文件...", "cyan")
                new_count = 0

                with open(self.log_file, 'r', encoding='utf-8', errors='ignore') as f:
                    lines = f.readlines()
                    self.last_position = f.tell()

                    for line in lines:
                        match = self.PATTERN.search(line)
                        if match:
                            if self.add_missing_text(match.group(2), match.group(1), force_ui_update=True):
                                new_count += 1

                        # 每处理 100 行更新一次 UI，防止大文件扫描时界面卡死
                        # if len(lines) > 1000: live.update(self.generate_ui())

                if new_count > 0:
                    self.save_exports()
                    self.add_log(f"初始扫描完成：发现 [bold white on green] {new_count} [/] 条新项并同步磁盘", "green")
                else:
                    self.add_log("初始扫描完成：未发现新缺失项", "dim")

                self.add_log(f"历史记录加载完毕 (最近 {len(self.recent_findings)} 条)", "blue")

                # --- 阶段 2: 实时监控循环 ---
                while self.running:
                    changed = False
                    if self.log_file.stat().st_size < self.last_position:
                        self.last_position = 0
                        self.add_log("日志文件已重置，重新读取", "yellow")

                    with open(self.log_file, 'r', encoding='utf-8', errors='ignore') as f:
                        f.seek(self.last_position)
                        new_lines = f.readlines()
                        self.last_position = f.tell()

                        if new_lines:
                            for line in new_lines:
                                match = self.PATTERN.search(line)
                                if match:
                                    if self.add_missing_text(match.group(2), match.group(1), force_ui_update=True):
                                        changed = True
                            if changed:
                                self.save_exports()

                        live.update(self.generate_ui())
                    time.sleep(0.5)

            except Exception:
                # 即使在 Live 模式下发生崩溃，也捕获它
                # 我们先退出 Live 模式，再打印堆栈
                live.stop()
                console.print_exception(show_locals=True)
                input("\n按回车键退出...")  # 保持窗口，防止直接消失

    def parse_line(self, line: str) -> bool:
        match = self.PATTERN.search(line)
        if match:
            return self.add_missing_text(match.group(2), match.group(1))
        return False

    def stop(self):
        self.running = False
        self.save_exports()


def main():
    load_dotenv()
    log_path = os.environ.get('STS2_LOG_FILE')
    out_path = os.environ.get('STS2_LOC_EXPORT_OUTPUT_DIR')

    if not log_path or not out_path:
        print("错误: 请在 .env 中设置环境变量")
        sys.exit(1)

    extractor = LocalizationExtractor(log_path, out_path)

    def signal_handler(sig, frame):
        extractor.stop()
        sys.exit(0)

    signal.signal(signal.SIGINT, signal_handler)

    try:
        extractor.monitor()
    except KeyboardInterrupt:
        extractor.stop()


if __name__ == '__main__':
    main()
