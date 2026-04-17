using Godot;

namespace Superstitio.Main.Features.Felix.UI;

/// <summary>
/// 环形纹理生成器 - 纯静态工具类
/// </summary>
public static class RingTextureGenerator
{
    /// <summary>
    /// 生成环形纹理
    /// </summary>
    public static Texture2D Generate(
        float innerRadius, float outerRadius,
        Color color,
        float edgeSoftness = 1.5f,
        float startAngle = 0f, float fillDegrees = 360f
    )
    {
        // 1. 明确计算出直径
        int diameter = Mathf.CeilToInt(outerRadius * 2);
        // 2. 留出少许 Padding 空间防止锯齿裁剪，并确保是偶数以获得完美的中心点
        int size = diameter + 4;
        if (size % 2 != 0) size++;

        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);

        // 3. 此时中心点是严格的 (size/2, size/2)
        Vector2 center = new Vector2(size, size) / 2f;

        // 规范化起始角度
        float normalizedStart = Mathf.PosMod(startAngle, 360f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // 计算像素中心到纹理中心的距离
                // center 是 size / 2f (例如 157 / 2 = 78.5)
                // x + 0.5f 确保我们在计算像素的正中央位置
                Vector2 localPos = new Vector2(x + 0.5f, y + 0.5f) - center;
                float distance = localPos.Length();
                
                // 距离裁剪
                if (distance < innerRadius - edgeSoftness || distance > outerRadius + edgeSoftness)
                    continue;

                // 角度裁剪
                // 数学转 UI 角度：+90度。上=0, 右=90, 下=180, 左=270
                float angle = Mathf.RadToDeg(localPos.Angle()) + 90f;
                angle = Mathf.PosMod(angle, 360f);
                
                bool inAngle;
                float endAngle = normalizedStart + fillDegrees;
                if (endAngle > 360f)
                    inAngle = angle >= normalizedStart || angle <= endAngle - 360f;
                else
                    inAngle = angle >= normalizedStart && angle <= endAngle;
                
                if (!inAngle) continue;

                float alpha = CalculateAlpha(distance, innerRadius, outerRadius, edgeSoftness);

                if (alpha > 0.01f)
                {
                    Color finalColor = color;
                    finalColor.A = alpha;
                    image.SetPixel(x, y, finalColor);
                }
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static float CalculateAlpha(float distance, float innerRadius, float outerRadius, float edgeSoftness)
    {
        // 外边缘渐变
        if (distance > outerRadius - edgeSoftness && distance < outerRadius + edgeSoftness)
        {
            float alpha = 1f - (distance - (outerRadius - edgeSoftness)) / (edgeSoftness * 2);
            return Mathf.Clamp(alpha, 0f, 1f);
        }

        // 内边缘渐变
        if (distance > innerRadius - edgeSoftness && distance < innerRadius + edgeSoftness)
        {
            float alpha = (distance - (innerRadius - edgeSoftness)) / (edgeSoftness * 2);
            return Mathf.Clamp(alpha, 0f, 1f);
        }

        // 环内实体部分
        if (distance > innerRadius && distance < outerRadius)
        {
            return 1f;
        }

        // 超出范围
        return 0f;
    }
}