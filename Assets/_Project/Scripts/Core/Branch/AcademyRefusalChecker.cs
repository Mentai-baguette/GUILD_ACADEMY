using System;

namespace GuildAcademy.Core.Branch
{
    public class AcademyRefusalChecker
    {
        public const int RequiredOppositeWalks = 5;

        private bool _bedReturned;
        private int _oppositeWalkCount;

        public bool BedReturned => _bedReturned;
        public int OppositeWalkCount => _oppositeWalkCount;
        public bool IsRefusalTriggered { get; private set; }

        public event Action<int> OnOppositeWalk; // fires with current count (1-5)
        public event Action OnRefusalTriggered;

        public void RegisterBedReturn()
        {
            _bedReturned = true;
        }

        /// <summary>
        /// Called when player walks opposite direction. Returns the walk count (1-5+).
        /// After 5th walk, if called again, triggers refusal.
        /// </summary>
        public int RegisterOppositeWalk()
        {
            if (IsRefusalTriggered) return _oppositeWalkCount;

            _oppositeWalkCount++;
            OnOppositeWalk?.Invoke(_oppositeWalkCount);

            if (_oppositeWalkCount > RequiredOppositeWalks)
            {
                IsRefusalTriggered = true;
                OnRefusalTriggered?.Invoke();
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
