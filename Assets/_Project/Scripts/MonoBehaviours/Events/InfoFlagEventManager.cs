using UnityEngine;
using GuildAcademy.Core.Events;
using GuildAcademy.Core.Branch;
using GuildAcademy.MonoBehaviours.Branch;

namespace GuildAcademy.MonoBehaviours.Events
{
    public class InfoFlagEventManager : MonoBehaviour
    {
        public static InfoFlagEventManager Instance { get; private set; }
        public InfoFlagEventRegistry Registry { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var flagSystem = BranchManager.Instance?.Service?.Flags ?? new FlagSystem();
            Registry = new InfoFlagEventRegistry(flagSystem);

            foreach (var evt in InfoFlagEventDefinitions.CreateAll())
                Registry.Register(evt);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
