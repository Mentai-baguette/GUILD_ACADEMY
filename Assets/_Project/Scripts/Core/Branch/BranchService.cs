using System;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Branch
{
    public class BranchService
    {
        // NOTE: Flags/Trust are public because EndingContext references them directly.
        // Mutations should go through BranchService methods (SetFlag, AddTrust) to ensure events fire.
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

        public int GetActiveInfoFlagCount() => Flags.GetActiveCount();
        public bool AreAllInfoFlagsSet() => Flags.AreAllSet();
        public bool TrustMeetsThreshold(CharacterId id, int threshold) => Trust.MeetsThreshold(id, threshold);

        public EndingType CheckEnding(EndingContext context)
        {
            return EndingResolver.Resolve(context);
        }

        public EndingContext CreateEndingContext(
            BattlePhase phase, BattleResult result,
            bool shionRescued, bool carlosDefeated,
            int erosionPercent, int shionTrust = -1,
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
                ShionTrust = shionTrust >= 0 ? shionTrust : Trust.GetTrust(CharacterId.Shion),
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
