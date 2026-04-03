namespace Superstitio.Main.Maso.Cards.Kongfu;

/**
 * TODO Cost 1，造成 4-2 点 伤害 两次。顶峰中，每造成未被格挡的伤害，抽 1 张牌。
 * 目前顶峰不是状态而是触发时机，而且这张卡本身的效果就不佳，应该修改。
 * 也许简单改为不需要顶峰要求。
 * 也许改为下两次造成未被格挡的伤害时。
 */
public class KoKiLegPit;

// public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
// {
//     if (dealer == base.Owner && target.IsPlayer && props.IsPoweredAttack() && result.UnblockedDamage > 0)
//     {
//         await CreatureCmd.LoseMaxHp(choiceContext, target, base.Amount, isFromCard: false);
//     }
// }