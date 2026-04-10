using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    /// <summary>
    /// バトル画面の全UI管理を行うマネージャークラス。
    /// SerializeFieldで各UIコンポーネントをInspectorから接続する前提。
    /// 未接続でもnullチェックによりクラッシュしない。
    /// </summary>
    public class BattleUIManager : MonoBehaviour
    {
        // ============================================================
        //  Inspector References
        // ============================================================

        [Header("Core References")]
        [SerializeField] private BattleManager _battleManager;

        // --- Battle Log ---
        [Header("Battle Log")]
        [SerializeField] private Text _battleLog;

        // --- Party Status (HP/MP bars) ---
        [Header("Party Status Panel")]
        [SerializeField] private Transform _partyStatusContainer;
        [SerializeField] private GameObject _partyStatusEntryPrefab;

        // --- Enemy Display ---
        [Header("Enemy Display")]
        [SerializeField] private Transform _enemyDisplayContainer;
        [SerializeField] private GameObject _enemyDisplayEntryPrefab;

        // --- ATB Order Display ---
        [Header("ATB Order Display")]
        [SerializeField] private Transform _atbOrderContainer;
        [SerializeField] private GameObject _atbOrderEntryPrefab;

        // --- Link Gauge (party-shared) ---
        [Header("Link Gauge")]
        [SerializeField] private Slider _linkGaugeSlider;
        [SerializeField] private Text _linkGaugeLabel;

        // --- Erosion Gauge (Ray exclusive) ---
        [Header("Erosion Gauge")]
        [SerializeField] private Slider _erosionGaugeSlider;
        [SerializeField] private Image _erosionGaugeFill;
        [SerializeField] private Text _erosionGaugeLabel;
        [SerializeField] private GameObject _erosionGaugeRoot;

        // --- Command Panel ---
        [Header("Command Panel")]
        [SerializeField] private GameObject _commandPanel;
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _itemButton;
        [SerializeField] private Button _defendButton;
        [SerializeField] private Button _fleeButton;
        [SerializeField] private Button _dualArtsButton;
        [SerializeField] private Button _changeButton;
        [SerializeField] private Button _swapButton;

        // --- Skill Selection Panel ---
        [Header("Skill Selection Panel")]
        [SerializeField] private GameObject _skillPanel;
        [SerializeField] private Transform _skillListContainer;
        [SerializeField] private GameObject _skillEntryPrefab;
        [SerializeField] private Button _skillBackButton;

        // --- Target Selection Panel ---
        [Header("Target Selection Panel")]
        [SerializeField] private GameObject _targetPanel;
        [SerializeField] private Transform _targetListContainer;
        [SerializeField] private GameObject _targetEntryPrefab;
        [SerializeField] private Button _targetBackButton;

        // --- Formation Display ---
        [Header("Formation Display")]
        [SerializeField] private Transform _frontRowContainer;
        [SerializeField] private Transform _backRowContainer;

        // --- Speed / Auto Controls ---
        [Header("Battle Speed & Auto")]
        [SerializeField] private Button _speedButton;
        [SerializeField] private Text _speedLabel;
        [SerializeField] private Button _autoButton;
        [SerializeField] private Text _autoLabel;

        // --- Erosion Gauge Color Thresholds ---
        [Header("Erosion Colors")]
        [SerializeField] private Color _erosionNormalColor = new Color(0.2f, 0.8f, 0.2f);   // Green
        [SerializeField] private Color _erosionUnstableColor = new Color(0.9f, 0.9f, 0.1f); // Yellow
        [SerializeField] private Color _erosionDangerousColor = new Color(1.0f, 0.5f, 0.0f); // Orange
        [SerializeField] private Color _erosionCriticalColor = new Color(0.9f, 0.1f, 0.1f); // Red

        // ============================================================
        //  Runtime State
        // ============================================================

        private CharacterStats _currentActor;
        private bool _subscribed;
        private bool _isAutoMode;
        private int _speedIndex; // 0 = 1.0x, 1 = 1.5x, 2 = 2.0x
        private readonly float[] _speedValues = { 1.0f, 1.5f, 2.0f };
        private readonly string[] _speedLabels = { "1.0x", "1.5x", "2.0x" };

        private float _linkGaugeValue; // 0-100, UI-only for now
        private float _erosionGaugeValue; // 0-100, UI-only for now

        // Pending command for target selection flow
        private CommandType _pendingCommandType;
        private SkillData _pendingSkill;
        private bool _targetSelectingAlly; // true = ally target, false = enemy target

        // Cached UI entries
        private readonly List<PartyStatusEntry> _partyStatusEntries = new List<PartyStatusEntry>();
        private readonly List<EnemyDisplayEntry> _enemyDisplayEntries = new List<EnemyDisplayEntry>();
        private readonly List<ATBOrderEntry> _atbOrderEntries = new List<ATBOrderEntry>();

        // Battle log
        private const int MaxLogLines = 50;
        private readonly Queue<string> _logLines = new Queue<string>();

        // ============================================================
        //  MonoBehaviour Lifecycle
        // ============================================================

        private void Start()
        {
            if (_battleManager == null)
                _battleManager = FindObjectOfType<BattleManager>();

            HideAllPanels();
            SetupButtons();
            TrySubscribeEvents();
            InitSpeedAndAuto();
        }

        private void Update()
        {
            // Wait for BattleManager init then subscribe
            if (_battleManager != null && !_subscribed && _battleManager.IsInitialized)
            {
                TrySubscribeEvents();
                BuildPartyStatusUI();
                BuildEnemyDisplayUI();
            }

            if (_battleManager == null || !_battleManager.IsInitialized) return;

            UpdatePartyStatusBars();
            UpdateEnemyDisplayBars();
            UpdateATBOrderDisplay();
            UpdateErosionGauge();
            UpdateLinkGauge();
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

            // Restore timescale
            Time.timeScale = 1.0f;
        }

        // ============================================================
        //  Initialization
        // ============================================================

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
            // Command buttons
            if (_attackButton != null) _attackButton.onClick.AddListener(OnAttackPressed);
            if (_skillButton != null) _skillButton.onClick.AddListener(OnSkillPressed);
            if (_itemButton != null) _itemButton.onClick.AddListener(OnItemPressed);
            if (_defendButton != null) _defendButton.onClick.AddListener(OnDefendPressed);
            if (_fleeButton != null) _fleeButton.onClick.AddListener(OnFleePressed);
            if (_dualArtsButton != null) _dualArtsButton.onClick.AddListener(OnDualArtsPressed);
            if (_changeButton != null) _changeButton.onClick.AddListener(OnChangePressed);
            if (_swapButton != null) _swapButton.onClick.AddListener(OnSwapPressed);

            // Sub-panel buttons
            if (_skillBackButton != null) _skillBackButton.onClick.AddListener(OnSkillBackPressed);
            if (_targetBackButton != null) _targetBackButton.onClick.AddListener(OnTargetBackPressed);

            // Speed / Auto
            if (_speedButton != null) _speedButton.onClick.AddListener(OnSpeedPressed);
            if (_autoButton != null) _autoButton.onClick.AddListener(OnAutoPressed);
        }

        private void HideAllPanels()
        {
            SetActive(_commandPanel, false);
            SetActive(_skillPanel, false);
            SetActive(_targetPanel, false);
        }

        private void InitSpeedAndAuto()
        {
            _speedIndex = 0;
            _isAutoMode = false;
            Time.timeScale = 1.0f;
            UpdateSpeedLabel();
            UpdateAutoLabel();
        }

        // ============================================================
        //  UI Building (dynamic entries)
        // ============================================================

        private bool _partyUIBuilt;
        private bool _enemyUIBuilt;

        private void BuildPartyStatusUI()
        {
            if (_partyUIBuilt) return;
            if (_battleManager.Party == null || _battleManager.Party.Count == 0) return;
            _partyUIBuilt = true;

            if (_partyStatusContainer == null || _partyStatusEntryPrefab == null) return;

            ClearChildren(_partyStatusContainer);
            _partyStatusEntries.Clear();

            foreach (var member in _battleManager.Party)
            {
                var go = Instantiate(_partyStatusEntryPrefab, _partyStatusContainer);
                var entry = go.GetComponent<PartyStatusEntry>();
                if (entry != null)
                {
                    entry.Initialize(member);
                    _partyStatusEntries.Add(entry);
                }
            }
        }

        private void BuildEnemyDisplayUI()
        {
            if (_enemyUIBuilt) return;
            if (_battleManager.Enemies == null || _battleManager.Enemies.Count == 0) return;
            _enemyUIBuilt = true;

            if (_enemyDisplayContainer == null || _enemyDisplayEntryPrefab == null) return;

            ClearChildren(_enemyDisplayContainer);
            _enemyDisplayEntries.Clear();

            foreach (var enemy in _battleManager.Enemies)
            {
                var go = Instantiate(_enemyDisplayEntryPrefab, _enemyDisplayContainer);
                var entry = go.GetComponent<EnemyDisplayEntry>();
                if (entry != null)
                {
                    entry.Initialize(enemy, _battleManager.Break);
                    _enemyDisplayEntries.Add(entry);
                }
            }
        }

        // ============================================================
        //  Per-Frame Updates
        // ============================================================

        private void UpdatePartyStatusBars()
        {
            foreach (var entry in _partyStatusEntries)
            {
                if (entry != null) entry.UpdateDisplay();
            }
        }

        private void UpdateEnemyDisplayBars()
        {
            foreach (var entry in _enemyDisplayEntries)
            {
                if (entry != null) entry.UpdateDisplay();
            }
        }

        private void UpdateATBOrderDisplay()
        {
            if (_atbOrderContainer == null || _atbOrderEntryPrefab == null) return;
            if (_battleManager.ATB == null) return;

            // Gather all alive combatants with their gauge values
            var allCombatants = new List<(CharacterStats stats, float gauge, bool isEnemy)>();

            if (_battleManager.Party != null)
            {
                foreach (var member in _battleManager.Party)
                {
                    if (member.CurrentHp > 0)
                        allCombatants.Add((member, _battleManager.ATB.GetGauge(member), false));
                }
            }

            if (_battleManager.Enemies != null)
            {
                foreach (var enemy in _battleManager.Enemies)
                {
                    if (enemy.CurrentHp > 0)
                        allCombatants.Add((enemy, _battleManager.ATB.GetGauge(enemy), true));
                }
            }

            // Sort by gauge descending (closest to acting first)
            allCombatants.Sort((a, b) => b.gauge.CompareTo(a.gauge));

            // Ensure we have enough entries
            while (_atbOrderEntries.Count < allCombatants.Count)
            {
                var go = Instantiate(_atbOrderEntryPrefab, _atbOrderContainer);
                var entry = go.GetComponent<ATBOrderEntry>();
                if (entry != null) _atbOrderEntries.Add(entry);
                else { Destroy(go); break; }
            }

            // Update existing entries
            for (int i = 0; i < _atbOrderEntries.Count; i++)
            {
                if (i < allCombatants.Count)
                {
                    _atbOrderEntries[i].gameObject.SetActive(true);
                    _atbOrderEntries[i].UpdateDisplay(
                        allCombatants[i].stats.Name,
                        allCombatants[i].gauge / ATBSystem.MaxGauge,
                        allCombatants[i].isEnemy);
                }
                else
                {
                    _atbOrderEntries[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateErosionGauge()
        {
            // Erosion gauge is Ray-exclusive, UI-only for now
            // Future: connect to actual erosion system
            if (_erosionGaugeRoot != null)
            {
                // Show only if a character named "レイ" is in the party
                bool hasRay = false;
                if (_battleManager.Party != null)
                {
                    foreach (var member in _battleManager.Party)
                    {
                        if (member.Name == "レイ") { hasRay = true; break; }
                    }
                }
                _erosionGaugeRoot.SetActive(hasRay);
            }

            if (_erosionGaugeSlider != null)
            {
                _erosionGaugeSlider.minValue = 0f;
                _erosionGaugeSlider.maxValue = 100f;
                _erosionGaugeSlider.value = _erosionGaugeValue;
            }

            // 4-stage color
            if (_erosionGaugeFill != null)
            {
                float pct = _erosionGaugeValue;
                if (pct < 25f)
                    _erosionGaugeFill.color = _erosionNormalColor;
                else if (pct < 50f)
                    _erosionGaugeFill.color = _erosionUnstableColor;
                else if (pct < 75f)
                    _erosionGaugeFill.color = _erosionDangerousColor;
                else
                    _erosionGaugeFill.color = _erosionCriticalColor;
            }

            if (_erosionGaugeLabel != null)
            {
                string stage;
                if (_erosionGaugeValue < 25f) stage = "Normal";
                else if (_erosionGaugeValue < 50f) stage = "Unstable";
                else if (_erosionGaugeValue < 75f) stage = "Dangerous";
                else stage = "Critical";
                _erosionGaugeLabel.text = $"侵蝕: {_erosionGaugeValue:F0}% ({stage})";
            }
        }

        private void UpdateLinkGauge()
        {
            // Link gauge is party-shared, UI-only for now
            // Future: connect to SoulLinkSystem
            if (_linkGaugeSlider != null)
            {
                _linkGaugeSlider.minValue = 0f;
                _linkGaugeSlider.maxValue = 100f;
                _linkGaugeSlider.value = _linkGaugeValue;
            }

            if (_linkGaugeLabel != null)
            {
                _linkGaugeLabel.text = $"LINK: {_linkGaugeValue:F0}%";
            }
        }

        // ============================================================
        //  Public API (for external systems to update gauges)
        // ============================================================

        /// <summary>侵蝕ゲージを外部から設定 (0-100)</summary>
        public void SetErosionGauge(float value)
        {
            _erosionGaugeValue = Mathf.Clamp(value, 0f, 100f);
        }

        /// <summary>リンクゲージを外部から設定 (0-100)</summary>
        public void SetLinkGauge(float value)
        {
            _linkGaugeValue = Mathf.Clamp(value, 0f, 100f);
        }

        // ============================================================
        //  Battle Flow Event Handlers
        // ============================================================

        private void OnBattleStart()
        {
            Log("バトル開始！");
        }

        private void OnCommandRequired(CharacterStats actor)
        {
            // Only show command UI for party members (enemies handled by AI)
            if (_battleManager.Enemies != null && _battleManager.Enemies.Contains(actor))
                return;

            _currentActor = actor;

            // Auto-battle: automatically attack first alive enemy
            if (_isAutoMode)
            {
                ExecuteAutoAttack();
                return;
            }

            ShowCommandPanel();
            Log($"{actor.Name}のターン");
        }

        private void OnActionExecuted(ActionResult result)
        {
            if (result == null) return;

            // Defend action
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
            HideAllPanels();
            _currentActor = null;

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

        // ============================================================
        //  Command Button Handlers
        // ============================================================

        private void OnAttackPressed()
        {
            if (_currentActor == null) return;
            _pendingCommandType = CommandType.Attack;
            _pendingSkill = null;
            _targetSelectingAlly = false;
            ShowTargetPanel(false);
        }

        private void OnSkillPressed()
        {
            if (_currentActor == null) return;
            ShowSkillPanel();
        }

        private void OnItemPressed()
        {
            if (_currentActor == null) return;
            // Item system not yet implemented; log message
            Log("アイテムはまだ使用できません");
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

            SubmitAndHide(command);
        }

        private void OnFleePressed()
        {
            if (_currentActor == null) return;
            if (_battleManager == null || _battleManager.BattleFlow == null) return;

            if (_battleManager.Setup != null && !_battleManager.Setup.CanFlee)
            {
                Log("逃げられない！");
                return;
            }

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = _currentActor,
                Type = CommandType.Flee
            };

            _battleManager.SubmitPlayerCommand(command);
            if (_commandPanel != null) _commandPanel.SetActive(false);
            _currentActor = null;

            // Fleeの実際のシーン遷移はBattleManager.SubmitPlayerCommand内で処理される
        }

        private void OnDualArtsPressed()
        {
            if (_currentActor == null) return;
            // Dual Arts (DA) not yet implemented
            Log("★DAはまだ使用できません");
        }

        private void OnChangePressed()
        {
            if (_currentActor == null) return;
            // Change formation row during battle
            Log($"{_currentActor.Name}は隊列を変更した");

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = _currentActor,
                Type = CommandType.Change
            };

            SubmitAndHide(command);
        }

        private void OnSwapPressed()
        {
            if (_currentActor == null) return;
            // Swap party member - show ally target selection
            _pendingCommandType = CommandType.Swap;
            _pendingSkill = null;
            _targetSelectingAlly = true;
            ShowTargetPanel(true);
        }

        // ============================================================
        //  Skill Selection
        // ============================================================

        private void ShowSkillPanel()
        {
            SetActive(_commandPanel, false);
            SetActive(_skillPanel, true);

            if (_skillListContainer == null || _skillEntryPrefab == null) return;

            ClearChildren(_skillListContainer);

            // Get skills for current actor (placeholder: use EnemySkills list or empty)
            // In future, each CharacterStats will have their own skill list
            var skills = GetActorSkills(_currentActor);
            if (skills == null || skills.Count == 0)
            {
                Log("使用できるスキルがありません");
                OnSkillBackPressed();
                return;
            }

            foreach (var skill in skills)
            {
                var go = Instantiate(_skillEntryPrefab, _skillListContainer);
                var btn = go.GetComponent<Button>();
                var label = go.GetComponentInChildren<Text>();

                if (label != null)
                    label.text = $"{skill.Name} (MP:{skill.MpCost})";

                if (btn != null)
                {
                    var captured = skill;
                    btn.onClick.AddListener(() => OnSkillSelected(captured));

                    // Disable if not enough MP
                    if (_currentActor.CurrentMp < skill.MpCost)
                        btn.interactable = false;
                }
            }
        }

        private void OnSkillSelected(SkillData skill)
        {
            _pendingCommandType = CommandType.Skill;
            _pendingSkill = skill;

            // Determine target type
            switch (skill.TargetType)
            {
                case SkillTargetType.SingleEnemy:
                    _targetSelectingAlly = false;
                    ShowTargetPanel(false);
                    break;
                case SkillTargetType.SingleAlly:
                    _targetSelectingAlly = true;
                    ShowTargetPanel(true);
                    break;
                case SkillTargetType.Self:
                    // Auto-target self
                    ExecuteSkillCommand(_currentActor);
                    break;
                case SkillTargetType.AllEnemies:
                    // Target first enemy (AoE handled by executor)
                    var firstEnemy = FindFirstAliveEnemy();
                    if (firstEnemy != null) ExecuteSkillCommand(firstEnemy);
                    break;
                case SkillTargetType.AllAllies:
                    // Target self (AoE handled by executor)
                    ExecuteSkillCommand(_currentActor);
                    break;
                default:
                    _targetSelectingAlly = false;
                    ShowTargetPanel(false);
                    break;
            }
        }

        private void OnSkillBackPressed()
        {
            SetActive(_skillPanel, false);
            ShowCommandPanel();
        }

        private List<SkillData> GetActorSkills(CharacterStats actor)
        {
            // Placeholder: in future, each character has their own skill list
            // For now, return EnemySkills if actor is enemy, or empty list
            if (_battleManager.Setup != null && _battleManager.Setup.EnemySkills != null
                && _battleManager.Enemies != null && _battleManager.Enemies.Contains(actor))
            {
                return _battleManager.Setup.EnemySkills;
            }

            // Return empty list for party members until skill system is connected
            return new List<SkillData>();
        }

        // ============================================================
        //  Target Selection
        // ============================================================

        private void ShowTargetPanel(bool selectAlly)
        {
            SetActive(_commandPanel, false);
            SetActive(_skillPanel, false);
            SetActive(_targetPanel, true);

            if (_targetListContainer == null || _targetEntryPrefab == null) return;

            ClearChildren(_targetListContainer);

            var candidates = selectAlly ? _battleManager.Party : _battleManager.Enemies;
            if (candidates == null) return;

            foreach (var candidate in candidates)
            {
                if (candidate.CurrentHp <= 0) continue;

                var go = Instantiate(_targetEntryPrefab, _targetListContainer);
                var btn = go.GetComponent<Button>();
                var label = go.GetComponentInChildren<Text>();

                if (label != null)
                {
                    string hpInfo = $"{candidate.Name} HP:{candidate.CurrentHp}/{candidate.MaxHp}";
                    label.text = hpInfo;
                }

                if (btn != null)
                {
                    var captured = candidate;
                    btn.onClick.AddListener(() => OnTargetSelected(captured));
                }
            }
        }

        private void OnTargetSelected(CharacterStats target)
        {
            SetActive(_targetPanel, false);

            switch (_pendingCommandType)
            {
                case CommandType.Attack:
                    ExecuteAttackCommand(target);
                    break;
                case CommandType.Skill:
                    ExecuteSkillCommand(target);
                    break;
                case CommandType.Swap:
                    ExecuteSwapCommand(target);
                    break;
                default:
                    // Fallback: treat as attack
                    ExecuteAttackCommand(target);
                    break;
            }
        }

        private void OnTargetBackPressed()
        {
            SetActive(_targetPanel, false);

            // Return to skill panel if we were selecting a skill target, otherwise command panel
            if (_pendingCommandType == CommandType.Skill && _pendingSkill != null)
            {
                ShowSkillPanel();
            }
            else
            {
                ShowCommandPanel();
            }
        }

        // ============================================================
        //  Command Execution
        // ============================================================

        private void ExecuteAttackCommand(CharacterStats target)
        {
            if (_currentActor == null || target == null) return;

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = target,
                Type = CommandType.Attack,
                Element = _currentActor.Element
            };

            SubmitAndHide(command);
        }

        private void ExecuteSkillCommand(CharacterStats target)
        {
            if (_currentActor == null || target == null || _pendingSkill == null) return;

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = target,
                Type = CommandType.Skill,
                Element = _pendingSkill.Element,
                SkillPower = _pendingSkill.Power,
                MpCost = _pendingSkill.MpCost,
                IsMagic = _pendingSkill.IsMagic
            };

            SubmitAndHide(command);
        }

        private void ExecuteSwapCommand(CharacterStats target)
        {
            if (_currentActor == null || target == null) return;

            Log($"{_currentActor.Name}と{target.Name}を入れ替えた");

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = target,
                Type = CommandType.Swap
            };

            SubmitAndHide(command);
        }

        private void ExecuteAutoAttack()
        {
            if (_currentActor == null) return;

            var target = FindFirstAliveEnemy();
            if (target == null) return;

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = target,
                Type = CommandType.Attack,
                Element = _currentActor.Element
            };

            SubmitAndHide(command);
        }

        private void SubmitAndHide(BattleCommand command)
        {
            _battleManager.SubmitPlayerCommand(command);
            HideAllPanels();
            _currentActor = null;
            _pendingSkill = null;
        }

        // ============================================================
        //  Speed & Auto Battle
        // ============================================================

        private void OnSpeedPressed()
        {
            _speedIndex = (_speedIndex + 1) % _speedValues.Length;
            Time.timeScale = _speedValues[_speedIndex];
            UpdateSpeedLabel();
        }

        private void OnAutoPressed()
        {
            _isAutoMode = !_isAutoMode;
            UpdateAutoLabel();

            // If auto mode just turned on and we're waiting for command, auto-attack immediately
            if (_isAutoMode && _currentActor != null)
            {
                ExecuteAutoAttack();
            }
        }

        private void UpdateSpeedLabel()
        {
            if (_speedLabel != null)
                _speedLabel.text = _speedLabels[_speedIndex];
        }

        private void UpdateAutoLabel()
        {
            if (_autoLabel != null)
                _autoLabel.text = _isAutoMode ? "AUTO: ON" : "AUTO: OFF";
        }

        // ============================================================
        //  Panel Visibility Helpers
        // ============================================================

        private void ShowCommandPanel()
        {
            SetActive(_commandPanel, true);
            SetActive(_skillPanel, false);
            SetActive(_targetPanel, false);
        }

        // ============================================================
        //  Battle Log
        // ============================================================

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

        // ============================================================
        //  Utility
        // ============================================================

        private CharacterStats FindFirstAliveEnemy()
        {
            if (_battleManager.Enemies == null) return null;
            foreach (var enemy in _battleManager.Enemies)
            {
                if (enemy.CurrentHp > 0) return enemy;
            }
            return null;
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
