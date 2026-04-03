using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Superstitio.Main.Extensions;

/// <summary>
/// 提供对 <see cref="IEnumerable{IHoverTip}"/> 的扩展方法。
/// </summary>
public static class HoverTipCollectionExtensions
{
    /// <param name="tips">提示集合。</param>
    extension(IEnumerable<IHoverTip> tips)
    {
        /// <summary>
        /// 如果条件为真，则添加提示到集合中。
        /// </summary>
        /// <param name="condition">添加条件。</param>
        /// <param name="tipFactory">用于创建要添加的提示的工厂函数。</param>
        /// <returns>原集合。</returns>
        public IEnumerable<IHoverTip> TryAddTip(bool condition, Func<IHoverTip> tipFactory)
        {
            var tipList = tips.ToList();

            if (condition)
                tipList.Add(tipFactory());

            return tipList;
        }

        /// <summary>
        /// 如果 <see cref="LocString"/> 存在（非空且不为空字符串），则添加到提示集合中。
        /// </summary>
        /// <param name="locString">要检查并添加的本地化字符串。</param>
        /// <returns>原集合。</returns>
        public IEnumerable<IHoverTip> TryAddTip(LocString locString)
        {
            var tipList = tips.ToList();

            if (locString.Exists())
                tipList.Add(new HoverTip(locString));

            return tipList;
        }

        /// <summary>
        /// 添加多个提示到集合中。
        /// </summary>
        /// <param name="addonList">多个提示</param>
        /// <returns>原集合。</returns>
        public IEnumerable<IHoverTip> TryAddTip(IEnumerable<IHoverTip> addonList)
        {
            var tipList = tips.ToList();

            tipList.AddRange(addonList);

            return tipList;
        }
    }
}