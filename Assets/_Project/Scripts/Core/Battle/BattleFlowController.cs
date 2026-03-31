using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public enum BattleFlowState
    {
        NotStarted,
        TickingATB,
        WaitingForCommand,
        ExecutingAction,
        BattleOver
    }

    public class BattleFlowController
    {
        private readonly ATBSystem _atb;
        private readonly ActionExecutor _executor;
        private readonly BreakSystem _breakSystem;

        private List<CharacterStats> _party = new List<CharacterStats>();
        private List<CharacterStats> _enemies = new List<CharacterStats>();

        public BattleFlowState State { get; private set; } = BattleFlowState.NotStarted;

        public event Action OnBattleStart;
        public event Action<CharacterStats> OnCommandRequired;
        public event Action<ActionResult> OnActionExecuted;
        public event Action<BattleResult> OnBattleEnd;

        public BattleFlowController(ATBSystem atb, ActionExecutor executor, BreakSystem breakSystem)
        {
            _atb = atb ?? throw new ArgumentNullException(nameof(atb));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _breakSystem = breakSystem ?? throw new ArgumentNullException(nameof(breakSystem));
        }

        public void StartBattle(List<CharacterStats> party, List<CharacterStats> enemies)
        {
            if (party == null) throw new ArgumentNullException(nameof(party));
            if (enemies == null) throw new ArgumentNullException(nameof(enemies));

            _party = new List<CharacterStats>(party);
            _enemies = new List<CharacterStats>(enemies);

            foreach (var member in _party)
            {
                _atb.AddCombatant(member);
                _breakSystem.Register(member);
            }

            foreach (var enemy in _enemies)
            {
                _atb.AddCombatant(enemy);
                _breakSystem.Register(enemy);
            }

            State = BattleFlowState.TickingATB;
            OnBattleStart?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (State != BattleFlowState.TickingATB)
                return;

            _atb.Tick(deltaTime);

            var actor = GetCurrentActor();
            if (actor != null)
            {
                State = BattleFlowState.WaitingForCommand;
                OnCommandRequired?.Invoke(actor);
            }
        }

        public CharacterStats GetCurrentActor()
        {
            return _atb.GetReadyCharacters().FirstOrDefault();
        }

        public ActionResult SubmitCommand(BattleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            State = BattleFlowState.ExecutingAction;

            var result = _executor.Execute(command);
            _atb.ResetGauge(command.Attacker);
            _breakSystem.TickBreakRecovery(command.Target);

            OnActionExecuted?.Invoke(result);

            var battleResult = CheckBattleEnd();
            if (battleResult != BattleResult.None)
            {
                State = BattleFlowState.BattleOver;
                OnBattleEnd?.Invoke(battleResult);
            }
            else
            {
                State = BattleFlowState.TickingATB;
            }

            return result;
        }

        public BattleResult CheckBattleEnd()
        {
            bool allEnemiesDead = _enemies.All(e => e.CurrentHp <= 0);
            bool allPartyDead = _party.All(p => p.CurrentHp <= 0);

            if (allEnemiesDead && allPartyDead)
                return BattleResult.AllDefeated;
            if (allEnemiesDead)
                return BattleResult.PlayerVictory;
            if (allPartyDead)
                return BattleResult.EnemyVictory;

            return BattleResult.None;
        }
    }
}
