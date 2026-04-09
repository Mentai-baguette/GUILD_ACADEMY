using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    public class BattleUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager _battleManager;

        [Header("UI Elements")]
        [SerializeField] private Text _battleLog;
        [SerializeField] private GameObject _commandPanel;
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _defendButton;
        [SerializeField] private Button _fleeButton;

        private CharacterStats _currentActor;

        private void Start()
        {
            if (_battleManager == null)
                _battleManager = FindObjectOfType<BattleManager>();

            if (_commandPanel != null)
                _commandPanel.SetActive(false);

            SetupButtons();
            TrySubscribeEvents();
        }

        private void Update()
        {
            // BattleManagerの初期化を待ってからイベント購読（Start順問題の対策）
            if (_battleManager != null && !_subscribed && _battleManager.IsInitialized)
            {
                TrySubscribeEvents();
            }
        }

        private bool _subscribed;

        private void TrySubscribeEvents()
        {
            if (_subscribed || _battleManager == null || _battleManager.BattleFlow == null) return;

            _battleManager.BattleFlow.OnCommandRequired += OnCommandRequired;
            _battleManager.BattleFlow.OnActionExecuted += OnActionExecuted;
            _battleManager.BattleFlow.OnBattleEnd += OnBattleEnd;
            _battleManager.BattleFlow.OnBattleStart += OnBattleStart;
            _subscribed = true;
        }

        private void SetupButtons()
        {
            if (_attackButton != null) _attackButton.onClick.AddListener(OnAttackPressed);
            if (_defendButton != null) _defendButton.onClick.AddListener(OnDefendPressed);
            if (_fleeButton != null) _fleeButton.onClick.AddListener(OnFleePressed);
        }

        private void OnBattleStart()
        {
            Log("バトル開始！");
        }

        private void OnCommandRequired(CharacterStats actor)
        {
            // Only show command UI for party members (enemies are handled by AI)
            if (_battleManager.Setup != null && _battleManager.Setup.Enemies.Contains(actor))
                return;

            _currentActor = actor;
            if (_commandPanel != null) _commandPanel.SetActive(true);
            Log($"{actor.Name}のターン");
        }

        private void OnAttackPressed()
        {
            if (_currentActor == null) return;

            // Find first alive enemy
            CharacterStats target = null;
            if (_battleManager.Setup != null)
            {
                foreach (var enemy in _battleManager.Setup.Enemies)
                {
                    if (enemy.CurrentHp > 0) { target = enemy; break; }
                }
            }
            if (target == null) return;

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = target,
                Type = CommandType.Attack,
                Element = _currentActor.Element
            };

            _battleManager.SubmitPlayerCommand(command);
            if (_commandPanel != null) _commandPanel.SetActive(false);
            _currentActor = null;
        }

        private void OnDefendPressed()
        {
            if (_currentActor == null) return;

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = _currentActor,
                Type = CommandType.Defend
            };

            _battleManager.SubmitPlayerCommand(command);
            if (_commandPanel != null) _commandPanel.SetActive(false);
            _currentActor = null;
        }

        private void OnFleePressed()
        {
            if (_battleManager.Setup != null && !_battleManager.Setup.CanFlee)
            {
                Log("逃げられない！");
                return;
            }

            // For hackathon, flee always succeeds for non-boss
            Log("逃げた！");
            if (_battleManager.Setup == null) return;

            BattleSetupData.Current = null;
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(_battleManager.Setup.ReturnSceneName);
            }
            else
            {
                Debug.LogWarning("[BattleUI] SceneTransitionManager not found. Falling back to direct load.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(_battleManager.Setup.ReturnSceneName);
            }
        }

        private void OnActionExecuted(ActionResult result)
        {
            // Defend: attacker targets self and no damage/heal
            if (result.Attacker == result.Target && result.DamageDealt == 0 && result.HealAmount == 0)
            {
                Log($"{result.Attacker.Name}は防御した");
                return;
            }

            string msg = $"{result.Attacker.Name}の攻撃！ ";
            if (result.WasCritical) msg += "クリティカル！ ";
            if (result.WasWeakHit) msg += "弱点！ ";
            if (result.DamageDealt > 0) msg += $"{result.Target.Name}に{result.DamageDealt}ダメージ！";
            if (result.HealAmount > 0) msg += $"{result.Target.Name}のHPが{result.HealAmount}回復！";
            if (result.TriggeredBreak) msg += " BREAK！";
            if (result.TargetDefeated) msg += $" {result.Target.Name}を倒した！";
            Log(msg);
        }

        private void OnBattleEnd(BattleResult result)
        {
            if (_commandPanel != null) _commandPanel.SetActive(false);

            switch (result)
            {
                case BattleResult.PlayerVictory:
                    Log("勝利！");
                    break;
                case BattleResult.EnemyVictory:
                    Log("全滅...");
                    break;
                case BattleResult.AllDefeated:
                    Log("相打ち...");
                    break;
            }
        }

        private const int MaxLogLines = 50;
        private readonly System.Collections.Generic.Queue<string> _logLines = new System.Collections.Generic.Queue<string>();

        private void Log(string message)
        {
            _logLines.Enqueue(message);
            while (_logLines.Count > MaxLogLines)
                _logLines.Dequeue();

            if (_battleLog != null)
            {
                var lines = _logLines.ToArray();
                System.Array.Reverse(lines);
                _battleLog.text = string.Join("\n", lines);
            }
            Debug.Log($"[Battle] {message}");
        }

        private void OnDestroy()
        {
            if (_battleManager != null && _battleManager.BattleFlow != null)
            {
                _battleManager.BattleFlow.OnCommandRequired -= OnCommandRequired;
                _battleManager.BattleFlow.OnActionExecuted -= OnActionExecuted;
                _battleManager.BattleFlow.OnBattleEnd -= OnBattleEnd;
                _battleManager.BattleFlow.OnBattleStart -= OnBattleStart;
            }
        }
    }
}
