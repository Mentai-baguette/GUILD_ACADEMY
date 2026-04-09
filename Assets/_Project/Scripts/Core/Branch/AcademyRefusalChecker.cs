using System;

namespace GuildAcademy.Core.Branch
{
    public class AcademyRefusalChecker
    {
        /// <summary>
        /// 警告台詞の回数。この回数の警告後、もう一度歩くとEND1が発動する。
        /// シナリオ: 5回の警告台詞（walks 1-5） → 6回目の歩行でEND1
        /// </summary>
        public const int WarningCount = 5;

        private bool _bedReturned;
        private int _oppositeWalkCount;

        public bool BedReturned => _bedReturned;
        public int OppositeWalkCount => _oppositeWalkCount;
        public bool IsRefusalTriggered { get; private set; }

        /// <summary>警告歩行時に発火（カウント 1〜WarningCount）。拒否発動時には発火しない。</summary>
        public event Action<int> OnOppositeWalk;
        /// <summary>END1拒否が確定した時に発火。</summary>
        public event Action OnRefusalTriggered;

        public void RegisterBedReturn()
        {
            _bedReturned = true;
        }

        /// <summary>
        /// Called when player walks opposite direction. Returns the walk count.
        /// Bed return flag must be set first (hidden operation 1).
        /// After WarningCount warnings, the next walk triggers refusal (END1).
        /// </summary>
        public int RegisterOppositeWalk()
        {
            if (IsRefusalTriggered) return _oppositeWalkCount;
            if (!_bedReturned) return 0;

            _oppositeWalkCount++;

            if (_oppositeWalkCount > WarningCount)
            {
                IsRefusalTriggered = true;
                OnRefusalTriggered?.Invoke();
            }
            else
            {
                OnOppositeWalk?.Invoke(_oppositeWalkCount);
            }

            return _oppositeWalkCount;
        }

        public void Reset()
        {
            _bedReturned = false;
            _oppositeWalkCount = 0;
            IsRefusalTriggered = false;
        }
    }
}
