using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class BattleResultCalculator
    {
        /// <summary>
        /// バトル結果を計算し、パーティメンバーのステータスを更新する。
        /// 注意: このメソッドはpartyメンバーのExp/Lv/Spを直接変更する副作用があります。
        /// 1回のバトルにつき1回だけ呼び出してください（二重呼び出しでEXP二重加算になります）。
        /// EXP = 敵Lv合計 × 10、Gold = 敵Lv合計 × 5。
        /// LvUP閾値: 100 × 現Lv。SP: 偶数Lvで+1SP。
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
