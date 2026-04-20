// -----------------------------------------------------------------------------
// 源代码来源: https://github.com/BAKAOLC/STS2-RitsuLib/
// 
// MIT License
// 
// Copyright (c) 2026 OLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----------------------------------------------------------------------------

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Api.DynamicVars;

/// <summary>
///     Factory helpers for common mod card <see cref="DynamicVar" /> shapes.
///     为常见的 Mod 卡牌 <see cref="DynamicVar"/> 变量 提供工厂辅助方法。
/// </summary>
public static class ModVars
{
    /// <summary>
    ///     创建一个整数的动态变量，名称为 <paramref name="name"/>，数值为 <paramref name="amount"/>。
    /// </summary>
    /// <param name="name">动态变量的键名（Key）。</param>
    /// <param name="amount">动态变量的数值。</param>
    /// <returns>一个 <see cref="IntVar"/> 实例。</returns>
    public static IntVar Int(string name, decimal amount)
    {
        return new IntVar(name, amount);
    }

    /// <summary>
    ///     创建一个字符串类型的动态变量，名称为 <paramref name="name"/>。
    /// </summary>
    /// <param name="name">动态变量的键名（Key）。</param>
    /// <param name="value">可选的初始字符串值，默认为空字符串。</param>
    /// <returns>一个 <see cref="StringVar"/> 实例。</returns>
    public static StringVar String(string name, string value = "")
    {
        return new StringVar(name, value);
    }

    /// <summary>
    ///     创建一个 <see cref="ComputedDynamicVar"/> 实例，并支持可选的预览计算逻辑。
    /// </summary>
    /// <param name="name">动态变量的键名（Key）。</param>
    /// <param name="baseValue">基础回退数值，当没有预览覆盖逻辑时使用。</param>
    /// <param name="currentValueFactory">
    ///     用于解析实时数值的委托。接收所属的 <see cref="CardModel"/>（可能在卡牌上下文外为空），返回计算后的数值。
    /// </param>
    /// <param name="previewValueFactory">
    ///     可选委托，用于在卡牌预览状态下提供专门的数值计算逻辑。
    ///     如果为 <c>null</c>，则回退使用 <paramref name="currentValueFactory"/>。
    /// </param>
    /// <returns>一个 <see cref="ComputedDynamicVar"/> 实例。</returns>
    public static ComputedDynamicVar Computed(
        string name,
        decimal baseValue,
        Func<CardModel?, decimal> currentValueFactory,
        Func<CardModel?, CardPreviewMode, Creature?, bool, decimal>? previewValueFactory = null
    )
    {
        return new ComputedDynamicVar(name, baseValue, currentValueFactory, previewValueFactory);
    }
}

/// <summary>
///     一种特殊的 <see cref="DynamicVar"/>，其显示的值由委托动态计算生成，而非基于固定的基础数值。
/// </summary>
public sealed class ComputedDynamicVar : DynamicVar
{
    // 用于计算实时数值的委托
    private Func<CardModel?, decimal> CurrentValueFactory { get; }
    
    // 用于计算预览界面数值的可选委托
    private Func<CardModel?, CardPreviewMode, Creature?, bool, decimal>? PreviewValueFactory { get; }

    /// <summary>
    ///     创建一个带有可选预览逻辑的计算型动态变量。
    /// </summary>
    /// <param name="name">
    ///     动态变量的键名（Key）。
    /// </param>
    /// <param name="baseValue">
    ///     回退基础数值。当没有预览覆盖逻辑应用时，将使用此数值。
    /// </param>
    /// <param name="currentValueFactory">
    ///     解析实时数值的委托。接收所属的 <see cref="CardModel"/> 实例（在非卡牌上下文中可能为 <c>null</c>），返回计算出的具体数值。
    /// </param>
    /// <param name="previewValueFactory">
    ///     可选的覆盖委托，用于在卡牌预览阶段进行数值计算。如果此参数为 <c>null</c>，则将使用 <paramref name="currentValueFactory"/> 作为替代。
    /// </param>
    public ComputedDynamicVar(
        string name,
        decimal baseValue,
        Func<CardModel?, decimal> currentValueFactory,
        Func<CardModel?, CardPreviewMode, Creature?, bool, decimal>? previewValueFactory = null
    )
        : base(name, baseValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(currentValueFactory);

        this.CurrentValueFactory = currentValueFactory;
        this.PreviewValueFactory = previewValueFactory;
    }

    /// <inheritdoc />
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks
    )
    {
        this.PreviewValue = this.PreviewValueFactory?.Invoke(card, previewMode, target, runGlobalHooks)
                            ?? this.CurrentValueFactory(card);
    }

    /// <inheritdoc />
    protected override decimal GetBaseValueForIConvertible()
    {
        return this.CurrentValueFactory(this._owner as CardModel);
    }
}