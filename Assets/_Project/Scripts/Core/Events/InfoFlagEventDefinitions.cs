using System.Collections.Generic;
using GuildAcademy.Core.Branch;

namespace GuildAcademy.Core.Events
{
    public static class InfoFlagEventDefinitions
    {
        public static List<InfoFlagEventData> CreateAll()
        {
            return new List<InfoFlagEventData>
            {
                new InfoFlagEventData(
                    "evt_shion_past", FlagSystem.Flags.ShionPast,
                    "dialogue_shion_past",
                    "シオンから過去の話を聞く（信頼値が一定以上で発生）"),

                new InfoFlagEventData(
                    "evt_carlos_plan", FlagSystem.Flags.CarlosPlan,
                    "dialogue_carlos_plan",
                    "カルロスの計画の一端を知る",
                    new List<string> { FlagSystem.Flags.AcademySecret }),

                new InfoFlagEventData(
                    "evt_vessel_truth", FlagSystem.Flags.VesselTruth,
                    "dialogue_vessel_truth",
                    "器の真実を知る",
                    new List<string> { FlagSystem.Flags.ShionPast }),

                new InfoFlagEventData(
                    "evt_academy_secret", FlagSystem.Flags.AcademySecret,
                    "dialogue_academy_secret",
                    "学園の秘密を発見する"),

                new InfoFlagEventData(
                    "evt_seal_method", FlagSystem.Flags.SealMethod,
                    "dialogue_seal_method",
                    "封印の方法を知る",
                    new List<string> { FlagSystem.Flags.VesselTruth }),

                new InfoFlagEventData(
                    "evt_dark_power_risk", FlagSystem.Flags.DarkPowerRisk,
                    "dialogue_dark_power_risk",
                    "闇の力のリスクを理解する"),

                new InfoFlagEventData(
                    "evt_trust_betrayal", FlagSystem.Flags.TrustBetrayal,
                    "dialogue_trust_betrayal",
                    "信頼と裏切りの真実",
                    new List<string> { FlagSystem.Flags.CarlosPlan }),

                new InfoFlagEventData(
                    "evt_salvation_path", FlagSystem.Flags.SalvationPath,
                    "dialogue_salvation_path",
                    "救済の道を見出す",
                    new List<string> { FlagSystem.Flags.SealMethod, FlagSystem.Flags.DarkPowerRisk })
            };
        }
    }
}
