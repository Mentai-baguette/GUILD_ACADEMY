using UnityEngine;
using System.Collections.Generic;
using GuildAcademy.Core.Data;
using GuildAcademy.Data;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class EncounterTrigger : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private EnemyDataSO[] _enemies;

        [Header("Party (leave empty to use default party SO)")]
        [SerializeField] private CharacterDataSO[] _partyOverride;

        [Header("Battle Settings")]
        [SerializeField] private bool _isBossBattle;
        [SerializeField] private bool _canFlee = true;
        [SerializeField] private bool _destroyAfterBattle = true;

        private const string DefaultPartyPath = "Data/Characters";

        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            StartEncounter();
        }

        public void StartEncounter()
        {
            var enemyStats = new List<CharacterStats>();
            if (_enemies != null && _enemies.Length > 0)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy != null)
                        enemyStats.Add(enemy.ToCharacterStats());
                }
            }

            if (enemyStats.Count == 0)
            {
                Debug.LogWarning("[EncounterTrigger] No enemies configured.");
                _triggered = false;
                return;
            }

            var party = GetParty();

            var setup = new BattleSetupData(party, enemyStats,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                IsBossBattle = _isBossBattle,
                CanFlee = _canFlee
            };

            BattleSetupData.Current = setup;

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(SceneNames.Battle);
            }
            else
            {
                Debug.LogWarning("[EncounterTrigger] SceneTransitionManager not found. Falling back to direct load.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Battle);
            }
        }

        public void OnBattleReturned()
        {
            if (_destroyAfterBattle)
            {
                Destroy(gameObject);
            }
            else
            {
                _triggered = false;
            }
        }

        private List<CharacterStats> GetParty()
        {
            var sources = (_partyOverride != null && _partyOverride.Length > 0)
                ? _partyOverride
                : Resources.LoadAll<CharacterDataSO>(DefaultPartyPath);

            var party = new List<CharacterStats>();
            foreach (var so in sources)
            {
                if (so != null)
                    party.Add(so.ToCharacterStats());
            }

            if (party.Count == 0)
            {
                Debug.LogWarning("[EncounterTrigger] No party data found. Using fallback.");
                party.Add(new CharacterStats("レイ", 105, 25, 12, 10, 10, ElementType.Dark));
            }

            return party;
        }
    }
}
