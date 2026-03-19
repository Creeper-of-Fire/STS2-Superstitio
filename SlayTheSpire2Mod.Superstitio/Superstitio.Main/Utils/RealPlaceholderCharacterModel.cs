using BaseLib.Abstracts;

namespace Superstitio.Main.Utils;

/// <summary>
/// 真正的占位符角色基类。
/// 专门用于填补 <see cref="PlaceholderCharacterModel"/> 中遗漏的占位符属性（如过渡音效）。
/// </summary>
public abstract class RealPlaceholderCharacterModel : PlaceholderCharacterModel
{
    /// <summary>
    /// 角色选择过渡音效。
    /// 补充 <see cref="PlaceholderCharacterModel"/> 基类缺失的占位逻辑。
    /// </summary>
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
}