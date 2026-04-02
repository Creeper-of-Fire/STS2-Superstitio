using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.InlayHints;
using JetBrains.DocumentModel;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.RichText;

// 注册高亮器：定义在编辑器中的视觉效果
[RegisterHighlighter("DesignConceptHint", EffectType = EffectType.HIGHLIGHT_ABOVE_TEXT_MARKER)]
public class DesignConceptHighlighting : IInlayHintWithDescriptionHighlighting
{
    private readonly DocumentRange _range;
    private readonly string _originalConcept;

    public DesignConceptHighlighting(DocumentRange range, string originalConcept)
    {
        _range = range;
        _originalConcept = originalConcept;
    }

    // --- IHighlighting 接口实现 ---

    // 鼠标悬停在类名上时显示的浮窗文字
    public string ToolTip => $"设计原型概念: {_originalConcept}";

    // 右侧滚动条（Error Stripe）上显示的文字（Hint 级别通常不显示）
    public string ErrorStripeToolTip => ToolTip;

    // 检查高亮是否依然有效（比如文件是否被关闭）
    public bool IsValid() => _range.IsValid();

    // 告诉 Rider 高亮具体在哪个位置（也就是类名的位置）
    public DocumentRange CalculateRange() => _range;

    // --- IInlayHintHighlighting 接口实现 ---

    // 这是核心：真正显示在代码行里的灰色文字
    public string Text => $" [原型: {_originalConcept}]";

    // --- IInlayHintWithDescriptionHighlighting 接口实现 ---

    // 这一步是给 Rider 的设置页面或辅助功能用的
    // RichText 是 JetBrains 的带样式文本，可以直接从字符串隐式转换
    public RichText Description => new RichText($"原型映射提示: {_originalConcept}");
}