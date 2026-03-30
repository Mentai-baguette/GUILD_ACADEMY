using System;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class DamageCalculator
    {
        private readonly IRandom _random;

        public DamageCalculator(IRandom random)
        {
            _random = random;
        }

        public int Calculate(int atk, int def, ElementType attackElement,
            ElementType defenderWeakElement, ElementType defenderResistElement,
            ElementType defenderNullElement, bool isCritical, bool isBreakState)
        {
            int variance = _random.Range(-3, 4);
            int baseDamage = atk - def + variance;

            float elementMult = GetElementMultiplier(attackElement, defenderWeakElement, defenderResistElement, defenderNullElement);

            if (elementMult == 0f)
                return 0;

            float critMult = isCritical ? 1.5f : 1.0f;
            float breakMult = isBreakState ? 2.0f : 1.0f;

            int damage = (int)(baseDamage * elementMult * critMult * breakMult);
            return Math.Max(1, damage);
        }

        public static float GetElementMultiplier(ElementType attackElement,
            ElementType weakElement, ElementType resistElement, ElementType nullElement)
        {
            if (attackElement == ElementType.None)
                return 1.0f;

            if (attackElement == nullElement)
                return 0.0f;

            if (attackElement == weakElement)
                return 1.5f;

            if (attackElement == resistElement)
                return 0.5f;

            return 1.0f;
        }
    }
}
