using UnityEngine;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;

namespace GuildAcademy.MonoBehaviours.Branch
{
    public class BranchManager : MonoBehaviour
    {
        public static BranchManager Instance { get; private set; }

        public BranchService Service { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Service = new BranchService();
        }

        public void SetFlag(string flagName, bool value) => Service.SetFlag(flagName, value);
        public bool GetFlag(string flagName) => Service.GetFlag(flagName);
        public void AddTrust(CharacterId id, int amount) => Service.AddTrust(id, amount);
        public int GetTrust(CharacterId id) => Service.GetTrust(id);
        public EndingType CheckEnding(EndingContext context) => Service.CheckEnding(context);

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
