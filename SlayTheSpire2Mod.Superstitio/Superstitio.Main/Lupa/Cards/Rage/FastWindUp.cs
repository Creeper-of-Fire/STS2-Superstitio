using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Base;
using Superstitio.Main.DynamicVars.Extensions;
using Superstitio.Main.Extensions;
using Superstitio.Main.Features.Felix;
using Superstitio.Main.Features.HangingCard;
using Superstitio.Main.Features.PowerCardPower;
using Superstitio.Main.Lupa.Base;

namespace Superstitio.Main.Lupa.Cards.Rage;

// /// <summary>
// /// 漆黑噤默之页 - 当你 狂暴 时， 强制满足 !M! 次
// /// </summary>
// public class FastWindUp() : LupaBaseCard(new CardInitMessage
// {
//     BaseCost = 1,
//     Type = CardType.Power,
//     Rarity = CardRarity.Uncommon,
//     Target = TargetType.Self
// })
// {
//     private const int StatisfyTimes = 2;
//     private const int StatisfyTimesUpgrade = 1;
//
//     /// <inheritdoc />
//     protected override IEnumerable<DynamicVarSpec> InitVarsWithUpgrade =>
//     [
//         new PowerVar<FastWindUpPower>(StatisfyTimes).WithUpgrade(StatisfyTimesUpgrade)
//     ];
//
//     /// <inheritdoc cref="FastWindUp"/>
//     public class FastWindUpPower : PowerCardPower<FastWindUp>, IAfterClimaxReached
//     {
//         /// <inheritdoc />
//         public async Task AfterClimaxReached(Creature powerOwner, FelixPower felixPower, Creature? applier, CardModel? cardSource)
//         {
//             if (this.Owner.Player is null)
//                 return;
//
//             var configs = HangingCardManager.GetHangingCardTokens<AutoHangingCardTokenWithConfig>(this.Owner.Player)
//                 .Where(it => it.HangingCardConfig.HangingType == HangingType.Follow);
//
//             foreach (var config in configs)
//             {
//                 await config.AfterCardPlayed(choiceContext, cardPlay);
//             }
//         }
//     }
//
//     /// <inheritdoc />
//     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         await PowerCmd.ApplyByCard<FastWindUpPower>(this, this.Owner);
//     }
// }