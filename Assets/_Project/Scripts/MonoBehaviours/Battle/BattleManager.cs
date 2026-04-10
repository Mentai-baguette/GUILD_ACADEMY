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
        private FormationSystem _formationSystem;
        private IEnemyAI _enemyAI;
        private IRandom _random;

        private BattleSetupData _setup;
        private List<CharacterStats> _party;       // バトル参加中メンバー
        private List<CharacterStats> _reserves;     // 控えメンバー
        private List<CharacterStats> _enemies;
        private bool _fleeing;

        public BattleFlowController BattleFlow => _battleFlow;
        public BattleSetupData Setup => _setup;
        public ATBSystem ATB => _atb;
        public BreakSystem Break => _breakSystem;
        public FormationSystem Formation => _formationSystem;
        public IReadOnlyList<CharacterStats> Party => _party;
        public IReadOnlyList<CharacterStats> Reserves => _reserves;
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
            _reserves = _setup.Reserves ?? new List<CharacterStats>();
            _enemies = _setup.Enemies;

            InitializeSystems();
            StartBattle();
        }

        private void InitializeSystems()
        {
            _random = new UnityRandom();
            _damageCalc = new DamageCalculator(_random);
            _breakSystem = new BreakSystem();
            _formationSystem = new FormationSystem();
            _executor = new ActionExecutor(_damageCalc, _breakSystem, _random);
            _atb = new ATBSystem();
            _battleFlow = new BattleFlowController(_atb, _executor, _breakSystem);
            _enemyAI = new BasicEnemyAI();

            // パーティメンバーの初期隊列を前列に設定
            foreach (var member in _party)
                _formationSystem.SetRow(member, FormationRow.Front);

            _battleFlow.OnCommandRequired += OnCommandRequired;
            _battleFlow.OnBattleEnd += OnBattleEnd;
        }

        private void StartBattle()
        {
            _battleFlow.StartBattle(_party, _enemies);
        }

        private void Update()
        {
            if (_battleFlow != null && _battleFlow.State == BattleFlowState.TickingATB && !_fleeing)
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

            // PlayerVictory時はBattleResultUIがフィールド復帰を制御するため、
            // ここではリザルト画面が無い場合のみ自動復帰する
            if (result != BattleResult.PlayerVictory)
            {
                StartCoroutine(ReturnToField(result));
            }
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

            // Change: 隊列切替の実処理
            if (command.Type == CommandType.Change && _formationSystem != null)
            {
                _formationSystem.ChangeRow(command.Attacker);
                Debug.Log($"[BattleManager] {command.Attacker.Name} → {_formationSystem.GetRow(command.Attacker)}");
            }

            // Swap: バトルメンバー⇔控えメンバーの入替（FF10式）
            if (command.Type == CommandType.Swap && command.Target != null)
            {
                int partyIdx = _party.IndexOf(command.Attacker);
                int reserveIdx = _reserves.IndexOf(command.Target);

                if (partyIdx >= 0 && reserveIdx >= 0)
                {
                    // バトルメンバーから外して控えに、控えからバトルに
                    _party[partyIdx] = command.Target;
                    _reserves[reserveIdx] = command.Attacker;

                    // 入場メンバーのATBゲージをリセット、ATBシステムに登録
                    _atb.ResetGauge(command.Target);
                    _atb.AddCombatant(command.Target);
                    _atb.RemoveCombatant(command.Attacker);
                    _breakSystem.Register(command.Target);

                    Debug.Log($"[BattleManager] Swap: {command.Attacker.Name}(→控え) ⇔ {command.Target.Name}(→バトル)");
                }
                else
                {
                    Debug.LogWarning($"[BattleManager] Swap failed: {command.Attacker.Name} not in party or {command.Target.Name} not in reserves");
                }
            }

            var result = _battleFlow.SubmitCommand(command);

            // Flee: 逃走禁止戦闘ではBattleManager側でもブロック
            if (command.Type == CommandType.Flee)
            {
                if (_setup != null && !_setup.CanFlee)
                {
                    // 逃走禁止戦闘では無視
                    return result;
                }

                _fleeing = true;
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
