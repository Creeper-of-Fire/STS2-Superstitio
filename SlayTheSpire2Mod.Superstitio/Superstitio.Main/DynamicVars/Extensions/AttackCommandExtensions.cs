using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Superstitio.Main.DynamicVars.Extensions;

/// <summary>
/// 攻击命令的扩展方法
/// </summary>
public static class AttackCommandExtensions
{
    extension(AttackCommand attackCommand)
    {
        /// <summary>
        /// 尝试选择目标，如果目标无效则选择随机目标
        /// </summary>
        public AttackCommand TryTargetingOrRandom(Creature? target, CombatState combatState, bool allowDuplicates = true)
        {
            if (target is { IsAlive: true, IsEnemy: true })
                return attackCommand.Targeting(target);

            return attackCommand.TargetingRandomOpponents(combatState, allowDuplicates);
        }
    }
}