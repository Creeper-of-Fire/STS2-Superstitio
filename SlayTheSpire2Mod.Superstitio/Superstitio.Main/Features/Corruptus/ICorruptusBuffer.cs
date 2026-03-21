using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Superstitio.Main.Features.Corruptus;

/// <summary>
/// 定义拥有腐朽缓冲组件的实体接口。
/// </summary>
public interface ICorruptusBuffer
{
    /// <summary>
    /// 获取腐朽缓冲组件实例。
    /// </summary>
    public CorruptusBufferComponent CorruptusBufferComponent { get; }

    /// <summary>
    /// 获取拥有该缓冲的生物实体。
    /// </summary>
    public Creature OwnerCreature { get; }
}