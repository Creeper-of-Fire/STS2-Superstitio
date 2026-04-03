using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Superstitio.Main.Extensions;

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
            if (target is { IsAlive: true })
                return attackCommand.Targeting(target);

            return attackCommand.TargetingRandomOpponents(combatState, allowDuplicates);
        }

        /// <summary>
        /// 应用目标选择逻辑
        /// </summary>
        public AttackCommand ApplyTargeting(CardModel card, Creature? target, bool tryRandomWhenTargetDie, bool forceAttackTarget)
        {
            var combatState = card.CombatState;

            if (forceAttackTarget)
                return SingleAttackCommand();

            switch (card.TargetType)
            {
                case TargetType.AnyEnemy:
                    return SingleAttackCommand();
                case TargetType.AllEnemies:
                    if (combatState is null)
                        return attackCommand;
                    return attackCommand.TargetingAllOpponents(combatState);
                case TargetType.RandomEnemy:
                    if (combatState is null)
                        return attackCommand;
                    return attackCommand.TargetingRandomOpponents(combatState);
                case TargetType.None:
                case TargetType.Self:
                case TargetType.AnyPlayer:
                case TargetType.AnyAlly:
                case TargetType.AllAllies:
                case TargetType.TargetedNoCreature:
                case TargetType.Osty:
                default:
                    throw new Exception($"Unsupported AttackCommand target type {card.TargetType} for card {card.Title}");
            }

            AttackCommand SingleAttackCommand()
            {
                if (target is not null)
                    return attackCommand.Targeting(target);
                if (tryRandomWhenTargetDie && combatState is not null)
                    return attackCommand.TryTargetingOrRandom(target, combatState);

                return attackCommand;
            }
        }

        /// <summary>
        /// 应用攻击特效
        /// </summary>
        public AttackCommand ApplyEffects(string? vfx, string? sfx, string? tmpSfx)
        {
            if (vfx is null && sfx is null && tmpSfx is null)
                return attackCommand;

            return attackCommand.WithHitFx(vfx: vfx, sfx: sfx, tmpSfx: tmpSfx);
        }
    }

    extension(DamageCmd)
    {
        /// <summary>
        /// 为卡牌创建攻击命令。根据提供的伤害变量或卡牌自带的动态变量确定伤害值，
        /// 并配置攻击目标、攻击次数和特效（视觉/音效）。
        /// </summary>
        public static AttackCommand AutoAttack(CardModel card, CardPlay cardPlay, DynamicVar? varForDamage = null, int hitCount = 1,
            bool tryRandomWhenTargetDie = false, bool forceAttackTarget = false,
            string? vfx = null, string? sfx = null, string? tmpSfx = null)
        {
            return DamageCmd.AutoAttack(
                card, cardPlay.Target, varForDamage, hitCount,
                tryRandomWhenTargetDie, forceAttackTarget,
                vfx, sfx, tmpSfx
            );
        }

        /// <summary>
        /// 为卡牌创建攻击命令。根据提供的伤害变量或卡牌自带的动态变量确定伤害值，
        /// 并配置攻击目标、攻击次数和特效（视觉/音效）。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="target"></param>
        /// <param name="varForDamage">可选的自定义伤害变量。如果提供，将优先使用其值；否则尝试从卡牌动态变量中获取。</param>
        /// <param name="hitCount">攻击命中次数，默认为 1 次。</param>
        /// <param name="tryRandomWhenTargetDie">当 <paramref name="card"/>的<see cref="CardModel.TargetType"/>为<see cref="TargetType.AnyEnemy"/>且<paramref name="target"/>死亡/为空时，是否随机转火。</param>
        /// <param name="forceAttackTarget">启用时，强制攻击目标，不受随机等影响</param>
        /// <param name="vfx">攻击命中时的视觉效果文件名（可选）。</param>
        /// <param name="sfx">攻击命中时的音效文件名（可选）。</param>
        /// <param name="tmpSfx">攻击命中时的临时音效文件名（可选）。</param>
        /// <returns>配置好的 <see cref="AttackCommand"/> 实例</returns>
        /// <exception cref="Exception">当卡牌既没有提供有效的 <paramref name="varForDamage"/>，
        /// 也不包含默认的计算伤害变量<see cref="CalculatedDamageVar"/>或基础伤害变量<see cref="DamageVar"/>时抛出。</exception>
        public static AttackCommand AutoAttack(CardModel card, Creature? target, DynamicVar? varForDamage = null, int hitCount = 1,
            bool tryRandomWhenTargetDie = false, bool forceAttackTarget = false,
            string? vfx = null, string? sfx = null, string? tmpSfx = null)
        {
            var cmd = CreateDamageCommand(card, varForDamage);

            return cmd.FromCard(card).WithHitCount(hitCount)
                .ApplyTargeting(card, target, tryRandomWhenTargetDie, forceAttackTarget)
                .ApplyEffects(vfx: vfx, sfx: sfx, tmpSfx: tmpSfx);
        }
    }

    /// <summary>
    /// 根据伤害变量创建攻击命令
    /// </summary>
    /// <param name="card">发起攻击的卡牌模型。</param>
    /// <param name="varForDamage"></param>
    private static AttackCommand CreateDamageCommand(CardModel card, DynamicVar? varForDamage)
    {
        if (varForDamage is CalculatedDamageVar calculatedDamageVar)
            return DamageCmd.Attack(calculatedDamageVar);

        if (varForDamage is not null)
            return DamageCmd.Attack(varForDamage.BaseValue);

        if (card.DynamicVars.ContainsKey(CalculatedDamageVar.defaultName))
            return DamageCmd.Attack(card.DynamicVars.CalculatedDamage);

        if (card.DynamicVars.ContainsKey(DamageVar.defaultName))
            return DamageCmd.Attack(card.DynamicVars.Damage.BaseValue);

        throw new Exception($"卡牌 '{card.Title}' 没有受支持的伤害变量。请确保卡牌定义了 'Damage' 或计算后的伤害变量。");
    }
}