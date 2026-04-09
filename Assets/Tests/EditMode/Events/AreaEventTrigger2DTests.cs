using System.Reflection;
using GuildAcademy.Core.Branch;
using GuildAcademy.MonoBehaviours.Branch;
using GuildAcademy.MonoBehaviours.Events;
using GuildAcademy.MonoBehaviours.Field;
using NUnit.Framework;
using UnityEngine;

namespace GuildAcademy.Tests.EditMode.Events
{
    [TestFixture]
    public class AreaEventTrigger2DTests
    {
        private readonly System.Collections.Generic.List<GameObject> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (var i = _createdObjects.Count - 1; i >= 0; i--)
            {
                if (_createdObjects[i] != null)
                    Object.DestroyImmediate(_createdObjects[i]);
            }
            _createdObjects.Clear();
        }

        [Test]
        public void TryTriggerByTag_NonPlayer_DoesNotConsume()
        {
            var trigger = CreateTrigger();

            var handled = trigger.TryTriggerByTag(false);

            Assert.IsFalse(handled);
            Assert.IsFalse(trigger.IsConsumed);
        }

        [Test]
        public void TryTriggerByTag_TargetFlag_UpdatesBranchFlagAndConsumes()
        {
            CreateBranchManager();
            var trigger = CreateTrigger();
            SetPrivateField(trigger, "_useInfoFlagEvent", false);
            SetPrivateField(trigger, "_targetFlagName", FlagSystem.Flags.AcademySecret);
            SetPrivateField(trigger, "_targetFlagValue", true);

            var first = trigger.TryTriggerByTag(true);
            var second = trigger.TryTriggerByTag(true);

            Assert.IsTrue(first);
            Assert.IsFalse(second);
            Assert.IsTrue(trigger.IsConsumed);
            Assert.IsTrue(BranchManager.Instance.GetFlag(FlagSystem.Flags.AcademySecret));
        }

        [Test]
        public void TryTriggerByTag_InfoEvent_CompletesEventAndConsumes()
        {
            var branchManager = CreateBranchManager();
            var infoManager = CreateInfoFlagEventManager(branchManager);
            var trigger = CreateTrigger();
            SetPrivateField(trigger, "_eventId", "evt_shion_past");
            SetPrivateField(trigger, "_useInfoFlagEvent", true);
            SetPrivateField(trigger, "_targetFlagName", string.Empty);

            var handled = trigger.TryTriggerByTag(true);

            Assert.IsTrue(handled);
            Assert.IsTrue(trigger.IsConsumed);
            Assert.IsNotNull(infoManager.Registry);
            Assert.IsTrue(branchManager.GetFlag(FlagSystem.Flags.ShionPast));
        }

        private BranchManager CreateBranchManager()
        {
            var branchObject = new GameObject("BranchManager");
            _createdObjects.Add(branchObject);
            var branchManager = branchObject.AddComponent<BranchManager>();
            return branchManager;
        }

        private InfoFlagEventManager CreateInfoFlagEventManager(BranchManager branchManager)
        {
            var infoObject = new GameObject("InfoFlagEventManager");
            _createdObjects.Add(infoObject);
            var infoManager = infoObject.AddComponent<InfoFlagEventManager>();
            infoManager.Initialize(branchManager.Service.Flags);
            return infoManager;
        }

        private AreaEventTrigger2D CreateTrigger()
        {
            var triggerObject = new GameObject("AreaEventTrigger");
            _createdObjects.Add(triggerObject);
            triggerObject.AddComponent<BoxCollider2D>();
            var trigger = triggerObject.AddComponent<AreaEventTrigger2D>();
            SetPrivateField(trigger, "_oneShot", true);
            return trigger;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field not found: {fieldName}");
            field.SetValue(target, value);
        }
    }
}
