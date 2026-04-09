using UnityEngine;
using GuildAcademy.Core.Branch;
using GuildAcademy.MonoBehaviours.Branch;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class AcademyPathTracker : MonoBehaviour
    {
        private AcademyRefusalChecker _checker;

        // Dialogue lines for each opposite walk (indexed 0-4 for walks 1-5)
        private static readonly string[][] OppositeWalkDialogues = new[]
        {
            new[] { "ユナ「レイ、学園こっちだよ？」" },
            new[] { "ミオ「寝ぼけてるの？ こっちこっち〜」" },
            new[] { "ユナ「レイ...？ どうしたの？」", "ミオ「ねえ、なんか変だよ...」" },
            new[] { "ユナ「レイ、大丈夫？ 顔色悪いよ」", "ミオ「無理しなくていいんだよ？ でも...」" },
            new[] {
                "ユナ「...レイ。本当に、行きたくないの？」",
                "ユナ「わたしは...レイがどうしたいか、それが一番大事だと思う」",
                "ミオ「...ユナ」",
                "ユナ「でも、もし怖いだけなら...わたしがいるから。大丈夫だから」"
            }
        };

        private static readonly string[] RefusalDialogue = new[]
        {
            "レイ「...ごめん」",
            "（レイが一人で去っていく）",
            "（ユナとミオが見送る。ユナの目に涙）"
        };

        public void Initialize(AcademyRefusalChecker checker)
        {
            _checker = checker;
        }

        /// <summary>
        /// Called when player crosses the opposite-direction boundary.
        /// Returns dialogue lines to display, or null if already triggered.
        /// </summary>
        public string[] OnPlayerWalkedOpposite()
        {
            if (_checker == null || _checker.IsRefusalTriggered) return null;

            int walkCount = _checker.RegisterOppositeWalk();

            // Bed not yet examined — walks don't count, no dialogue
            if (walkCount <= 0) return null;

            if (_checker.IsRefusalTriggered)
            {
                // Trigger END1
                if (BranchManager.Instance != null)
                {
                    BranchManager.Instance.SetFlag(FlagSystem.Flags.AcademyRefused, true);
                }
                return RefusalDialogue;
            }

            // Return dialogue for this walk (1-indexed, array is 0-indexed)
            int dialogueIndex = Mathf.Min(walkCount - 1, OppositeWalkDialogues.Length - 1);
            return OppositeWalkDialogues[dialogueIndex];
        }
    }
}
