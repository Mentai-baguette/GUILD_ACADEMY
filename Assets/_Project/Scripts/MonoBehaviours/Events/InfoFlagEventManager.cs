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
        }

        /// <summary>
        /// BranchManager初期化後に呼ぶ。Awake順に依存しないよう遅延初期化。
        /// </summary>
        public void Initialize(FlagSystem flagSystem)
        {
            if (flagSystem == null)
                throw new System.ArgumentNullException(nameof(flagSystem));

            Registry = new InfoFlagEventRegistry(flagSystem);
            foreach (var evt in InfoFlagEventDefinitions.CreateAll())
                Registry.Register(evt);
        }

        /// <summary>
        /// 未初期化の場合、BranchManagerから自動初期化を試みる。
        /// </summary>
        private void Start()
        {
            if (Registry == null && BranchManager.Instance?.Service != null)
            {
                Initialize(BranchManager.Instance.Service.Flags);
            }

            if (Registry == null)
            {
                Debug.LogError("[InfoFlagEventManager] BranchManager not found. Initialize() must be called manually.");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
