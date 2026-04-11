using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    /// <summary>
    /// コマンド選択UIの状態遷移とカーソル管理を行うPure C#ロジック。
    /// MonoBehaviour依存なしでテスト可能。
    /// </summary>
    public class CommandSelectLogic
    {
        // ============================================================
        //  State
        // ============================================================

        public enum Phase
        {
            /// <summary>コマンド選択画面</summary>
            Command,
            /// <summary>スキルリスト選択画面</summary>
            SkillList,
            /// <summary>ターゲット選択画面</summary>
            TargetSelect,
            /// <summary>DA用ペア選択（基盤のみ）</summary>
            DualArtsPairSelect,
            /// <summary>非アクティブ（コマンド未要求）</summary>
            Inactive
        }

        public Phase CurrentPhase { get; private set; } = Phase.Inactive;
        public int CursorIndex { get; private set; }

        // ============================================================
        //  Command Definitions
        // ============================================================

        private static readonly CommandType[] CommandList =
        {
            CommandType.Attack,
            CommandType.Skill,
            CommandType.Item,
            CommandType.Defend,
            CommandType.Flee,
            CommandType.DualArts,
            CommandType.Change,
            CommandType.Swap
        };

        public static int CommandCount => CommandList.Length;

        public static CommandType GetCommandAt(int index)
        {
            if (index < 0 || index >= CommandList.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return CommandList[index];
        }

        // ============================================================
        //  Events
        // ============================================================

        /// <summary>カーソル移動時に発火</summary>
        public event Action OnCursorMoved;

        /// <summary>コマンド決定時に発火（BattleCommand完成）</summary>
        public event Action<BattleCommand> OnCommandDecided;

        /// <summary>キャンセル時に発火（フェーズ戻り）</summary>
        public event Action OnCancelled;

        /// <summary>決定入力時に発火（SE用）</summary>
        public event Action OnConfirmed;

        /// <summary>フェーズ変更時に発火</summary>
        public event Action<Phase> OnPhaseChanged;

        // ============================================================
        //  Current Selection Context
        // ============================================================

        private CharacterStats _actor;
        private CommandType _pendingCommand;
        private SkillData _pendingSkill;

        // 外部から渡されるリスト
        private IReadOnlyList<SkillData> _currentSkills;
        private IReadOnlyList<CharacterStats> _currentTargets;

        // Flee可能か
        private bool _canFlee = true;
        private bool _lastCanFlee = true;

        public CharacterStats Actor => _actor;
        public CommandType PendingCommand => _pendingCommand;
        public SkillData PendingSkill => _pendingSkill;
        public IReadOnlyList<SkillData> CurrentSkills => _currentSkills;
        public IReadOnlyList<CharacterStats> CurrentTargets => _currentTargets;
        public bool CanFlee => _canFlee;

        // ============================================================
        //  Public API
        // ============================================================

        /// <summary>コマンド選択を開始する</summary>
        public void Begin(CharacterStats actor, bool canFlee = true)
        {
            _actor = actor ?? throw new ArgumentNullException(nameof(actor));
            _lastCanFlee = canFlee;
            _canFlee = canFlee;
            _pendingSkill = null;
            _currentSkills = null;
            _currentTargets = null;
            CursorIndex = 0;
            SetPhase(Phase.Command);
        }

        /// <summary>非アクティブに戻す</summary>
        public void Deactivate()
        {
            _actor = null;
            _pendingSkill = null;
            _currentSkills = null;
            _currentTargets = null;
            SetPhase(Phase.Inactive);
        }

        /// <summary>カーソルを指定位置に設定</summary>
        public void SetCursorIndex(int index)
        {
            if (CurrentPhase == Phase.Inactive) return;
            int count = GetCurrentListCount();
            if (count <= 0 || index < 0 || index >= count) return;
            if (CursorIndex == index) return;
            CursorIndex = index;
            OnCursorMoved?.Invoke();
        }

        /// <summary>カーソルを上方向に移動</summary>
        public void MoveCursorUp()
        {
            if (CurrentPhase == Phase.Inactive) return;
            int count = GetCurrentListCount();
            if (count <= 0) return;
            CursorIndex = (CursorIndex - 1 + count) % count;
            OnCursorMoved?.Invoke();
        }

        /// <summary>カーソルを下方向に移動</summary>
        public void MoveCursorDown()
        {
            if (CurrentPhase == Phase.Inactive) return;
            int count = GetCurrentListCount();
            if (count <= 0) return;
            CursorIndex = (CursorIndex + 1) % count;
            OnCursorMoved?.Invoke();
        }

        /// <summary>
        /// 決定入力を処理。
        /// 戻り値: trueなら入力を受け付けた。falseなら無効な操作。
        /// </summary>
        public bool Confirm()
        {
            if (CurrentPhase == Phase.Inactive || _actor == null) return false;

            switch (CurrentPhase)
            {
                case Phase.Command:
                    return HandleCommandConfirm();
                case Phase.SkillList:
                    return HandleSkillConfirm();
                case Phase.TargetSelect:
                    return HandleTargetConfirm();
                case Phase.DualArtsPairSelect:
                    // DA基盤のみ: ログ出力用にキャンセル扱い
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// キャンセル入力を処理。
        /// 戻り値: trueならキャンセルを受け付けた。
        /// </summary>
        public bool Cancel()
        {
            if (CurrentPhase == Phase.Inactive) return false;

            switch (CurrentPhase)
            {
                case Phase.Command:
                    // コマンドフェーズではキャンセル不可（最上位）
                    return false;
                case Phase.SkillList:
                    CursorIndex = 0;
                    SetPhase(Phase.Command);
                    OnCancelled?.Invoke();
                    return true;
                case Phase.TargetSelect:
                    // スキルからのターゲット選択ならスキルリストに戻る
                    if (_pendingCommand == CommandType.Skill && _pendingSkill != null)
                    {
                        CursorIndex = 0;
                        SetPhase(Phase.SkillList);
                    }
                    else
                    {
                        CursorIndex = 0;
                        SetPhase(Phase.Command);
                    }
                    OnCancelled?.Invoke();
                    return true;
                case Phase.DualArtsPairSelect:
                    CursorIndex = 0;
                    SetPhase(Phase.Command);
                    OnCancelled?.Invoke();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>スキルリスト表示用にスキルリストを設定してフェーズ遷移</summary>
        public void SetSkillList(IReadOnlyList<SkillData> skills)
        {
            _currentSkills = skills;
            CursorIndex = 0;
            SetPhase(Phase.SkillList);
        }

        /// <summary>ターゲット選択用にターゲットリストを設定してフェーズ遷移</summary>
        public void SetTargetList(IReadOnlyList<CharacterStats> targets, CommandType command, SkillData skill = null)
        {
            _currentTargets = targets;
            _pendingCommand = command;
            _pendingSkill = skill;
            CursorIndex = 0;
            SetPhase(Phase.TargetSelect);
        }

        /// <summary>DA用ペア選択フェーズに遷移</summary>
        public void EnterDualArtsPairSelect(IReadOnlyList<CharacterStats> allies)
        {
            _currentTargets = allies;
            _pendingCommand = CommandType.DualArts;
            CursorIndex = 0;
            SetPhase(Phase.DualArtsPairSelect);
        }

        // ============================================================
        //  Skill Usability Check
        // ============================================================

        /// <summary>スキルがMP不足かどうかを判定</summary>
        public bool IsSkillUsable(SkillData skill)
        {
            if (_actor == null || skill == null) return false;
            return _actor.CurrentMp >= skill.MpCost;
        }

        // ============================================================
        //  Internal
        // ============================================================

        private bool HandleCommandConfirm()
        {
            var commandType = CommandList[CursorIndex];
            _pendingCommand = commandType;

            switch (commandType)
            {
                case CommandType.Attack:
                    // ターゲット選択へ（外部でターゲットリストを設定する）
                    OnConfirmed?.Invoke();
                    return true;

                case CommandType.Skill:
                    // スキルリスト表示へ（外部でスキルリストを設定する）
                    OnConfirmed?.Invoke();
                    return true;

                case CommandType.Item:
                    // アイテム未実装
                    OnConfirmed?.Invoke();
                    return true;

                case CommandType.Defend:
                    EmitCommand(new BattleCommand
                    {
                        Attacker = _actor,
                        Target = _actor,
                        Type = CommandType.Defend
                    });
                    return true;

                case CommandType.Flee:
                    if (!_canFlee) return false;
                    EmitCommand(new BattleCommand
                    {
                        Attacker = _actor,
                        Target = _actor,
                        Type = CommandType.Flee
                    });
                    return true;

                case CommandType.DualArts:
                    // DA基盤のみ（外部でペア選択リストを設定する）
                    OnConfirmed?.Invoke();
                    return true;

                case CommandType.Change:
                    EmitCommand(new BattleCommand
                    {
                        Attacker = _actor,
                        Target = _actor,
                        Type = CommandType.Change
                    });
                    return true;

                case CommandType.Swap:
                    // ターゲット選択へ（外部で控えメンバーリストを設定する）
                    OnConfirmed?.Invoke();
                    return true;

                default:
                    return false;
            }
        }

        private bool HandleSkillConfirm()
        {
            if (_currentSkills == null || CursorIndex >= _currentSkills.Count) return false;
            var skill = _currentSkills[CursorIndex];
            if (!IsSkillUsable(skill)) return false;

            _pendingSkill = skill;
            OnConfirmed?.Invoke();
            return true;
        }

        private bool HandleTargetConfirm()
        {
            if (_currentTargets == null || CursorIndex >= _currentTargets.Count) return false;
            var target = _currentTargets[CursorIndex];
            if (target.CurrentHp <= 0) return false;

            var command = new BattleCommand
            {
                Attacker = _actor,
                Target = target,
                Type = _pendingCommand
            };

            // スキルの場合はスキル情報を付与
            if (_pendingCommand == CommandType.Skill && _pendingSkill != null)
            {
                command.Element = _pendingSkill.Element;
                command.SkillPower = _pendingSkill.Power;
                command.MpCost = _pendingSkill.MpCost;
                command.IsMagic = _pendingSkill.IsMagic;
            }
            else if (_pendingCommand == CommandType.Attack)
            {
                command.Element = _actor.Element;
            }

            EmitCommand(command);
            return true;
        }

        private void EmitCommand(BattleCommand command)
        {
            OnConfirmed?.Invoke();
            OnCommandDecided?.Invoke(command);
            Deactivate();
        }

        private void SetPhase(Phase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        private int GetCurrentListCount()
        {
            switch (CurrentPhase)
            {
                case Phase.Command:
                    return CommandList.Length;
                case Phase.SkillList:
                    return _currentSkills?.Count ?? 0;
                case Phase.TargetSelect:
                case Phase.DualArtsPairSelect:
                    return _currentTargets?.Count ?? 0;
                default:
                    return 0;
            }
        }
    }
}
