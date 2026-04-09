using System;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Branch
{
    public class BranchService
    {
        public FlagSystem Flags { get; }
        public TrustSystem Trust { get; }

        public event Action<string, bool> OnFlagChanged;
        public event Action<CharacterId, int> OnTrustChanged;

        public BranchService() : this(new FlagSystem(), new TrustSystem()) { }

        public BranchService(FlagSystem flags, TrustSystem trust)
        {
            Flags = flags ?? throw new ArgumentNullException(nameof(flags));
            Trust = trust ?? throw new ArgumentNullException(nameof(trust));
        }

        public void SetFlag(string flagName, bool value)
        {
            Flags.Set(flagName, value);
            OnFlagChanged?.Invoke(flagName, value);
        }

        public bool GetFlag(string flagName) => Flags.Get(flagName);

        public void AddTrust(CharacterId id, int amount)
        {
            Trust.AddTrust(id, amount);
            OnTrustChanged?.Invoke(id, Trust.GetTrust(id));
        }

        public int GetTrust(CharacterId id) => Trust.GetTrust(id);

        public EndingType CheckEnding(EndingContext context)
        {
            return EndingResolver.Resolve(context);
        }

        public EndingContext CreateEndingContext(
            BattlePhase phase, BattleResult result,
            bool shionRescued, bool carlosDefeated,
            int erosionPercent, int shionTrust = 0,
            bool greyveEventCleared = false, int setsunaTrust = 0)
        {
            return new EndingContext
            {
                Flags = Flags,
                Trust = Trust,
                Phase = phase,
                Result = result,
                ShionRescued = shionRescued,
                CarlosDefeated = carlosDefeated,
                ErosionPercent = erosionPercent,
                ShionTrust = shionTrust,
                GreyveEventCleared = greyveEventCleared,
                SetsunaTrust = setsunaTrust
            };
        }

        public void Reset()
        {
            Flags.Reset();
            Trust.Reset();
        }
    }
}
