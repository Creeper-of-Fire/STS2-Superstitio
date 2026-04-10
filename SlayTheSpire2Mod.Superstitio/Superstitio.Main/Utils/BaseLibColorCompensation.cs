using Godot;

namespace Superstitio.Main.Utils;

/// <summary>
/// 为 BaseLib HSV Shader 提供颜色补偿的扩展方法。
/// 
/// 原理说明：
/// 1. Shader 使用 YIQ 色彩空间进行色相旋转
/// 2. 最终颜色 = Shader输出 * modulate_color * 底图纹理
/// 3. 底图(card_frame_red)本身是深红色渐变，平均亮度约 0.3-0.4
/// 4. modulate_color 是 Godot 的 self_modulate，可能被 UI 系统压暗
/// 
/// 因此需要将目标颜色乘以补偿系数，使最终渲染结果接近预期。
/// </summary>
public static class BaseLibColorCompensation
{
    // YIQ 变换矩阵 (Shader 源码中的定义)
    private static readonly Basis RGB_TO_YIQ = new Basis(
        new Vector3(0.2989f, 0.5959f, 0.2115f), // Y row
        new Vector3(0.5870f, -0.2774f, -0.5229f), // I row
        new Vector3(0.1140f, -0.3216f, 0.3114f) // Q row
    );

    private static readonly Basis YIQ_TO_RGB = RGB_TO_YIQ.Inverse();

    /// <summary>
    /// 精确求解：给定目标颜色，返回应该输入给 Shader 的 RGB 颜色。
    /// </summary>
    /// <param name="targetRgb">你希望屏幕上显示的颜色 (Linear RGB)</param>
    /// <param name="h">Shader 当前的 Hue 参数 (0-1)</param>
    /// <param name="s">Shader 当前的 Saturation 参数 (0-5)</param>
    /// <param name="v">Shader 当前的 Value 参数</param>
    /// <param name="baseTextureColor">底图的平均颜色 (Linear RGB)。默认深灰色。</param>
    /// <param name="skipHueCompensation">对于低饱和度颜色，建议跳过 Hue 补偿以避免色偏</param>
    /// <returns>应该设置的 ShaderColor</returns>
    public static Color CalculateRequiredInputColor(
        this Color targetRgb,
        float h = 0.025f, // 来自 card_frame_red_mat.tres
        float s = 0.85f, // 来自 card_frame_red_mat.tres
        float v = 1.0f, // 来自 card_frame_red_mat.tres
        Color? baseTextureColor = null,
        bool? skipHueCompensation = null)
    {
        float baseTextureLuminance = 0.65f; // 底图平均颜色？猜测值
        
        // 1. 底图补偿：目标颜色 / 底图颜色
        Color baseTex = baseTextureColor ?? new Color(baseTextureLuminance, baseTextureLuminance, baseTextureLuminance); 
        Vector3 compensated = new Vector3(
            targetRgb.R / Mathf.Max(baseTex.R, 0.001f),
            targetRgb.G / Mathf.Max(baseTex.G, 0.001f),
            targetRgb.B / Mathf.Max(baseTex.B, 0.001f)
        );

        // 2. 智能判断是否需要跳过 Hue 补偿
        //    计算目标颜色的饱和度：RGB 通道的标准差或最大-最小差
        float colorSpread = Mathf.Max(Mathf.Max(targetRgb.R, targetRgb.G), targetRgb.B)
                            - Mathf.Min(Mathf.Min(targetRgb.R, targetRgb.G), targetRgb.B);
        
        bool shouldSkipHue = skipHueCompensation ?? (colorSpread < 0.3f);

        Basis totalInvMatrix;

        if (shouldSkipHue)
        {
            // 低饱和度颜色：只做 Saturation 逆变换，不做 Hue 旋转
            // 这样可以保持原始色相，避免偏移
            float invS = 1.0f / Mathf.Max(s, 0.001f);
            Basis satInvMatrix = new Basis(
                new Vector3(1, 0, 0),
                new Vector3(0, invS, 0),
                new Vector3(0, 0, invS)
            );

            // 在 YIQ 空间应用 Saturation 逆变换
            totalInvMatrix = YIQ_TO_RGB * satInvMatrix * RGB_TO_YIQ;
        }
        else
        {
            // 高饱和度颜色：完整逆变换
            float angle = (1.0f - h) * Mathf.Pi * 2.0f;
            float sinH = Mathf.Sin(-angle);
            float cosH = Mathf.Cos(-angle);

            Basis hueInvMatrix = new Basis(
                new Vector3(1, 0, 0),
                new Vector3(0, cosH, -sinH),
                new Vector3(0, sinH, cosH)
            );

            float invS = 1.0f / Mathf.Max(s, 0.001f);
            Basis satInvMatrix = new Basis(
                new Vector3(1, 0, 0),
                new Vector3(0, invS, 0),
                new Vector3(0, 0, invS)
            );

            totalInvMatrix = YIQ_TO_RGB * satInvMatrix * hueInvMatrix * RGB_TO_YIQ;
        }

        // 5. 应用逆变换到补偿后的颜色
        Vector3 vec = totalInvMatrix * compensated;

        // 6. Value 补偿
        vec /= Mathf.Max(v, 0.001f);

        // 7. 返回颜色 (可能包含 >1.0 的 HDR 值)
        return new Color(vec.X, vec.Y, vec.Z);
    }
}