#!/usr/bin/env python3
# png2ctex.py - PNG转Godot 4.5 .ctex格式
#
# 功能：将 PNG 图片转换为 Godot 4.5 的 .ctex 纹理格式，并生成 .import 元数据文件
#
# 使用示例：
#   from png2ctex import CtexConverter
#   
#   # 方式1：直接转换
#   ctex_data, ctex_name = CtexConverter.convert(Path("card.png"), "res://MyMod/images/card.png")
#   
#   # 方式2：一站式获取需要打包的文件条目
#   entries = CtexConverter.convert_and_get_entries(Path("card.png"), "MyMod", "images/card.png")
#   # entries = [(虚拟路径, 数据), ...] 包含 .ctex 和 .import 两个文件
#
# 命令行测试：
#   python png2ctex.py card.png
#
#
# 基于BSchneppe (benedikt.schneppe+resume@gmail.com) 逆向工程得到的格式而实现
# Contributed by: Creeper-of-Fire
# GitHub: https://github.com/Creeper-of-Fire

from PIL import Image
import hashlib
import struct
from pathlib import Path
from typing import Tuple, Optional


class CtexConverter:
    """PNG到.ctex转换器"""

    # GST2头部结构 (56字节)
    HEADER_FORMAT = '<4s' + 'I' * 4 + 'i' + 'I' * 8  # 小端序，14个字段
    MAGIC = b'GST2'
    VERSION = 1
    FLAGS = 0x0D000000  # lossless, no mipmaps
    LIMITER = -1
    DATA_FORMAT = 2
    IMAGE_FORMAT = 5  # RGBA8 WebP

    @classmethod
    def convert(cls, png_path: Path, res_path: str) -> Tuple[bytes, str]:
        """
        转换PNG为.ctex
        
        Args:
            png_path: PNG文件路径
            res_path: 游戏中的资源路径 (如 res://ModName/images/card.png)
        
        Returns:
            (ctex_data, ctex_filename)
            - ctex_data: .ctex文件字节数据
            - ctex_filename: 格式为 "原文件名.png-{md5}.ctex"
        """
        # 1. 读取PNG并转为无损WebP
        img = Image.open(png_path)

        # 确保RGBA模式
        if img.mode != 'RGBA':
            img = img.convert('RGBA')

        width, height = img.size

        # 保存为WebP (无损)
        import io
        webp_buffer = io.BytesIO()
        img.save(webp_buffer, format='WEBP', lossless=True, quality=100)
        webp_data = webp_buffer.getvalue()

        # 2. 构建56字节GST2头
        # 注意: 第5个字段(flags)后跟limiter，然后12字节padding
        # 实际布局: magic(4), version(4), width(4), height(4), flags(4), 
        #           limiter(4), reserved1(4), reserved2(4), reserved3(4),
        #           data_format(4), packed_dims(4), reserved4(4), image_format(4), data_size(4)
        packed_dims = (height << 16) | width

        header = struct.pack(
            cls.HEADER_FORMAT,
            cls.MAGIC,  # 1
            cls.VERSION,  # 2
            width,  # 3
            height,  # 4
            cls.FLAGS,  # 5
            cls.LIMITER,  # 6
            0, 0, 0,  # 7, 8, 9 (三个保留)
            cls.DATA_FORMAT,  # 10
            packed_dims,  # 11
            0,  # 12 (保留)
            cls.IMAGE_FORMAT,  # 13
            len(webp_data)  # 14 (WebP数据大小)
        )

        # 验证头部长度
        assert len(header) == 56, f"Header should be 56 bytes, got {len(header)}"

        # 3. 合并头+WebP数据
        ctex_data = header + webp_data

        # 4. 生成文件名 (原文件名.png-{md5}.ctex)
        # MD5基于res_path计算
        md5 = hashlib.md5(res_path.encode('utf-8')).hexdigest()
        ctex_filename = f"{png_path.stem}.png-{md5}.ctex"

        return ctex_data, ctex_filename

    @classmethod
    def generate_import_file(cls, source_path: str, ctex_vpath: str) -> str:
        """
        生成.import文件内容
        
        Args:
            source_path: 源文件在PCK中的路径 (如 res://ModName/images/card.png)
            ctex_vpath: .ctex文件在PCK中的虚拟路径 (如 res://.godot/imported/card.png-xxx.ctex)
        
        Returns:
            .import文件内容
        """
        return f'''[remap]

importer="texture"
type="CompressedTexture2D"
path="{ctex_vpath}"
metadata={{
"vram_texture": false
}}

[deps]

source_file="{source_path}"
dest_files=["{ctex_vpath}"]

[params]

compress/mode=0
compress/high_quality=false
compress/lossy_quality=1.0
mipmaps/generate=false'''

    @classmethod
    def get_output_info(cls, png_path: Path, res_prefix: str, rel_path: str):
        """
        轻量级方法：仅计算预期的产物路径，不执行任何图像处理。
        用于增量更新检查。
        """
        # 1. 计算源虚拟路径 (res://ModName/images/xxx.png)
        source_vpath = f"res://{res_prefix}/{rel_path}"

        # 2. 计算 MD5 (基于虚拟路径)
        md5 = hashlib.md5(source_vpath.encode('utf-8')).hexdigest()

        # 3. 构造预期的 ctex 和 import 虚拟路径
        ctex_filename = f"{png_path.stem}.png-{md5}.ctex"
        ctex_vpath = f"res://.godot/imported/{ctex_filename}"
        import_vpath = f"{source_vpath}.import"

        return {
            "ctex_vpath": ctex_vpath,
            "import_vpath": import_vpath
        }

    @classmethod
    def convert_and_get_entries(cls, png_path: Path, res_prefix: str, rel_path: str):
        """
        一站式转换: 返回需要添加到PCK的文件条目
        
        Args:
            png_path: PNG文件路径
            res_prefix: 资源前缀 (如 ModName)
            rel_path: 相对于assets目录的路径 (如 images/cards/card.png)
        
        Returns:
            list of (virtual_path, data) 元组
        """
        # 源文件在游戏中的路径
        source_vpath = f"res://{res_prefix}/{rel_path}"

        # 转换ctex
        ctex_data, ctex_filename = cls.convert(png_path, source_vpath)

        # .ctex的虚拟路径 (在.godot/imported下)
        ctex_vpath = f"res://.godot/imported/{ctex_filename}"

        # .import文件路径 (与原PNG同目录)
        import_vpath = f"res://{res_prefix}/{rel_path}.import"

        # 生成.import内容
        import_content = cls.generate_import_file(source_vpath, ctex_vpath)

        return [
            (ctex_vpath, ctex_data),  # .ctex文件
            (import_vpath, import_content.encode('utf-8'))  # .import文件
        ]


# 简易测试
if __name__ == "__main__":
    import sys

    if len(sys.argv) != 2:
        print("用法: python png2ctex.py <png文件路径>")
        sys.exit(1)

    png_path = Path(sys.argv[1])
    if not png_path.exists():
        print(f"文件不存在: {png_path}")
        sys.exit(1)

    # 测试转换
    res_path = f"res://TestMod/images/{png_path.name}"
    ctex_data, ctex_name = CtexConverter.convert(png_path, res_path)

    # 保存到文件
    output_path = png_path.parent / ctex_name
    output_path.write_bytes(ctex_data)

    print(f"✅ 转换成功: {output_path}")
    print(f"   大小: {len(ctex_data)} 字节")

    # 生成.import文件
    import_content = CtexConverter.generate_import_file(
        res_path,
        f"res://.godot/imported/{ctex_name}"
    )
    import_path = png_path.with_suffix('.png.import')
    import_path.write_text(import_content)
    print(f"✅ 生成.import: {import_path}")
