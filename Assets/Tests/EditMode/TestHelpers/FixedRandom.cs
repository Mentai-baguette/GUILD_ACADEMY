using GuildAcademy.Core.Battle;

namespace GuildAcademy.Tests.EditMode.TestHelpers
{
    public class FixedRandom : IRandom
    {
        private readonly int _value;

        public FixedRandom(int value)
        {
            _value = value;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return _value;
        }
    }
}
