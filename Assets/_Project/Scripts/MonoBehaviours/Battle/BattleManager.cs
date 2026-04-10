using UnityEngine;
using System.Collections.Generic;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    public class BattleManager : MonoBehaviour
    {
        private BattleFlowController _battleFlow;
        private ATBSystem _atb;
        private ActionExecutor _executor;
        private BreakSystem _breakSystem;
        private DamageCalculator _damageCalc;
        private IEnemyAI _enemyAI;
        private IRandom _random;

        private BattleSetupData _setup;
        private List<CharacterStats> _party;
        private List<CharacterStats> _enemies;

        public BattleFlowController BattleFlow => _battleFlow;
        public BattleSetupData Setup => _setup;
        public IReadOnlyList<CharacterStats> Party => _party;
        public IReadOnlyList<CharacterStats> Enemies => _enemies;

        public event System.Action<BattleResult> OnBattleFinished;

        public bool IsInitialized => _battleFlow != null;

        private void Start()
        {
            _setup = BattleSetupData.Current;
            if (_setup == null)
            {
                Debug.LogError("[BattleManager] BattleSetupData.Current is null. Cannot start battle.");
                return;
            }

            _party = _setup.Party;
            _enemies = _setup.Enemies;

            InitializeSystems();
            StartBattle();
        }

        private void InitializeSystems()
        {
            _random = new UnityRandom();
            _damageCalc = new DamageCalculator(_random);
            _breakSystem = new BreakSystem();
            _executor = new ActionExecutor(_damageCalc, _breakSystem, _random);
            _atb = new ATBSystem();
            _battleFlow = new BattleFlowController(_atb, _executor, _breakSystem);
            _enemyAI = new BasicEnemyAI();

            _battleFlow.OnCommandRequired += OnCommandRequired;
            _battleFlow.OnBattleEnd += OnBattleEnd;
        }

        private void StartBattle()
        {
            _battleFlow.StartBattle(_party, _enemies);
        }

        private void Update()
        {
            if (_battleFlow != null && _battleFlow.State == BattleFlowState.TickingATB)
            {
                _battleFlow.Tick(Time.deltaTime);
            }
        }

        private void OnCommandRequired(CharacterStats actor)
        {
            // Check if the actor is an enemy
            if (_enemies.Contains(actor))
            {
                var context = new EnemyAIContext
                {
                    Actor = actor,
                    Party = _party,
                    Enemies = _enemies,
                    BreakSystem = _breakSystem,
                    AvailableSkills = _setup.EnemySkills,
                    Random = _random
                };
                var command = _enemyAI.DecideAction(context);
                _battleFlow.SubmitCommand(command);
            }
            // Party member commands will be handled by BattleUIManager
        }

        private void OnBattleEnd(BattleResult result)
        {
            OnBattleFinished?.Invoke(result);

            // Return to field after a delay
            StartCoroutine(ReturnToField(result));
        }

        private System.Collections.IEnumerator ReturnToField(BattleResult result)
        {
            yield return new WaitForSeconds(2f);

            BattleSetupData.Current = null;

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(_setup.ReturnSceneName);
            }
            else
            {
                Debug.LogWarning("[BattleManager] SceneTransitionManager not found. Falling back to direct load.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(_setup.ReturnSceneName);
            }
        }

        /// <summary>
        /// Submit a command from the UI (for party members).
        /// </summary>
        public ActionResult SubmitPlayerCommand(BattleCommand command)
        {
            if (_battleFlow.State != BattleFlowState.WaitingForCommand) return null;
            var result = _battleFlow.SubmitCommand(command);

            // Flee成功時はフィールドに戻る
            if (command.Type == CommandType.Flee)
            {
                StartCoroutine(ReturnToField(BattleResult.PlayerVictory));
            }

            return result;
        }

        private void OnDestroy()
        {
            if (_battleFlow != null)
            {
                _battleFlow.OnCommandRequired -= OnCommandRequired;
                _battleFlow.OnBattleEnd -= OnBattleEnd;
            }
        }
    }

    /// <summary>
    /// Unity's Random.Range wrapper implementing IRandom.
    /// </summary>
    public class UnityRandom : IRandom
    {
        public int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
