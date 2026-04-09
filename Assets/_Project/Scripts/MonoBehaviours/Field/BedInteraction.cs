using UnityEngine;
using GuildAcademy.Core.Branch;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class BedInteraction : MonoBehaviour
    {
        [SerializeField] private string[] _examineTexts = new[]
        {
            "もう少し寝ていたい...",
            "...まだ眠い",
            "...zzz"
        };

        private AcademyRefusalChecker _checker;
        private int _examineCount;

        public void Initialize(AcademyRefusalChecker checker)
        {
            _checker = checker;
        }

        public void OnInteract()
        {
            _examineCount++;

            // After examining bed 3+ times, register bed return flag
            if (_examineCount >= 3 && _checker != null)
            {
                _checker.RegisterBedReturn();
            }

            // Display text (cycle through available texts)
            var textIndex = Mathf.Min(_examineCount - 1, _examineTexts.Length - 1);
            Debug.Log($"[Bed] {_examineTexts[textIndex]}");
        }
    }
}
