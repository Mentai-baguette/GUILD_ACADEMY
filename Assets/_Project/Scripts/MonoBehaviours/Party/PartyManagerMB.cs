using UnityEngine;
using GuildAcademy.Core.Party;
using GuildAcademy.Data;

namespace GuildAcademy.MonoBehaviours.Party
{
    /// <summary>
    /// パーティ管理のSingleton MonoBehaviour。
    /// Inspectorで初期パーティメンバーを設定し、ストーリー進行でAdd/Removeする。
    /// </summary>
    public class PartyManagerMB : MonoBehaviour
    {
        public static PartyManagerMB Instance { get; private set; }

        [Header("Initial Party (story start members)")]
        [SerializeField] private CharacterDataSO[] _initialMembers;

        public PartyManager Party { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Party = new PartyManager();

            if (_initialMembers != null)
            {
                foreach (var so in _initialMembers)
                {
                    if (so != null)
                        Party.AddMember(so.ToCharacterStats());
                }
            }
        }

        public void AddMember(CharacterDataSO characterSO)
        {
            if (characterSO != null)
                Party.AddMember(characterSO.ToCharacterStats());
        }

        public bool RemoveMember(string characterName)
        {
            return Party.RemoveMember(characterName);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
