using System.Runtime.CompilerServices;

namespace Superstitio.Api.Extensions;

/// <summary>
/// 提供一种便捷方式，将额外数据惰性地附加到任意引用类型对象上。
/// </summary>
/// <typeparam name="TKey">附加目标对象的类型，必须是引用类型 (<see langword="class"/>)。</typeparam>
/// <typeparam name="TVal">附加数据的类型。</typeparam>
/// <remarks>
/// <para>此类是对 <see cref="ConditionalWeakTable{TKey, TValue}"/> 的一个基础封装。</para>
/// <para>核心优势：持有键（Key）的弱引用。当 <typeparamref name="TKey"/> 实例没有其他强引用且被垃圾回收（GC）时，关联的数据值也会自动被清理，有效避免内存泄露。</para>
/// <para>
/// <b>性能提示：</b>内部使用 <see langword="object"/> 存储数据。若 <typeparamref name="TVal"/> 为值类型（如 <see langword="int"/>、<see langword="bool"/>），
/// 在存取过程中会发生装箱（Boxing）操作，可能对性能产生负面影响。建议 <typeparamref name="TVal"/> 尽量使用引用类型。
/// </para>
/// <para>此类型的所有公共成员均为线程安全。</para>
/// <para>来自 BaseLib 的代码 <see href="https://github.com/Alchyr/BaseLib-StS2"/></para>
/// </remarks>
public class WeekField<TKey, TVal> where TKey : class
{
    private ConditionalWeakTable<TKey, object?> Table { get; } = [];
    private Func<TKey, TVal?> DefaultVal { get; }

    /// <summary>
    /// 使用一个返回默认值的无参工厂方法初始化 <see cref="WeekField{TKey,TVal}"/> 类的新实例。
    /// </summary>
    /// <param name="defaultVal">一个用于生成默认值的委托。当访问尚未附加数据的键时，将调用此委托来生成初始值。</param>
    public WeekField(Func<TVal?> defaultVal)
    {
        this.DefaultVal = _ => defaultVal();
    }

    /// <summary>
    /// 使用一个基于键（<typeparamref name="TKey"/>）生成默认值的工厂方法初始化 <see cref="WeekField{TKey,TVal}"/> 类的新实例。
    /// </summary>
    /// <param name="defaultVal">
    /// 一个用于生成默认值的委托，该委托接收当前要附加的目标对象（键）作为参数。
    /// 当访问尚未附加数据的键时，将调用此委托来生成初始值。
    /// </param>
    public WeekField(Func<TKey, TVal?> defaultVal)
    {
        this.DefaultVal = defaultVal;
    }

    /// <summary>
    /// 获取或设置与指定对象关联的值。
    /// </summary>
    /// <param name="obj">要获取或设置其关联数据的对象。</param>
    /// <returns>与 <paramref name="obj"/> 关联的数据。</returns>
    /// <remarks>
    /// 当获取（Get）尚未附加任何数据的键时，将自动调用构造函数中提供的 <see cref="DefaultVal"/> 委托来生成并存储默认值。
    /// </remarks>
    public TVal? this[TKey obj]
    {
        get => this.Get(obj);
        set => this.Set(obj, value);
    }

    /// <summary>
    /// 获取与指定对象关联的值。若尚不存在关联，则使用默认值工厂方法创建并存储新值。
    /// </summary>
    /// <param name="obj">目标对象。</param>
    /// <returns>关联的值，若默认工厂返回 <see langword="null"/> 则可能为 <see langword="null"/>。</returns>
    public TVal? Get(TKey obj)
    {
        if (this.Table.TryGetValue(obj, out var result)) return (TVal?)result;

        this.Table.Add(obj, result = this.DefaultVal(obj));
        return (TVal?)result;
    }

    /// <summary>
    /// 设置与指定对象关联的值。这将覆盖任何已存在的旧值。
    /// </summary>
    /// <param name="obj">目标对象。</param>
    /// <param name="val">要关联的值。</param>
    public void Set(TKey obj, TVal? val)
    {
        this.Table.AddOrUpdate(obj, val);
    }
}