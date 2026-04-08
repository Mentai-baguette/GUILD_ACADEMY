using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using TMPro;
using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Battle
{
    [DisallowMultipleComponent]
    public class BattleUIManager : MonoBehaviour
    {
        [Header("Combatant Views")]
        [SerializeField] private BattleCombatantView[] _partyViews;
        [SerializeField] private BattleCombatantView[] _enemyViews;

        [Header("Command UI")]
        [SerializeField] private BattleCommandMenuView _commandMenuView;
        [SerializeField] private TextMeshProUGUI _battleMessageText;

        private BattleFlowController _battleFlowController;
        private ATBSystem _atbSystem;
        private BreakSystem _breakSystem;
        private readonly List<CharacterStats> _party = new List<CharacterStats>();
        private readonly List<CharacterStats> _enemies = new List<CharacterStats>();
        private CharacterStats _currentActor;

        public void Bind(BattleFlowController battleFlowController, IReadOnlyList<CharacterStats> party, IReadOnlyList<CharacterStats> enemies, ATBSystem atbSystem, BreakSystem breakSystem)
        {
            Unbind();

            _battleFlowController = battleFlowController;
            _atbSystem = atbSystem;
            _breakSystem = breakSystem;

            _party.Clear();
            _party.AddRange(party.Where(member => member != null));

            _enemies.Clear();
            _enemies.AddRange(enemies.Where(enemy => enemy != null));

            if (_battleFlowController != null)
            {
                _battleFlowController.OnBattleStart += HandleBattleStart;
                _battleFlowController.OnCommandRequired += HandleCommandRequired;
                _battleFlowController.OnActionExecuted += HandleActionExecuted;
                _battleFlowController.OnBattleEnd += HandleBattleEnd;
            }

            if (_commandMenuView != null)
            {
                _commandMenuView.OnAttackRequested += HandleAttackRequested;
                _commandMenuView.OnSkillRequested += HandleSkillRequested;
                _commandMenuView.OnItemRequested += HandleItemRequested;
                _commandMenuView.OnDefendRequested += HandleDefendRequested;
                _commandMenuView.SetVisible(false);
            }

            SetupCombatantViews();
            RefreshViews();
        }

        public void RefreshViews()
        {
            RefreshCombatantViews(_partyViews, _party);
            RefreshCombatantViews(_enemyViews, _enemies);
        }

        private void Unbind()
        {
            if (_battleFlowController != null)
            {
                _battleFlowController.OnBattleStart -= HandleBattleStart;
                _battleFlowController.OnCommandRequired -= HandleCommandRequired;
                _battleFlowController.OnActionExecuted -= HandleActionExecuted;
                _battleFlowController.OnBattleEnd -= HandleBattleEnd;
            }

            if (_commandMenuView != null)
            {
                _commandMenuView.OnAttackRequested -= HandleAttackRequested;
                _commandMenuView.OnSkillRequested -= HandleSkillRequested;
                _commandMenuView.OnItemRequested -= HandleItemRequested;
                _commandMenuView.OnDefendRequested -= HandleDefendRequested;
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void SetupCombatantViews()
        {
            BindCombatantViews(_partyViews, _party, true);
            BindCombatantViews(_enemyViews, _enemies, false);
        }

        private void BindCombatantViews(BattleCombatantView[] views, IReadOnlyList<CharacterStats> combatants, bool isParty)
        {
            if (views == null)
                return;

            for (int i = 0; i < views.Length; i++)
            {
                var view = views[i];
                if (view == null)
                    continue;

                var combatant = i < combatants.Count ? combatants[i] : null;
                view.Bind(combatant, _atbSystem, _breakSystem, isParty);
            }
        }

        private void RefreshCombatantViews(BattleCombatantView[] views, IReadOnlyList<CharacterStats> combatants)
        {
            if (views == null)
                return;

            for (int i = 0; i < views.Length; i++)
            {
                var view = views[i];
                if (view == null)
                    continue;

                var combatant = i < combatants.Count ? combatants[i] : null;
                view.Bind(combatant, _atbSystem, _breakSystem, view.IsPartySlot);
                view.Refresh();
            }
        }

        private void HandleBattleStart()
        {
            SetMessage("Battle Start");
            if (_commandMenuView != null)
            {
                _commandMenuView.SetVisible(false);
            }
        }

        private void HandleCommandRequired(CharacterStats actor)
        {
            _currentActor = actor;
            RefreshViews();

            if (actor == null)
                return;

            if (IsPlayerControlled(actor))
            {
                SetMessage(actor.Name + " の行動を選択");
                if (_commandMenuView != null)
                {
                    _commandMenuView.SetActor(actor.Name);
                    _commandMenuView.SetVisible(true);
                }

                return;
            }

            if (_commandMenuView != null)
            {
                _commandMenuView.SetVisible(false);
            }

            SubmitAutoEnemyCommand(actor);
        }

        private void HandleActionExecuted(ActionResult result)
        {
            if (result == null)
                return;

            var outcome = result.TargetDefeated ? " Defeat!" : string.Empty;
            SetMessage(result.Attacker.Name + " -> " + result.Target.Name + " / " + result.DamageDealt + outcome);
            RefreshViews();

            if (_commandMenuView != null)
            {
                _commandMenuView.SetVisible(false);
            }
        }

        private void HandleBattleEnd(BattleResult result)
        {
            SetMessage("Battle End: " + result);
            if (_commandMenuView != null)
            {
                _commandMenuView.SetVisible(false);
            }
        }

        private void HandleAttackRequested()
        {
            SubmitPlayerCommand(CommandType.Attack, 100, null);
        }

        private void HandleSkillRequested()
        {
            SubmitPlayerCommand(CommandType.Skill, 140, null);
        }

        private void HandleItemRequested()
        {
            SubmitPlayerCommand(CommandType.Item, 0, _currentActor);
        }

        private void HandleDefendRequested()
        {
            SubmitPlayerCommand(CommandType.Defend, 0, _currentActor);
        }

        private void SubmitPlayerCommand(CommandType commandType, int skillPower, CharacterStats explicitTarget)
        {
            if (_battleFlowController == null || _currentActor == null)
                return;

            var target = explicitTarget ?? GetFirstLivingEnemy();
            if (target == null && commandType != CommandType.Defend)
                return;

            var command = new BattleCommand
            {
                Attacker = _currentActor,
                Target = commandType == CommandType.Defend ? _currentActor : target,
                Type = commandType,
                Element = _currentActor.Element,
                SkillPower = skillPower,
                MpCost = 0
            };

            _battleFlowController.SubmitCommand(command);
        }

        private void SubmitAutoEnemyCommand(CharacterStats actor)
        {
            if (_battleFlowController == null || actor == null)
                return;

            var target = GetFirstLivingParty();
            if (target == null)
                return;

            var command = new BattleCommand
            {
                Attacker = actor,
                Target = target,
                Type = CommandType.Attack,
                Element = actor.Element,
                SkillPower = 100,
                MpCost = 0
            };

            _battleFlowController.SubmitCommand(command);
        }

        private CharacterStats GetFirstLivingEnemy()
        {
            return _enemies.FirstOrDefault(enemy => enemy != null && enemy.CurrentHp > 0);
        }

        private CharacterStats GetFirstLivingParty()
        {
            return _party.FirstOrDefault(member => member != null && member.CurrentHp > 0);
        }

        private bool IsPlayerControlled(CharacterStats actor)
        {
            return actor != null && _party.Contains(actor);
        }

        private void SetMessage(string message)
        {
            if (_battleMessageText != null)
            {
                _battleMessageText.text = message;
            }
        }
    }
}