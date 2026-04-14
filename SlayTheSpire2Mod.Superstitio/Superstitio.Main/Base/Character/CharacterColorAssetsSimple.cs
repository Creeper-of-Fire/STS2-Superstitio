using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Superstitio.Main.Base.Character;

/// <summary>
/// 提供基础的颜色设计。
/// </summary>
public record CharacterColorAssetsSimple : CharacterColorAssets
{
    /// <inheritdoc />
    [SetsRequiredMembers]
    public CharacterColorAssetsSimple(Color nameColor)
    {
        this.NameColor = nameColor;
        this.EnergyLabelOutlineColor = this.NameColor.Darkened(0.6f);;
        this.DialogueColor = this.NameColor.Darkened(0.7f);
        this.MapDrawingColor = this.NameColor.Lightened(0.2f);
        this.RemoteTargetingLineColor = this.NameColor.Lightened(0.3f);
        this.RemoteTargetingLineOutline = this.NameColor.Darkened(0.5f);
    }
}