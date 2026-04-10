using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Core.Party;

namespace GuildAcademy.Tests.EditMode.Party
{
    [TestFixture]
    public class PartyManagerTests
    {
        private PartyManager _party;
        private CharacterStats _ray;
        private CharacterStats _luna;
        private CharacterStats _kaito;
        private CharacterStats _mira;
        private CharacterStats _sion;

        [SetUp]
        public void SetUp()
        {
            _party = new PartyManager();
            _ray = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _luna = new CharacterStats("Luna", 90, 60, 25, 15, 12);
            _kaito = new CharacterStats("Kaito", 120, 40, 35, 25, 8);
            _mira = new CharacterStats("Mira", 80, 70, 20, 10, 14);
            _sion = new CharacterStats("Sion", 110, 55, 28, 22, 11);
        }

        // ===========================================
        // 既存機能: AddMember / RemoveMember / Clear
        // ===========================================

        [Test]
        public void AddMember_IncreasesCount()
        {
            _party.AddMember(_ray);
            Assert.AreEqual(1, _party.Count);
        }

        [Test]
        public void AddMember_DuplicateName_Ignored()
        {
            _party.AddMember(_ray);
            var ray2 = new CharacterStats("Ray", 200, 100, 50, 40, 20);
            _party.AddMember(ray2);
            Assert.AreEqual(1, _party.Count);
        }

        [Test]
        public void RemoveMember_ExistingName_ReturnsTrue()
        {
            _party.AddMember(_ray);
            Assert.IsTrue(_party.RemoveMember("Ray"));
            Assert.AreEqual(0, _party.Count);
        }

        [Test]
        public void RemoveMember_NonExistingName_ReturnsFalse()
        {
            Assert.IsFalse(_party.RemoveMember("Nobody"));
        }

        // ===========================================
        // レイ固定ロジック
        // ===========================================

        [Test]
        public void SetLeader_RemoveMember_LeaderCannotBeRemoved()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.SetLeader(_ray);

            bool removed = _party.RemoveMember("Ray");

            Assert.IsFalse(removed);
            Assert.AreEqual(2, _party.Count);
        }

        [Test]
        public void SetLeader_NonLeaderCanStillBeRemoved()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.SetLeader(_ray);

            bool removed = _party.RemoveMember("Luna");

            Assert.IsTrue(removed);
            Assert.AreEqual(1, _party.Count);
        }

        [Test]
        public void GetBattleParty_LeaderAlwaysFirst()
        {
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.AddMember(_ray);
            _party.SetLeader(_ray);

            var battleParty = _party.GetBattleParty();

            Assert.AreEqual("Ray", battleParty[0].Name);
        }

        // ===========================================
        // バトルメンバーと控えの分離
        // ===========================================

        [Test]
        public void SetBattleFormation_SplitsBattleAndReserve()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.AddMember(_mira);
            _party.AddMember(_sion);

            _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna, _kaito });

            var battle = _party.GetBattleMembers();
            var reserve = _party.GetReserveMembers();

            Assert.AreEqual(3, battle.Count);
            Assert.AreEqual(2, reserve.Count);
            Assert.IsTrue(reserve.Contains(_mira));
            Assert.IsTrue(reserve.Contains(_sion));
        }

        [Test]
        public void SetBattleFormation_ExceedMax_ThrowsException()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.AddMember(_mira);

            Assert.Throws<System.ArgumentException>(() =>
                _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna, _kaito, _mira }));
        }

        [Test]
        public void SetBattleFormation_NonMember_ThrowsException()
        {
            _party.AddMember(_ray);

            var stranger = new CharacterStats("Stranger", 50, 50, 10, 10, 10);
            Assert.Throws<System.ArgumentException>(() =>
                _party.SetBattleFormation(new List<CharacterStats> { stranger }));
        }

        // ===========================================
        // SwapMember: FF10式入替
        // ===========================================

        [Test]
        public void SwapMember_ValidIndices_SwapsCorrectly()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.AddMember(_mira);
            _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna, _kaito });

            bool result = _party.SwapMember(1, 0); // Luna <-> Mira

            Assert.IsTrue(result);
            var battle = _party.GetBattleMembers();
            var reserve = _party.GetReserveMembers();
            Assert.AreEqual("Mira", battle[1].Name);
            Assert.IsTrue(reserve.Contains(_luna));
        }

        [Test]
        public void SwapMember_NegativeBattleIndex_ReturnsFalse()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.SetBattleFormation(new List<CharacterStats> { _ray });

            Assert.IsFalse(_party.SwapMember(-1, 0));
        }

        [Test]
        public void SwapMember_OutOfRangeBattleIndex_ReturnsFalse()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.SetBattleFormation(new List<CharacterStats> { _ray });

            Assert.IsFalse(_party.SwapMember(5, 0));
        }

        [Test]
        public void SwapMember_OutOfRangeReserveIndex_ReturnsFalse()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.SetBattleFormation(new List<CharacterStats> { _ray });

            Assert.IsFalse(_party.SwapMember(0, 5));
        }

        [Test]
        public void SwapMember_ResetsATBGaugeForIncomingMember()
        {
            var mockATB = new MockATBResettable();
            _party.SetATBSystem(mockATB);

            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_mira);
            _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna });

            _party.SwapMember(1, 0); // Luna out, Mira in

            Assert.AreEqual(_mira, mockATB.LastResetTarget);
        }

        [Test]
        public void SwapMember_FiresOnMemberSwappedEvent()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_mira);
            _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna });

            CharacterStats swappedOut = null;
            CharacterStats swappedIn = null;
            _party.OnMemberSwapped += (outM, inM) => { swappedOut = outM; swappedIn = inM; };

            _party.SwapMember(1, 0); // Luna out, Mira in

            Assert.AreEqual(_luna, swappedOut);
            Assert.AreEqual(_mira, swappedIn);
        }

        // ===========================================
        // EXP配分
        // ===========================================

        [Test]
        public void DistributeEXP_EqualTurns_EqualDistribution()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);

            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_luna);

            var result = _party.DistributeEXP(100);

            Assert.AreEqual(50, result[_ray]);
            Assert.AreEqual(50, result[_luna]);
        }

        [Test]
        public void DistributeEXP_UnequalTurns_ProportionalDistribution()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);

            // Ray: 3 turns, Luna: 1 turn => total 4
            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_luna);

            var result = _party.DistributeEXP(100);

            // Ray: 100 * 3/4 = 75, Luna: 100 * 1/4 = 25
            Assert.AreEqual(75, result[_ray]);
            Assert.AreEqual(25, result[_luna]);
        }

        [Test]
        public void DistributeEXP_ReserveMember_GetsHalfOfAverage()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_mira); // reserve (no turns)

            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_luna);

            var result = _party.DistributeEXP(100);

            // Ray: 100 * 1/2 = 50, Luna: 100 * 1/2 = 50
            // Mira (reserve): avg battle exp = 100/2 = 50, half = 25
            Assert.AreEqual(50, result[_ray]);
            Assert.AreEqual(50, result[_luna]);
            Assert.AreEqual(25, result[_mira]);
        }

        [Test]
        public void DistributeEXP_MinimumGuarantee_AtLeast1()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);

            // Ray took all turns
            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_ray);

            // totalEXP = 2, Luna as reserve: avg = 2/1 = 2, half = 1
            // Try with very small EXP
            var result = _party.DistributeEXP(1);

            // Ray: 1 * 3/3 = 1, Luna (reserve): avg = 1/1 = 1, half = 0 => min 1
            Assert.GreaterOrEqual(result[_ray], 1);
            Assert.GreaterOrEqual(result[_luna], 1);
        }

        [Test]
        public void DistributeEXP_ZeroTotalEXP_ReturnsEmpty()
        {
            _party.AddMember(_ray);
            _party.IncrementTurnCount(_ray);

            var result = _party.DistributeEXP(0);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ResetTurnCounts_ClearsAllCounts()
        {
            _party.AddMember(_ray);
            _party.IncrementTurnCount(_ray);
            _party.IncrementTurnCount(_ray);

            _party.ResetTurnCounts();

            // After reset, distribute should give minimum guarantee
            var result = _party.DistributeEXP(100);
            Assert.AreEqual(1, result[_ray]); // No turns => minimum
        }

        // ===========================================
        // ScheduleManager連携
        // ===========================================

        [Test]
        public void IsAvailableForParty_NoScheduleCheck_AlwaysTrue()
        {
            Assert.IsTrue(_party.IsAvailableForParty(_ray));
        }

        [Test]
        public void IsAvailableForParty_InLesson_ReturnsFalse()
        {
            _party.SetScheduleCheck(member => member.Name == "Luna");

            _party.AddMember(_ray);
            _party.AddMember(_luna);

            Assert.IsTrue(_party.IsAvailableForParty(_ray));
            Assert.IsFalse(_party.IsAvailableForParty(_luna));
        }

        [Test]
        public void GetBattleParty_ExcludesUnavailableMembers()
        {
            _party.SetScheduleCheck(member => member.Name == "Luna");

            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);

            var battleParty = _party.GetBattleParty();

            Assert.AreEqual(2, battleParty.Count);
            Assert.IsFalse(battleParty.Exists(m => m.Name == "Luna"));
        }

        // ===========================================
        // GetBattleParty と SetBattleFormation の連携
        // ===========================================

        [Test]
        public void GetBattleParty_WithFormationSet_ReturnsBattleMembers()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.AddMember(_mira);
            _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna, _kaito });

            var battleParty = _party.GetBattleParty();

            Assert.AreEqual(3, battleParty.Count);
            Assert.IsTrue(battleParty.Contains(_ray));
            Assert.IsTrue(battleParty.Contains(_luna));
            Assert.IsTrue(battleParty.Contains(_kaito));
            Assert.IsFalse(battleParty.Contains(_mira));
        }

        [Test]
        public void GetBattleParty_WithoutFormationSet_ReturnsAllUpToMax()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);

            // SetBattleFormation を呼ばない場合は後方互換で全メンバー返す
            var battleParty = _party.GetBattleParty();

            Assert.AreEqual(3, battleParty.Count);
        }

        [Test]
        public void GetBattleParty_WithFormation_LeaderFirst()
        {
            _party.AddMember(_luna);
            _party.AddMember(_ray);
            _party.AddMember(_kaito);
            _party.SetLeader(_ray);
            _party.SetBattleFormation(new List<CharacterStats> { _luna, _ray, _kaito });

            var battleParty = _party.GetBattleParty();

            Assert.AreEqual(_ray, battleParty[0]);
        }

        [Test]
        public void GetBattleParty_WithFormation_ExcludesUnavailable()
        {
            _party.SetScheduleCheck(m => m.Name == "Luna");
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.SetBattleFormation(new List<CharacterStats> { _ray, _luna, _kaito });

            var battleParty = _party.GetBattleParty();

            Assert.AreEqual(2, battleParty.Count);
            Assert.IsFalse(battleParty.Contains(_luna));
        }

        [Test]
        public void SetBattleFormation_LeaderNotIncluded_AutoAddsLeader()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.AddMember(_kaito);
            _party.SetLeader(_ray);
            // リーダー(Ray)を含まない編成を指定
            _party.SetBattleFormation(new List<CharacterStats> { _luna, _kaito });

            var battleMembers = _party.GetBattleMembers();
            // リーダーが自動追加されている
            Assert.IsTrue(battleMembers.Contains(_ray));
            Assert.IsTrue(battleMembers.Contains(_luna));
            Assert.IsTrue(battleMembers.Contains(_kaito));
        }

        [Test]
        public void SetBattleFormation_NullElement_ThrowsArgumentException()
        {
            _party.AddMember(_ray);
            Assert.Throws<ArgumentException>(() =>
                _party.SetBattleFormation(new List<CharacterStats> { null }));
        }

        [Test]
        public void SetBattleFormation_DuplicateMembers_ThrowsArgumentException()
        {
            _party.AddMember(_ray);
            Assert.Throws<ArgumentException>(() =>
                _party.SetBattleFormation(new List<CharacterStats> { _ray, _ray }));
        }

        // ===========================================
        // 隊列（前列/後列）
        // ===========================================

        [Test]
        public void GetFormation_Default_ReturnsFront()
        {
            Assert.AreEqual(FormationRow.Front, _party.GetFormation(_ray));
        }

        [Test]
        public void SetFormation_Back_ReturnsBack()
        {
            _party.SetFormation(_ray, FormationRow.Back);

            Assert.AreEqual(FormationRow.Back, _party.GetFormation(_ray));
        }

        [Test]
        public void SetFormation_CanChangeBackToFront()
        {
            _party.SetFormation(_ray, FormationRow.Back);
            _party.SetFormation(_ray, FormationRow.Front);

            Assert.AreEqual(FormationRow.Front, _party.GetFormation(_ray));
        }

        // ===========================================
        // Clear
        // ===========================================

        [Test]
        public void Clear_ResetsEverything()
        {
            _party.AddMember(_ray);
            _party.AddMember(_luna);
            _party.SetLeader(_ray);
            _party.SetBattleFormation(new List<CharacterStats> { _ray });
            _party.SetFormation(_ray, FormationRow.Back);
            _party.IncrementTurnCount(_ray);

            _party.Clear();

            Assert.AreEqual(0, _party.Count);
            Assert.IsNull(_party.Leader);
            Assert.AreEqual(0, _party.GetBattleMembers().Count);
            Assert.AreEqual(0, _party.GetReserveMembers().Count);
        }

        // ===========================================
        // Mock classes
        // ===========================================

        private class MockATBResettable : IATBResettable
        {
            public CharacterStats LastResetTarget { get; private set; }

            public void ResetGauge(CharacterStats stats)
            {
                LastResetTarget = stats;
            }
        }
    }
}
