using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class CommandSelectLogicTests
    {
        private CommandSelectLogic _logic;
        private CharacterStats _actor;
        private CharacterStats _enemy;
        private CharacterStats _ally;
        private List<BattleCommand> _decidedCommands;
        private int _cursorMovedCount;
        private int _cancelledCount;
        private int _confirmedCount;
        private List<CommandSelectLogic.Phase> _phaseChanges;

        [SetUp]
        public void SetUp()
        {
            _logic = new CommandSelectLogic();
            _actor = new CharacterStats("テスト戦士", 100, 50, 30, 20, 15, ElementType.Fire);
            _enemy = new CharacterStats("スライム", 50, 0, 10, 5, 8);
            _ally = new CharacterStats("テスト魔法使い", 80, 100, 15, 10, 12, ElementType.Ice);

            _decidedCommands = new List<BattleCommand>();
            _cursorMovedCount = 0;
            _cancelledCount = 0;
            _confirmedCount = 0;
            _phaseChanges = new List<CommandSelectLogic.Phase>();

            _logic.OnCommandDecided += cmd => _decidedCommands.Add(cmd);
            _logic.OnCursorMoved += () => _cursorMovedCount++;
            _logic.OnCancelled += () => _cancelledCount++;
            _logic.OnConfirmed += () => _confirmedCount++;
            _logic.OnPhaseChanged += phase => _phaseChanges.Add(phase);
        }

        // ============================================================
        //  Initial State
        // ============================================================

        [Test]
        public void InitialState_IsInactive()
        {
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Inactive));
            Assert.That(_logic.CursorIndex, Is.EqualTo(0));
            Assert.That(_logic.Actor, Is.Null);
        }

        // ============================================================
        //  Begin / Deactivate
        // ============================================================

        [Test]
        public void Begin_SetsCommandPhase()
        {
            _logic.Begin(_actor);

            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Command));
            Assert.That(_logic.Actor, Is.EqualTo(_actor));
            Assert.That(_logic.CursorIndex, Is.EqualTo(0));
        }

        [Test]
        public void Begin_NullActor_ThrowsException()
        {
            Assert.Throws<System.ArgumentNullException>(() => _logic.Begin(null));
        }

        [Test]
        public void Deactivate_SetsInactivePhase()
        {
            _logic.Begin(_actor);
            _logic.Deactivate();

            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Inactive));
            Assert.That(_logic.Actor, Is.Null);
        }

        // ============================================================
        //  Cursor Movement
        // ============================================================

        [Test]
        public void MoveCursorDown_IncrementsIndex()
        {
            _logic.Begin(_actor);
            _logic.MoveCursorDown();

            Assert.That(_logic.CursorIndex, Is.EqualTo(1));
            Assert.That(_cursorMovedCount, Is.EqualTo(1));
        }

        [Test]
        public void MoveCursorUp_DecrementsWithWrap()
        {
            _logic.Begin(_actor);
            _logic.MoveCursorUp();

            // Should wrap to last command (8 commands, index 7)
            Assert.That(_logic.CursorIndex, Is.EqualTo(CommandSelectLogic.CommandCount - 1));
            Assert.That(_cursorMovedCount, Is.EqualTo(1));
        }

        [Test]
        public void MoveCursorDown_WrapsAround()
        {
            _logic.Begin(_actor);

            // Move down past the last item
            for (int i = 0; i < CommandSelectLogic.CommandCount; i++)
                _logic.MoveCursorDown();

            Assert.That(_logic.CursorIndex, Is.EqualTo(0));
        }

        [Test]
        public void MoveCursor_WhenInactive_DoesNothing()
        {
            _logic.MoveCursorDown();
            _logic.MoveCursorUp();

            Assert.That(_cursorMovedCount, Is.EqualTo(0));
            Assert.That(_logic.CursorIndex, Is.EqualTo(0));
        }

        // ============================================================
        //  Command Count
        // ============================================================

        [Test]
        public void CommandCount_Is8()
        {
            Assert.That(CommandSelectLogic.CommandCount, Is.EqualTo(8));
        }

        [Test]
        public void GetCommandAt_ReturnsCorrectTypes()
        {
            Assert.That(CommandSelectLogic.GetCommandAt(0), Is.EqualTo(CommandType.Attack));
            Assert.That(CommandSelectLogic.GetCommandAt(1), Is.EqualTo(CommandType.Skill));
            Assert.That(CommandSelectLogic.GetCommandAt(2), Is.EqualTo(CommandType.Item));
            Assert.That(CommandSelectLogic.GetCommandAt(3), Is.EqualTo(CommandType.Defend));
            Assert.That(CommandSelectLogic.GetCommandAt(4), Is.EqualTo(CommandType.Flee));
            Assert.That(CommandSelectLogic.GetCommandAt(5), Is.EqualTo(CommandType.DualArts));
            Assert.That(CommandSelectLogic.GetCommandAt(6), Is.EqualTo(CommandType.Change));
            Assert.That(CommandSelectLogic.GetCommandAt(7), Is.EqualTo(CommandType.Swap));
        }

        [Test]
        public void GetCommandAt_InvalidIndex_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => CommandSelectLogic.GetCommandAt(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => CommandSelectLogic.GetCommandAt(8));
        }

        // ============================================================
        //  Defend Command (Direct Execute)
        // ============================================================

        [Test]
        public void Confirm_Defend_EmitsCommand()
        {
            _logic.Begin(_actor);

            // Move cursor to Defend (index 3)
            _logic.MoveCursorDown(); // 1
            _logic.MoveCursorDown(); // 2
            _logic.MoveCursorDown(); // 3

            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            Assert.That(_decidedCommands[0].Type, Is.EqualTo(CommandType.Defend));
            Assert.That(_decidedCommands[0].Attacker, Is.EqualTo(_actor));
            Assert.That(_decidedCommands[0].Target, Is.EqualTo(_actor));
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Inactive));
        }

        // ============================================================
        //  Flee Command
        // ============================================================

        [Test]
        public void Confirm_Flee_EmitsCommand()
        {
            _logic.Begin(_actor, canFlee: true);

            // Move cursor to Flee (index 4)
            for (int i = 0; i < 4; i++) _logic.MoveCursorDown();

            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            Assert.That(_decidedCommands[0].Type, Is.EqualTo(CommandType.Flee));
        }

        [Test]
        public void Confirm_FleeNotAllowed_ReturnsFalse()
        {
            _logic.Begin(_actor, canFlee: false);

            // Move cursor to Flee (index 4)
            for (int i = 0; i < 4; i++) _logic.MoveCursorDown();

            bool result = _logic.Confirm();

            Assert.That(result, Is.False);
            Assert.That(_decidedCommands.Count, Is.EqualTo(0));
        }

        // ============================================================
        //  Change Command
        // ============================================================

        [Test]
        public void Confirm_Change_EmitsCommand()
        {
            _logic.Begin(_actor);

            // Move cursor to Change (index 6)
            for (int i = 0; i < 6; i++) _logic.MoveCursorDown();

            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            Assert.That(_decidedCommands[0].Type, Is.EqualTo(CommandType.Change));
        }

        // ============================================================
        //  Attack Command (requires target selection)
        // ============================================================

        [Test]
        public void Confirm_Attack_FiresConfirmedEvent()
        {
            _logic.Begin(_actor);
            // Cursor is at Attack (index 0)

            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_confirmedCount, Is.GreaterThan(0));
            // Attack doesn't emit command directly; external code sets target list
        }

        // ============================================================
        //  Skill Selection Flow
        // ============================================================

        [Test]
        public void SetSkillList_ChangesToSkillPhase()
        {
            _logic.Begin(_actor);
            var skills = new List<SkillData>
            {
                new SkillData { Name = "ファイア", MpCost = 10, Power = 50, Element = ElementType.Fire, IsMagic = true, TargetType = SkillTargetType.SingleEnemy },
                new SkillData { Name = "ブリザド", MpCost = 15, Power = 60, Element = ElementType.Ice, IsMagic = true, TargetType = SkillTargetType.SingleEnemy }
            };

            _logic.SetSkillList(skills);

            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.SkillList));
            Assert.That(_logic.CursorIndex, Is.EqualTo(0));
            Assert.That(_logic.CurrentSkills, Is.EqualTo(skills));
        }

        [Test]
        public void SkillList_CursorMovement_Works()
        {
            _logic.Begin(_actor);
            var skills = new List<SkillData>
            {
                new SkillData { Name = "ファイア", MpCost = 10, Power = 50 },
                new SkillData { Name = "ブリザド", MpCost = 15, Power = 60 }
            };
            _logic.SetSkillList(skills);

            _logic.MoveCursorDown();
            Assert.That(_logic.CursorIndex, Is.EqualTo(1));

            _logic.MoveCursorDown();
            Assert.That(_logic.CursorIndex, Is.EqualTo(0)); // Wrap
        }

        [Test]
        public void IsSkillUsable_EnoughMp_ReturnsTrue()
        {
            _logic.Begin(_actor); // MP = 50
            var skill = new SkillData { Name = "ファイア", MpCost = 10 };

            Assert.That(_logic.IsSkillUsable(skill), Is.True);
        }

        [Test]
        public void IsSkillUsable_NotEnoughMp_ReturnsFalse()
        {
            _logic.Begin(_actor); // MP = 50
            var skill = new SkillData { Name = "メテオ", MpCost = 100 };

            Assert.That(_logic.IsSkillUsable(skill), Is.False);
        }

        [Test]
        public void SkillConfirm_UnusableSkill_ReturnsFalse()
        {
            _logic.Begin(_actor); // MP = 50
            var skills = new List<SkillData>
            {
                new SkillData { Name = "メテオ", MpCost = 100, Power = 200 }
            };
            _logic.SetSkillList(skills);

            bool result = _logic.Confirm();

            Assert.That(result, Is.False);
            Assert.That(_decidedCommands.Count, Is.EqualTo(0));
        }

        [Test]
        public void SkillConfirm_UsableSkill_FiresConfirmed()
        {
            _logic.Begin(_actor);
            var skills = new List<SkillData>
            {
                new SkillData { Name = "ファイア", MpCost = 10, Power = 50, Element = ElementType.Fire }
            };
            _logic.SetSkillList(skills);

            _confirmedCount = 0;
            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_confirmedCount, Is.EqualTo(1));
            Assert.That(_logic.PendingSkill, Is.Not.Null);
            Assert.That(_logic.PendingSkill.Name, Is.EqualTo("ファイア"));
        }

        // ============================================================
        //  Target Selection Flow
        // ============================================================

        [Test]
        public void SetTargetList_ChangesToTargetPhase()
        {
            _logic.Begin(_actor);
            var targets = new List<CharacterStats> { _enemy };

            _logic.SetTargetList(targets, CommandType.Attack);

            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.TargetSelect));
            Assert.That(_logic.CurrentTargets, Is.EqualTo(targets));
        }

        [Test]
        public void TargetConfirm_EmitsAttackCommand()
        {
            _logic.Begin(_actor);
            var targets = new List<CharacterStats> { _enemy };
            _logic.SetTargetList(targets, CommandType.Attack);

            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            Assert.That(_decidedCommands[0].Type, Is.EqualTo(CommandType.Attack));
            Assert.That(_decidedCommands[0].Target, Is.EqualTo(_enemy));
            Assert.That(_decidedCommands[0].Attacker, Is.EqualTo(_actor));
            Assert.That(_decidedCommands[0].Element, Is.EqualTo(ElementType.Fire)); // Actor's element
        }

        [Test]
        public void TargetConfirm_SkillCommand_HasSkillProperties()
        {
            _logic.Begin(_actor);
            var skill = new SkillData
            {
                Name = "ファイア",
                MpCost = 10,
                Power = 50,
                Element = ElementType.Fire,
                IsMagic = true,
                TargetType = SkillTargetType.SingleEnemy
            };
            var targets = new List<CharacterStats> { _enemy };
            _logic.SetTargetList(targets, CommandType.Skill, skill);

            bool result = _logic.Confirm();

            Assert.That(result, Is.True);
            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            var cmd = _decidedCommands[0];
            Assert.That(cmd.Type, Is.EqualTo(CommandType.Skill));
            Assert.That(cmd.Element, Is.EqualTo(ElementType.Fire));
            Assert.That(cmd.SkillPower, Is.EqualTo(50));
            Assert.That(cmd.MpCost, Is.EqualTo(10));
            Assert.That(cmd.IsMagic, Is.True);
        }

        [Test]
        public void TargetConfirm_DeadTarget_ReturnsFalse()
        {
            _logic.Begin(_actor);
            var deadEnemy = new CharacterStats("死亡敵", 50, 0, 10, 5, 8);
            deadEnemy.CurrentHp = 0;
            var targets = new List<CharacterStats> { deadEnemy };
            _logic.SetTargetList(targets, CommandType.Attack);

            bool result = _logic.Confirm();

            Assert.That(result, Is.False);
            Assert.That(_decidedCommands.Count, Is.EqualTo(0));
        }

        // ============================================================
        //  Cancel
        // ============================================================

        [Test]
        public void Cancel_CommandPhase_ReturnsFalse()
        {
            _logic.Begin(_actor);
            bool result = _logic.Cancel();

            Assert.That(result, Is.False);
        }

        [Test]
        public void Cancel_SkillPhase_ReturnsToCommand()
        {
            _logic.Begin(_actor);
            _logic.SetSkillList(new List<SkillData>
            {
                new SkillData { Name = "ファイア", MpCost = 10 }
            });

            bool result = _logic.Cancel();

            Assert.That(result, Is.True);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Command));
            Assert.That(_cancelledCount, Is.EqualTo(1));
        }

        [Test]
        public void Cancel_TargetPhase_FromAttack_ReturnsToCommand()
        {
            _logic.Begin(_actor);
            _logic.SetTargetList(new List<CharacterStats> { _enemy }, CommandType.Attack);

            bool result = _logic.Cancel();

            Assert.That(result, Is.True);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Command));
        }

        [Test]
        public void Cancel_TargetPhase_FromSkill_ReturnsToSkillList()
        {
            _logic.Begin(_actor);
            var skill = new SkillData { Name = "ファイア", MpCost = 10, TargetType = SkillTargetType.SingleEnemy };
            _logic.SetTargetList(new List<CharacterStats> { _enemy }, CommandType.Skill, skill);

            bool result = _logic.Cancel();

            Assert.That(result, Is.True);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.SkillList));
        }

        [Test]
        public void Cancel_Inactive_ReturnsFalse()
        {
            bool result = _logic.Cancel();
            Assert.That(result, Is.False);
        }

        // ============================================================
        //  Dual Arts Pair Select
        // ============================================================

        [Test]
        public void EnterDualArtsPairSelect_ChangesPhaseToDAPairSelect()
        {
            _logic.Begin(_actor);
            var allies = new List<CharacterStats> { _actor, _ally };

            _logic.EnterDualArtsPairSelect(allies);

            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.DualArtsPairSelect));
            Assert.That(_logic.CurrentTargets, Is.EqualTo(allies));
        }

        [Test]
        public void DualArtsPairSelect_Confirm_ReturnsFalse()
        {
            _logic.Begin(_actor);
            _logic.EnterDualArtsPairSelect(new List<CharacterStats> { _actor, _ally });

            // DA基盤のみなのでConfirmはfalse
            bool result = _logic.Confirm();
            Assert.That(result, Is.False);
        }

        [Test]
        public void DualArtsPairSelect_Cancel_ReturnsToCommand()
        {
            _logic.Begin(_actor);
            _logic.EnterDualArtsPairSelect(new List<CharacterStats> { _actor, _ally });

            bool result = _logic.Cancel();

            Assert.That(result, Is.True);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Command));
        }

        // ============================================================
        //  Phase Change Events
        // ============================================================

        [Test]
        public void PhaseChanges_AreTrackedCorrectly()
        {
            _logic.Begin(_actor);
            _logic.SetSkillList(new List<SkillData> { new SkillData { Name = "テスト", MpCost = 5 } });
            _logic.Cancel();
            _logic.Deactivate();

            Assert.That(_phaseChanges.Count, Is.EqualTo(4));
            Assert.That(_phaseChanges[0], Is.EqualTo(CommandSelectLogic.Phase.Command));
            Assert.That(_phaseChanges[1], Is.EqualTo(CommandSelectLogic.Phase.SkillList));
            Assert.That(_phaseChanges[2], Is.EqualTo(CommandSelectLogic.Phase.Command));
            Assert.That(_phaseChanges[3], Is.EqualTo(CommandSelectLogic.Phase.Inactive));
        }

        // ============================================================
        //  Full Flow: Attack
        // ============================================================

        [Test]
        public void FullFlow_Attack_SelectTarget_EmitsCommand()
        {
            // Begin
            _logic.Begin(_actor);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Command));

            // Confirm Attack (index 0)
            _logic.Confirm();

            // External sets target list
            _logic.SetTargetList(new List<CharacterStats> { _enemy }, CommandType.Attack);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.TargetSelect));

            // Confirm target
            _logic.Confirm();

            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            Assert.That(_decidedCommands[0].Type, Is.EqualTo(CommandType.Attack));
            Assert.That(_decidedCommands[0].Target, Is.EqualTo(_enemy));
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.Inactive));
        }

        // ============================================================
        //  Full Flow: Skill → Target → Confirm
        // ============================================================

        [Test]
        public void FullFlow_Skill_SelectSkill_SelectTarget_EmitsCommand()
        {
            var skill = new SkillData
            {
                Name = "ファイア",
                MpCost = 10,
                Power = 50,
                Element = ElementType.Fire,
                IsMagic = true,
                TargetType = SkillTargetType.SingleEnemy
            };

            _logic.Begin(_actor);

            // Move to Skill (index 1) and confirm
            _logic.MoveCursorDown();
            _logic.Confirm();

            // External sets skill list
            _logic.SetSkillList(new List<SkillData> { skill });
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.SkillList));

            // Confirm skill
            _logic.Confirm();

            // External sets target list
            _logic.SetTargetList(new List<CharacterStats> { _enemy }, CommandType.Skill, skill);
            Assert.That(_logic.CurrentPhase, Is.EqualTo(CommandSelectLogic.Phase.TargetSelect));

            // Confirm target
            _logic.Confirm();

            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            var cmd = _decidedCommands[0];
            Assert.That(cmd.Type, Is.EqualTo(CommandType.Skill));
            Assert.That(cmd.Target, Is.EqualTo(_enemy));
            Assert.That(cmd.SkillPower, Is.EqualTo(50));
            Assert.That(cmd.IsMagic, Is.True);
        }

        // ============================================================
        //  Swap Flow
        // ============================================================

        [Test]
        public void FullFlow_Swap_SelectTarget_EmitsCommand()
        {
            var reserve = new CharacterStats("控え戦士", 90, 40, 25, 18, 13);

            _logic.Begin(_actor);

            // Move to Swap (index 7)
            for (int i = 0; i < 7; i++) _logic.MoveCursorDown();
            _logic.Confirm();

            // External sets reserve list
            _logic.SetTargetList(new List<CharacterStats> { reserve }, CommandType.Swap);

            // Confirm target
            _logic.Confirm();

            Assert.That(_decidedCommands.Count, Is.EqualTo(1));
            Assert.That(_decidedCommands[0].Type, Is.EqualTo(CommandType.Swap));
            Assert.That(_decidedCommands[0].Target, Is.EqualTo(reserve));
        }
    }
}
