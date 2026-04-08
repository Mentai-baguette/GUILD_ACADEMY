using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public enum FormationRow { Front, Back }

    public class FormationSystem
    {
        public const float BACK_ROW_PHYSICAL_MODIFIER = 0.8f; // -20%

        private readonly Dictionary<CharacterStats, FormationRow> _formations
            = new Dictionary<CharacterStats, FormationRow>();

        /// <summary>隊列を設定する</summary>
        public void SetRow(CharacterStats character, FormationRow row)
        {
            _formations[character] = row;
        }

        /// <summary>隊列を取得する（未登録は前列扱い）</summary>
        public FormationRow GetRow(CharacterStats character)
        {
            return _formations.TryGetValue(character, out var row) ? row : FormationRow.Front;
        }

        /// <summary>前列⇔後列を切り替える（ターン消費）</summary>
        public void ChangeRow(CharacterStats character)
        {
            var current = GetRow(character);
            _formations[character] = current == FormationRow.Front
                ? FormationRow.Back
                : FormationRow.Front;
        }

        /// <summary>
        /// 攻撃側の隊列補正を返す。
        /// 後列 + 物理攻撃 → 0.8、それ以外 → 1.0
        /// </summary>
        public float GetAttackModifier(CharacterStats attacker, bool isMagic)
        {
            if (isMagic) return 1.0f;
            return GetRow(attacker) == FormationRow.Back
                ? BACK_ROW_PHYSICAL_MODIFIER
                : 1.0f;
        }

        /// <summary>
        /// 防御側の隊列補正を返す。
        /// 後列 + 物理被弾 → 0.8、それ以外 → 1.0
        /// </summary>
        public float GetDefenseModifier(CharacterStats defender, bool isMagic)
        {
            if (isMagic) return 1.0f;
            return GetRow(defender) == FormationRow.Back
                ? BACK_ROW_PHYSICAL_MODIFIER
                : 1.0f;
        }
    }
}
