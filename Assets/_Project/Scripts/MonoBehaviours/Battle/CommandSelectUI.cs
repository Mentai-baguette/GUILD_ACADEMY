using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.Audio;

namespace GuildAcademy.MonoBehaviours.Battle
{
    /// <summary>
    /// バトル中のコマンド選択UIコンポーネント。
    /// キーボード操作対応・カーソル選択・スキルリスト表示・ターゲット選択を管理する。
    /// CommandSelectLogic（Pure C#）をラップし、Unity UIとの橋渡しを行う。
    /// </summary>
    public class CommandSelectUI : MonoBehaviour
    {
        // ============================================================
        //  Inspector References
        // ============================================================

        [Header("Core")]
        [SerializeField] private BattleManager _battleManager;

        [Header("Command Panel")]
        [SerializeField] private GameObject _commandPanel;
        [SerializeField] private Text[] _commandLabels = new Text[8];
        [SerializeField] private Image _cursorImage;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = new Color(1f, 1f, 0.5f);
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("Skill Panel")]
        [SerializeField] private GameObject _skillPanel;
        [SerializeField] private Transform _skillListContainer;
        [SerializeField] private GameObject _skillEntryPrefab;

        [Header("Target Panel")]
        [SerializeField] private GameObject _targetPanel;
        [SerializeField] private Transform _targetListContainer;
        [SerializeField] private GameObject _targetEntryPrefab;

        [Header("SE")]
        [SerializeField] private AudioClip _seCursorMove;
        [SerializeField] private AudioClip _seConfirm;
        [SerializeField] private AudioClip _seCancel;

        // ============================================================
        //  Events
        // ============================================================

        /// <summary>コマンド決定時に発火するイベント</summary>
        public event Action<BattleCommand> OnCommandDecided;

        // ============================================================
        //  Runtime
        // ============================================================

        private CommandSelectLogic _logic;
        private List<Text> _skillEntryLabels = new List<Text>();
        private List<Button> _skillEntryButtons = new List<Button>();
        private List<Text> _targetEntryLabels = new List<Text>();
        private List<Button> _targetEntryButtons = new List<Button>();

        private static readonly string[] CommandDisplayNames =
        {
            "たたかう",
            "スキル",
            "アイテム",
            "ぼうぎょ",
            "にげる",
            "デュアルアーツ",
            "チェンジ",
            "スワップ"
        };

        // ============================================================
        //  MonoBehaviour Lifecycle
        // ============================================================

        private void Awake()
        {
            _logic = new CommandSelectLogic();

            // Subscribe to logic events
            _logic.OnCursorMoved += HandleCursorMoved;
            _logic.OnCommandDecided += HandleCommandDecided;
            _logic.OnCancelled += HandleCancelled;
            _logic.OnConfirmed += HandleConfirmed;
            _logic.OnPhaseChanged += HandlePhaseChanged;

            HideAll();
        }

        private void OnDestroy()
        {
            if (_logic != null)
            {
                _logic.OnCursorMoved -= HandleCursorMoved;
                _logic.OnCommandDecided -= HandleCommandDecided;
                _logic.OnCancelled -= HandleCancelled;
                _logic.OnConfirmed -= HandleConfirmed;
                _logic.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void Update()
        {
            if (_logic == null || _logic.CurrentPhase == CommandSelectLogic.Phase.Inactive) return;

            HandleKeyboardInput();
        }

        // ============================================================
        //  Public API
        // ============================================================

        /// <summary>コマンド選択を開始する</summary>
        public void BeginCommandSelect(CharacterStats actor, bool canFlee = true)
        {
            if (actor == null) return;
            _logic.Begin(actor, canFlee);
            RefreshCommandPanel();
        }

        /// <summary>コマンド選択をキャンセルして非表示にする</summary>
        public void Hide()
        {
            _logic.Deactivate();
            HideAll();
        }

        /// <summary>テスト用: 内部ロジックへの参照を返す</summary>
        internal CommandSelectLogic Logic => _logic;

        // ============================================================
        //  Keyboard Input
        // ============================================================

        private void HandleKeyboardInput()
        {
            // Up: ↑ or W
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _logic.MoveCursorUp();
            }
            // Down: ↓ or S
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _logic.MoveCursorDown();
            }
            // Confirm: Enter or Space
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ProcessConfirm();
            }
            // Cancel: Escape
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                _logic.Cancel();
            }
        }

        /// <summary>
        /// 決定入力時の処理。CommandSelectLogicのConfirm結果に基づいて
        /// フェーズ遷移に必要なリスト設定を行う。
        /// </summary>
        private void ProcessConfirm()
        {
            var phase = _logic.CurrentPhase;
            var cursorIndex = _logic.CursorIndex;

            if (phase == CommandSelectLogic.Phase.Command)
            {
                var commandType = CommandSelectLogic.GetCommandAt(cursorIndex);

                if (!_logic.Confirm()) return;

                // Confirm成功後、フェーズ遷移に必要なデータを設定
                switch (commandType)
                {
                    case CommandType.Attack:
                        var enemies = GetAliveEnemies();
                        if (enemies.Count > 0)
                            _logic.SetTargetList(enemies, CommandType.Attack);
                        else
                            _logic.Cancel();
                        break;

                    case CommandType.Skill:
                        var skills = GetActorSkills();
                        if (skills != null && skills.Count > 0)
                            _logic.SetSkillList(skills);
                        else
                        {
                            Debug.Log("[CommandSelectUI] 使用できるスキルがありません");
                            // コマンドフェーズに留まる
                            _logic.Begin(_logic.Actor, _logic.CanFlee);
                        }
                        break;

                    case CommandType.Item:
                        Debug.Log("[CommandSelectUI] アイテムはまだ使用できません");
                        // コマンドフェーズに戻す
                        _logic.Begin(_logic.Actor, _logic.CanFlee);
                        break;

                    case CommandType.DualArts:
                        Debug.Log("[CommandSelectUI] ★DA選択: ペア選択→技選択フローの基盤（未実装）");
                        var allies = GetAliveAllies();
                        if (allies.Count > 1)
                            _logic.EnterDualArtsPairSelect(allies);
                        else
                        {
                            Debug.Log("[CommandSelectUI] DAに必要なペアが不足しています");
                            _logic.Begin(_logic.Actor, _logic.CanFlee);
                        }
                        break;

                    case CommandType.Swap:
                        var reserves = GetReserves();
                        if (reserves.Count > 0)
                            _logic.SetTargetList(reserves, CommandType.Swap);
                        else
                        {
                            Debug.Log("[CommandSelectUI] 入れ替えできるメンバーがいません");
                            _logic.Begin(_logic.Actor, _logic.CanFlee);
                        }
                        break;

                    // Defend, Flee, Change は Confirm内で直接コマンド発行済み
                }
            }
            else if (phase == CommandSelectLogic.Phase.SkillList)
            {
                if (!_logic.Confirm()) return;

                // スキル確定後、ターゲット選択へ
                var skill = _logic.PendingSkill;
                if (skill != null)
                {
                    switch (skill.TargetType)
                    {
                        case SkillTargetType.SingleEnemy:
                            var enemies = GetAliveEnemies();
                            if (enemies.Count > 0)
                                _logic.SetTargetList(enemies, CommandType.Skill, skill);
                            break;

                        case SkillTargetType.SingleAlly:
                            var allies = GetAliveAllies();
                            if (allies.Count > 0)
                                _logic.SetTargetList(allies, CommandType.Skill, skill);
                            break;

                        case SkillTargetType.Self:
                            SubmitSkillCommandForTarget(_logic.Actor, skill);
                            break;

                        case SkillTargetType.AllEnemies:
                            var firstEnemy = GetFirstAliveEnemy();
                            if (firstEnemy != null)
                                SubmitSkillCommandForTarget(firstEnemy, skill);
                            break;

                        case SkillTargetType.AllAllies:
                            SubmitSkillCommandForTarget(_logic.Actor, skill);
                            break;
                    }
                }
            }
            else if (phase == CommandSelectLogic.Phase.TargetSelect)
            {
                _logic.Confirm();
            }
            else if (phase == CommandSelectLogic.Phase.DualArtsPairSelect)
            {
                Debug.Log("[CommandSelectUI] ★DAペア選択確定（未実装 — ログのみ）");
                _logic.Cancel();
            }
        }

        // ============================================================
        //  Logic Event Handlers
        // ============================================================

        private void HandleCursorMoved()
        {
            PlaySE(_seCursorMove);
            RefreshCursorDisplay();
        }

        private void HandleCommandDecided(BattleCommand command)
        {
            OnCommandDecided?.Invoke(command);
            HideAll();
        }

        private void HandleCancelled()
        {
            PlaySE(_seCancel);
        }

        private void HandleConfirmed()
        {
            PlaySE(_seConfirm);
        }

        private void HandlePhaseChanged(CommandSelectLogic.Phase phase)
        {
            switch (phase)
            {
                case CommandSelectLogic.Phase.Command:
                    RefreshCommandPanel();
                    break;
                case CommandSelectLogic.Phase.SkillList:
                    RefreshSkillPanel();
                    break;
                case CommandSelectLogic.Phase.TargetSelect:
                case CommandSelectLogic.Phase.DualArtsPairSelect:
                    RefreshTargetPanel();
                    break;
                case CommandSelectLogic.Phase.Inactive:
                    HideAll();
                    break;
            }
        }

        // ============================================================
        //  UI Refresh
        // ============================================================

        private void RefreshCommandPanel()
        {
            SetActive(_commandPanel, true);
            SetActive(_skillPanel, false);
            SetActive(_targetPanel, false);

            // コマンドラベルの更新
            for (int i = 0; i < _commandLabels.Length && i < CommandDisplayNames.Length; i++)
            {
                if (_commandLabels[i] == null) continue;
                _commandLabels[i].text = CommandDisplayNames[i];
                _commandLabels[i].color = (i == _logic.CursorIndex) ? _selectedColor : _normalColor;
            }
        }

        private void RefreshSkillPanel()
        {
            SetActive(_commandPanel, false);
            SetActive(_skillPanel, true);
            SetActive(_targetPanel, false);

            if (_skillListContainer == null || _skillEntryPrefab == null) return;

            ClearChildren(_skillListContainer);
            _skillEntryLabels.Clear();
            _skillEntryButtons.Clear();

            var skills = _logic.CurrentSkills;
            if (skills == null) return;

            for (int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                var go = Instantiate(_skillEntryPrefab, _skillListContainer);
                var label = go.GetComponentInChildren<Text>();
                var btn = go.GetComponent<Button>();

                if (label != null)
                {
                    bool usable = _logic.IsSkillUsable(skill);
                    label.text = $"{skill.Name} (MP:{skill.MpCost})";
                    label.color = usable ? _normalColor : _disabledColor;
                }

                _skillEntryLabels.Add(label);
                _skillEntryButtons.Add(btn);

                // ボタンクリックでもスキル選択可能
                if (btn != null)
                {
                    int capturedIndex = i;
                    btn.interactable = _logic.IsSkillUsable(skill);
                    btn.onClick.AddListener(() =>
                    {
                        _logic.SetCursorIndex(capturedIndex);
                        ProcessConfirm();
                    });
                }
            }

            RefreshCursorDisplay();
        }

        private void RefreshTargetPanel()
        {
            SetActive(_commandPanel, false);
            SetActive(_skillPanel, false);
            SetActive(_targetPanel, true);

            if (_targetListContainer == null || _targetEntryPrefab == null) return;

            ClearChildren(_targetListContainer);
            _targetEntryLabels.Clear();
            _targetEntryButtons.Clear();

            var targets = _logic.CurrentTargets;
            if (targets == null) return;

            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target.CurrentHp <= 0) continue;

                var go = Instantiate(_targetEntryPrefab, _targetListContainer);
                var label = go.GetComponentInChildren<Text>();
                var btn = go.GetComponent<Button>();

                if (label != null)
                {
                    label.text = $"{target.Name} HP:{target.CurrentHp}/{target.MaxHp}";
                    label.color = (i == _logic.CursorIndex) ? _selectedColor : _normalColor;
                }

                _targetEntryLabels.Add(label);
                _targetEntryButtons.Add(btn);
            }
        }

        private void RefreshCursorDisplay()
        {
            var phase = _logic.CurrentPhase;
            int cursor = _logic.CursorIndex;

            if (phase == CommandSelectLogic.Phase.Command)
            {
                for (int i = 0; i < _commandLabels.Length; i++)
                {
                    if (_commandLabels[i] != null)
                        _commandLabels[i].color = (i == cursor) ? _selectedColor : _normalColor;
                }
            }
            else if (phase == CommandSelectLogic.Phase.SkillList)
            {
                for (int i = 0; i < _skillEntryLabels.Count; i++)
                {
                    if (_skillEntryLabels[i] != null)
                    {
                        bool usable = _logic.CurrentSkills != null && i < _logic.CurrentSkills.Count
                            && _logic.IsSkillUsable(_logic.CurrentSkills[i]);
                        if (i == cursor)
                            _skillEntryLabels[i].color = usable ? _selectedColor : _disabledColor;
                        else
                            _skillEntryLabels[i].color = usable ? _normalColor : _disabledColor;
                    }
                }
            }
            else if (phase == CommandSelectLogic.Phase.TargetSelect || phase == CommandSelectLogic.Phase.DualArtsPairSelect)
            {
                for (int i = 0; i < _targetEntryLabels.Count; i++)
                {
                    if (_targetEntryLabels[i] != null)
                        _targetEntryLabels[i].color = (i == cursor) ? _selectedColor : _normalColor;
                }
            }

            // カーソルイメージの位置更新
            UpdateCursorImagePosition();
        }

        private void UpdateCursorImagePosition()
        {
            if (_cursorImage == null) return;

            Transform targetTransform = null;
            int cursor = _logic.CursorIndex;

            var phase = _logic.CurrentPhase;
            if (phase == CommandSelectLogic.Phase.Command && cursor < _commandLabels.Length && _commandLabels[cursor] != null)
            {
                targetTransform = _commandLabels[cursor].transform;
            }
            else if (phase == CommandSelectLogic.Phase.SkillList && cursor < _skillEntryLabels.Count && _skillEntryLabels[cursor] != null)
            {
                targetTransform = _skillEntryLabels[cursor].transform;
            }
            else if ((phase == CommandSelectLogic.Phase.TargetSelect || phase == CommandSelectLogic.Phase.DualArtsPairSelect)
                     && cursor < _targetEntryLabels.Count && _targetEntryLabels[cursor] != null)
            {
                targetTransform = _targetEntryLabels[cursor].transform;
            }

            if (targetTransform != null)
            {
                _cursorImage.gameObject.SetActive(true);
                _cursorImage.rectTransform.position = targetTransform.position
                    + Vector3.left * 30f; // カーソルを左側に表示
            }
            else
            {
                _cursorImage.gameObject.SetActive(false);
            }
        }

        // ============================================================
        //  Data Helpers
        // ============================================================

        private List<CharacterStats> GetAliveEnemies()
        {
            var result = new List<CharacterStats>();
            if (_battleManager == null || _battleManager.Enemies == null) return result;
            foreach (var enemy in _battleManager.Enemies)
            {
                if (enemy.CurrentHp > 0) result.Add(enemy);
            }
            return result;
        }

        private List<CharacterStats> GetAliveAllies()
        {
            var result = new List<CharacterStats>();
            if (_battleManager == null || _battleManager.Party == null) return result;
            foreach (var ally in _battleManager.Party)
            {
                if (ally.CurrentHp > 0) result.Add(ally);
            }
            return result;
        }

        private List<CharacterStats> GetReserves()
        {
            var result = new List<CharacterStats>();
            if (_battleManager == null || _battleManager.Reserves == null) return result;
            foreach (var reserve in _battleManager.Reserves)
            {
                if (reserve.CurrentHp > 0) result.Add(reserve);
            }
            return result;
        }

        private CharacterStats GetFirstAliveEnemy()
        {
            if (_battleManager == null || _battleManager.Enemies == null) return null;
            foreach (var enemy in _battleManager.Enemies)
            {
                if (enemy.CurrentHp > 0) return enemy;
            }
            return null;
        }

        private List<SkillData> GetActorSkills()
        {
            if (_battleManager == null || _battleManager.Setup == null || _logic.Actor == null)
                return null;

            if (_battleManager.Setup.PartySkills != null
                && _battleManager.Setup.PartySkills.TryGetValue(_logic.Actor.Name, out var skills))
            {
                return skills;
            }

            return null;
        }

        private void SubmitSkillCommandForTarget(CharacterStats target, SkillData skill)
        {
            if (_logic.Actor == null || target == null || skill == null) return;

            var command = new BattleCommand
            {
                Attacker = _logic.Actor,
                Target = target,
                Type = CommandType.Skill,
                Element = skill.Element,
                SkillPower = skill.Power,
                MpCost = skill.MpCost,
                IsMagic = skill.IsMagic
            };

            OnCommandDecided?.Invoke(command);
            _logic.Deactivate();
        }

        // ============================================================
        //  SE
        // ============================================================

        private void PlaySE(AudioClip clip)
        {
            if (clip == null) return;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(clip);
            }
            else
            {
                // Fallback: AudioManager未設定時はログ出力
                Debug.Log($"[CommandSelectUI] SE: {clip.name}");
            }
        }

        // ============================================================
        //  Utility
        // ============================================================

        private void HideAll()
        {
            SetActive(_commandPanel, false);
            SetActive(_skillPanel, false);
            SetActive(_targetPanel, false);
            if (_cursorImage != null) _cursorImage.gameObject.SetActive(false);
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
