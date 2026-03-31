using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class ActionResult
    {
        public CharacterStats Attacker { get; set; }
        public CharacterStats Target { get; set; }
        public int DamageDealt { get; set; }
        public int HealAmount { get; set; }
        public bool WasCritical { get; set; }
        public bool WasWeakHit { get; set; }
        public bool TriggeredBreak { get; set; }
        public bool TargetDefeated { get; set; }
    }
}
