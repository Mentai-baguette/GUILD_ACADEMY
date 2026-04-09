using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Events;
using F = GuildAcademy.Core.Branch.FlagSystem.Flags;

namespace GuildAcademy.Tests.EditMode.Events
{
    [TestFixture]
    public class InfoFlagEventTests
    {
        private FlagSystem _flags;
        private InfoFlagEventRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _flags = new FlagSystem();
            _registry = new InfoFlagEventRegistry(_flags);
        }

        [Test]
        public void Register_AddsEvent()
        {
            var evt = new InfoFlagEventData("test_evt", F.ShionPast, "dlg_test");
            _registry.Register(evt);
            Assert.AreEqual(1, _registry.TotalCount);
        }

        [Test]
        public void Register_DuplicateId_Throws()
        {
            var evt = new InfoFlagEventData("test_evt", F.ShionPast, "dlg_test");
            _registry.Register(evt);
            Assert.Throws<System.InvalidOperationException>(() =>
                _registry.Register(new InfoFlagEventData("test_evt", F.CarlosPlan, "dlg_test2")));
        }

        [Test]
        public void GetAvailableEvents_ExcludesCompleted()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.Register(new InfoFlagEventData("evt2", F.AcademySecret, "dlg2"));

            _flags.Set(F.ShionPast, true);

            var available = _registry.GetAvailableEvents();
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual("evt2", available[0].EventId);
        }

        [Test]
        public void GetAvailableEvents_ExcludesUnmetPrerequisites()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.CarlosPlan, "dlg1",
                prerequisites: new List<string> { F.AcademySecret }));

            var available = _registry.GetAvailableEvents();
            Assert.AreEqual(0, available.Count);

            _flags.Set(F.AcademySecret, true);
            available = _registry.GetAvailableEvents();
            Assert.AreEqual(1, available.Count);
        }

        [Test]
        public void CompleteEvent_SetsFlag()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.CompleteEvent("evt1");
            Assert.IsTrue(_flags.Get(F.ShionPast));
        }

        [Test]
        public void CompleteEvent_FiresOnFlagEventCompleted()
        {
            InfoFlagEventData completed = null;
            _registry.OnFlagEventCompleted += evt => completed = evt;

            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.CompleteEvent("evt1");

            Assert.IsNotNull(completed);
            Assert.AreEqual("evt1", completed.EventId);
        }

        [Test]
        public void CompleteEvent_UnknownId_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => _registry.CompleteEvent("nonexistent"));
        }

        [Test]
        public void CompleteEvent_AlreadyCompleted_Throws()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.CompleteEvent("evt1");
            Assert.Throws<System.InvalidOperationException>(() => _registry.CompleteEvent("evt1"));
        }

        [Test]
        public void CompleteEvent_PrerequisitesUnmet_Throws()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.CarlosPlan, "dlg1",
                prerequisites: new List<string> { F.AcademySecret }));
            Assert.Throws<System.InvalidOperationException>(() => _registry.CompleteEvent("evt1"));
        }

        [Test]
        public void CompletedCount_TracksCorrectly()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.Register(new InfoFlagEventData("evt2", F.AcademySecret, "dlg2"));

            Assert.AreEqual(0, _registry.CompletedCount);
            _registry.CompleteEvent("evt1");
            Assert.AreEqual(1, _registry.CompletedCount);
        }

        [Test]
        public void IsEventAvailable_CompletedEvent_ReturnsFalse()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.CompleteEvent("evt1");
            Assert.IsFalse(_registry.IsEventAvailable("evt1"));
        }

        [Test]
        public void GetCompletedEvents_ReturnsOnlyCompleted()
        {
            _registry.Register(new InfoFlagEventData("evt1", F.ShionPast, "dlg1"));
            _registry.Register(new InfoFlagEventData("evt2", F.AcademySecret, "dlg2"));
            _registry.CompleteEvent("evt1");

            var completed = _registry.GetCompletedEvents();
            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual("evt1", completed[0].EventId);
        }

        [Test]
        public void CreateAll_Returns8Events()
        {
            var all = InfoFlagEventDefinitions.CreateAll();
            Assert.AreEqual(8, all.Count);
        }

        [Test]
        public void CreateAll_AllEventsHaveUniqueIds()
        {
            var all = InfoFlagEventDefinitions.CreateAll();
            var ids = new HashSet<string>();
            foreach (var evt in all)
                Assert.IsTrue(ids.Add(evt.EventId), $"Duplicate event ID: {evt.EventId}");
        }

        [Test]
        public void FullWorkflow_RegisterAllAndCompleteTwo()
        {
            var allEvents = InfoFlagEventDefinitions.CreateAll();
            foreach (var evt in allEvents)
                _registry.Register(evt);

            Assert.AreEqual(8, _registry.TotalCount);

            _registry.CompleteEvent("evt_shion_past");
            _registry.CompleteEvent("evt_academy_secret");

            Assert.AreEqual(2, _registry.CompletedCount);
            Assert.AreEqual(2, _flags.GetActiveCount());

            Assert.IsTrue(_registry.IsEventAvailable("evt_carlos_plan"));
            Assert.IsTrue(_registry.IsEventAvailable("evt_vessel_truth"));
            Assert.IsFalse(_registry.IsEventAvailable("evt_seal_method"));
        }
    }
}
