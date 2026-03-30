using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Branch
{
    public static class EndingResolver
    {
        public static EndingType Resolve(EndingContext context)
        {
            // END1: 学園に行かない → 裏ハッピー（最優先）
            if (context.Flags.Get("academy_refused"))
                return EndingType.HiddenHappy;

            // END6: 全滅 or 侵蝕90%+（END2/END3より優先）
            if (context.Phase >= BattlePhase.ShionPhase2)
            {
                if (context.Result == BattleResult.AllDefeated || context.ErosionPercent >= 90)
                    return EndingType.TotalDefeat;
            }

            // END2: シオン第1形態でプレイヤー敗北 → トゥルー
            if (context.Phase == BattlePhase.ShionPhase1 && context.Result == BattleResult.EnemyVictory)
                return EndingType.True;

            // END3: シオン第2形態でプレイヤー敗北 → シオンルート
            if (context.Phase == BattlePhase.ShionPhase2 && context.Result == BattleResult.EnemyVictory)
                return EndingType.ShionRoute;

            // END5: 全条件達成 → 表ハッピー
            if (context.Phase == BattlePhase.CarlosBattle && context.CarlosDefeated &&
                context.ShionRescued && context.Flags.AreAllSet() &&
                context.Trust.AllMeetThreshold(80))
                return EndingType.TrueHappy;

            // END4: シオン第2形態勝利だが救出未達 → ノーマル
            if (context.Phase == BattlePhase.ShionPhase2 && context.Result == BattleResult.PlayerVictory &&
                !context.ShionRescued)
                return EndingType.Normal;

            return EndingType.None;
        }
    }
}
