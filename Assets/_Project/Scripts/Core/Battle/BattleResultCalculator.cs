using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class BattleResultCalculator
    {
        /// <summary>
        /// バトル結果を計算する。
        /// EXP = 敵Lv合計 × 10、Gold = 敵Lv合計 × 5。
        /// パーティ各メンバーにEXPを加算し、LvUP判定（閾値: 100 × 現Lv）を行う。
        /// SP = 2Lvごとに+1SP。
        /// </summary>
        public BattleResultData Calculate(List<CharacterStats> party, List<CharacterStats> enemies)
        {
            var result = new BattleResultData();

            int enemyLvSum = enemies.Sum(e => e.Lv);
            result.TotalEXP = enemyLvSum * 10;
            result.TotalGold = enemyLvSum * 5;

            foreach (var member in party)
            {
                int oldLevel = member.Lv;
                int oldSp = member.Sp;

                member.Exp += result.TotalEXP;

                // レベルアップ判定: 閾値 = 100 × 現Lv
                while (member.Exp >= 100 * member.Lv)
                {
                    member.Exp -= 100 * member.Lv;
                    member.Lv++;

                    // SP: 偶数Lvで+1SP
                    if (member.Lv % 2 == 0)
                    {
                        member.Sp++;
                    }
                }

                if (member.Lv > oldLevel)
                {
                    var levelUpInfo = new LevelUpInfo
                    {
                        OldLevel = oldLevel,
                        NewLevel = member.Lv,
                        StatChanges = new Dictionary<string, int>()
                    };
                    result.LevelUps[member.Name] = levelUpInfo;
                }

                result.TotalSP += member.Sp - oldSp;
            }

            return result;
        }
    }
}
