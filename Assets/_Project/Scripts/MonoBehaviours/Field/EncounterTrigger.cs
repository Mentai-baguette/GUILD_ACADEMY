using UnityEngine;
using System.Collections.Generic;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class EncounterTrigger : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private string _enemyName = "Enemy";
        [SerializeField] private int _enemyHp = 100;
        [SerializeField] private int _enemyMp = 30;
        [SerializeField] private int _enemyAtk = 15;
        [SerializeField] private int _enemyDef = 10;
        [SerializeField] private int _enemyAgi = 8;
        [SerializeField] private ElementType _enemyElement = ElementType.None;

        [Header("Battle Settings")]
        [SerializeField] private bool _isBossBattle;
        [SerializeField] private bool _canFlee = true;
        [SerializeField] private bool _destroyAfterBattle = true;

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
            var enemies = new List<CharacterStats>
            {
                new CharacterStats(_enemyName, _enemyHp, _enemyMp, _enemyAtk, _enemyDef, _enemyAgi, _enemyElement)
            };

            // Get current party (placeholder - in real implementation, get from PartyManager)
            var party = GetDefaultParty();

            var setup = new BattleSetupData(party, enemies,
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
        }

        private List<CharacterStats> GetDefaultParty()
        {
            // Hackathon default: 5 party members from scenario
            return new List<CharacterStats>
            {
                new CharacterStats("レイ", 450, 80, 42, 30, 28, ElementType.Dark, intStat: 25, res: 22, dex: 35, luk: 100),
                new CharacterStats("ユナ", 380, 150, 28, 25, 22, ElementType.Light, intStat: 48, res: 38, dex: 20, luk: 100),
                new CharacterStats("ミオ", 350, 120, 32, 22, 35, ElementType.Wind, intStat: 40, res: 28, dex: 30, luk: 100),
                new CharacterStats("カイト", 500, 60, 52, 38, 30, ElementType.Fire, intStat: 18, res: 20, dex: 28, luk: 100),
                new CharacterStats("シオン", 480, 100, 48, 35, 32, ElementType.Ice, intStat: 42, res: 35, dex: 38, luk: 100)
            };
        }
    }
}
