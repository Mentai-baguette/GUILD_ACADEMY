using System;
using System.Reflection;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.Field;
using NUnit.Framework;
using UnityEngine;

namespace GuildAcademy.Tests.EditMode.Field
{
    [TestFixture]
    public class FieldTransitionTests
    {
        private string _capturedSceneName;
        private Func<string, bool> _previousSceneLoadOverride;
        private Func<SceneSpawnPoint[]> _previousSpawnPointProvider;

        [SetUp]
        public void SetUp()
        {
            SceneTransitionData.Clear();
            _capturedSceneName = null;
            _previousSceneLoadOverride = ScenePortal2D.SceneLoadOverride;
            _previousSpawnPointProvider = SceneSpawnResolver.SpawnPointProvider;
            ScenePortal2D.SceneLoadOverride = sceneName =>
            {
                _capturedSceneName = sceneName;
                return true;
            };
            SceneSpawnResolver.SpawnPointProvider = null;
        }

        [TearDown]
        public void TearDown()
        {
            ScenePortal2D.SceneLoadOverride = _previousSceneLoadOverride;
            SceneSpawnResolver.SpawnPointProvider = _previousSpawnPointProvider;
            SceneTransitionData.Clear();
        }

        [Test]
        public void SceneTransitionData_SetGetRemoveAndClear_Work()
        {
            Assert.IsFalse(SceneTransitionData.Has("spawnPointId"));

            SceneTransitionData.Set("spawnPointId", "from_hallway");
            Assert.IsTrue(SceneTransitionData.Has("spawnPointId"));
            Assert.AreEqual("from_hallway", SceneTransitionData.Get("spawnPointId", "default"));

            SceneTransitionData.Remove("spawnPointId");
            Assert.IsFalse(SceneTransitionData.Has("spawnPointId"));
            Assert.AreEqual("default", SceneTransitionData.Get("spawnPointId", "default"));

            SceneTransitionData.Set("spawnPointId", "from_hallway");
            SceneTransitionData.Clear();
            Assert.IsFalse(SceneTransitionData.Has("spawnPointId"));
        }

        [Test]
        public void SceneSpawnPoint_NormalizeHelpers_ReturnExpectedValues()
        {
            Assert.AreEqual("default", SceneSpawnPoint.NormalizeSpawnId(null));
            Assert.AreEqual("default", SceneSpawnPoint.NormalizeSpawnId("   "));
            Assert.AreEqual("from_hallway", SceneSpawnPoint.NormalizeSpawnId("from_hallway"));

            var normalized = SceneSpawnPoint.NormalizeFacingDirection(new Vector2(3f, 4f));
            Assert.That(normalized.x, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(normalized.y, Is.EqualTo(0.8f).Within(0.0001f));

            var fallback = SceneSpawnPoint.NormalizeFacingDirection(Vector2.zero);
            Assert.That(fallback.x, Is.EqualTo(Vector2.down.x).Within(0.0001f));
            Assert.That(fallback.y, Is.EqualTo(Vector2.down.y).Within(0.0001f));
        }

        [Test]
        public void ScenePortal2D_PlayerCollision_SetsSpawnPointAndRequestsSceneLoad()
        {
            var portalObject = new GameObject("Portal");
            portalObject.AddComponent<BoxCollider2D>();
            var portal = portalObject.AddComponent<ScenePortal2D>();
            SetPrivateField(portal, "_targetSceneName", "Academy_Classroom");
            SetPrivateField(portal, "_targetSpawnPointId", "from_hallway");
            SetPrivateField(portal, "_playerTag", "Untagged");

            var playerObject = new GameObject("Player");
            playerObject.AddComponent<BoxCollider2D>();

            InvokePrivateMethod(portal, "OnTriggerEnter2D", playerObject.GetComponent<Collider2D>());

            Assert.AreEqual("Academy_Classroom", _capturedSceneName);
            Assert.IsTrue(SceneTransitionData.Has("spawnPointId"));
            Assert.AreEqual("from_hallway", SceneTransitionData.Get("spawnPointId", string.Empty));

            UnityEngine.Object.DestroyImmediate(playerObject);
            UnityEngine.Object.DestroyImmediate(portalObject);
        }

        [Test]
        public void ScenePortal2D_NonPlayerCollision_DoesNothing()
        {
            var portalObject = new GameObject("Portal");
            portalObject.AddComponent<BoxCollider2D>();
            var portal = portalObject.AddComponent<ScenePortal2D>();
            SetPrivateField(portal, "_targetSceneName", "Academy_Classroom");
            SetPrivateField(portal, "_targetSpawnPointId", "from_hallway");
            SetPrivateField(portal, "_playerTag", "MainCamera");

            var otherObject = new GameObject("Other");
            otherObject.AddComponent<BoxCollider2D>();

            InvokePrivateMethod(portal, "OnTriggerEnter2D", otherObject.GetComponent<Collider2D>());

            Assert.IsNull(_capturedSceneName);
            Assert.IsFalse(SceneTransitionData.Has("spawnPointId"));

            UnityEngine.Object.DestroyImmediate(otherObject);
            UnityEngine.Object.DestroyImmediate(portalObject);
        }

        [Test]
        public void SceneSpawnResolver_StartMovesPlayerToMatchingSpawnPoint()
        {
            var resolverObject = new GameObject("Resolver");
            resolverObject.transform.position = new Vector3(10f, 11f, 0f);
            var resolver = resolverObject.AddComponent<SceneSpawnResolver>();

            var spawnPointObject = new GameObject("SpawnPoint");
            spawnPointObject.transform.position = new Vector3(3f, 4f, 0f);
            spawnPointObject.AddComponent<SceneSpawnPoint>();

            SceneSpawnResolver.SpawnPointProvider = () => new[] { spawnPointObject.GetComponent<SceneSpawnPoint>() };
            SceneTransitionData.Set("spawnPointId", "default");

            InvokePrivateMethod(resolver, "Start");

            Assert.That(resolverObject.transform.position, Is.EqualTo(new Vector3(3f, 4f, 0f)));
            Assert.IsFalse(SceneTransitionData.Has("spawnPointId"));

            UnityEngine.Object.DestroyImmediate(spawnPointObject);
            UnityEngine.Object.DestroyImmediate(resolverObject);
        }

        [Test]
        public void SceneSpawnResolver_StartClearsKeyWhenNoSpawnPointMatches()
        {
            var resolverObject = new GameObject("Resolver");
            resolverObject.transform.position = new Vector3(7f, 8f, 0f);
            var resolver = resolverObject.AddComponent<SceneSpawnResolver>();

            SceneSpawnResolver.SpawnPointProvider = () => Array.Empty<SceneSpawnPoint>();
            SceneTransitionData.Set("spawnPointId", "missing");

            InvokePrivateMethod(resolver, "Start");

            Assert.That(resolverObject.transform.position, Is.EqualTo(new Vector3(7f, 8f, 0f)));
            Assert.IsFalse(SceneTransitionData.Has("spawnPointId"));

            UnityEngine.Object.DestroyImmediate(resolverObject);
        }

        [Test]
        public void PlayerController_Awake_AddsSceneSpawnResolverIfMissing()
        {
            var playerObject = new GameObject("Player");
            playerObject.AddComponent<Rigidbody2D>();
            var playerController = playerObject.AddComponent<PlayerController>();

            InvokePrivateMethod(playerController, "Awake");

            Assert.IsNotNull(playerObject.GetComponent<SceneSpawnResolver>());

            UnityEngine.Object.DestroyImmediate(playerObject);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName, fieldName);

            field.SetValue(target, value);
        }

        private static object InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new MissingMethodException(target.GetType().FullName, methodName);

            return method.Invoke(target, args);
        }
    }
}