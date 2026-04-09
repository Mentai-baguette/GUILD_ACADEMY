using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    /// <summary>
    /// 7属性の相性テーブル
    /// 循環弱点: 火→氷→風→地→火（1.5倍）
    /// 相互弱点: 光⇔闇（1.5倍）
    /// 同属性: 耐性（0.5倍）
    /// ボス専用: 自属性吸収（-1.0 = HP回復）
    /// </summary>
    public static class ElementAffinityTable
    {
        public const float WEAK_MULTIPLIER = 1.5f;
        public const float RESIST_MULTIPLIER = 0.5f;
        public const float NULL_MULTIPLIER = 0.0f;
        /// <summary>
        /// 吸収倍率（負値）。呼び出し側で負の倍率を吸収/回復として処理し、
        /// HPの上限管理（最大HPを超えない）も行う必要がある。
        /// </summary>
        public const float ABSORB_MULTIPLIER = -1.0f;
        public const float NEUTRAL_MULTIPLIER = 1.0f;

        /// <summary>
        /// 攻撃属性と防御属性から倍率を返す
        /// absorbs: ボス専用の吸収属性（nullならなし）
        /// </summary>
        public static float GetMultiplier(ElementType attackElement, ElementType defenderElement,
            ElementType? absorbElement = null)
        {
            if (attackElement == ElementType.None)
                return NEUTRAL_MULTIPLIER;

            // ボス専用: 吸収判定
            if (absorbElement.HasValue && attackElement == absorbElement.Value)
                return ABSORB_MULTIPLIER;

            // 同属性: 耐性
            if (attackElement == defenderElement && defenderElement != ElementType.None)
                return RESIST_MULTIPLIER;

            // 弱点判定
            if (IsWeakAgainst(attackElement, defenderElement))
                return WEAK_MULTIPLIER;

            return NEUTRAL_MULTIPLIER;
        }

        /// <summary>
        /// attackElementがdefenderElementの弱点を突くか
        /// 循環: 火→氷→風→地→火
        /// 相互: 光⇔闇
        /// </summary>
        public static bool IsWeakAgainst(ElementType attackElement, ElementType defenderElement)
        {
            return (attackElement, defenderElement) switch
            {
                // 循環弱点: 火→氷→風→地→火
                (ElementType.Fire, ElementType.Ice) => true,
                (ElementType.Ice, ElementType.Wind) => true,
                (ElementType.Wind, ElementType.Earth) => true,
                (ElementType.Earth, ElementType.Fire) => true,

                // 相互弱点: 光⇔闇
                (ElementType.Light, ElementType.Dark) => true,
                (ElementType.Dark, ElementType.Light) => true,

                _ => false
            };
        }

        /// <summary>
        /// 指定属性の弱点属性を返す
        /// </summary>
        public static ElementType GetWeakness(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => ElementType.Earth,  // 火は地に弱い
                ElementType.Ice => ElementType.Fire,     // 氷は火に弱い
                ElementType.Wind => ElementType.Ice,     // 風は氷に弱い
                ElementType.Earth => ElementType.Wind,   // 地は風に弱い
                ElementType.Light => ElementType.Dark,   // 光は闇に弱い
                ElementType.Dark => ElementType.Light,   // 闇は光に弱い
                _ => ElementType.None
            };
        }
    }
}
