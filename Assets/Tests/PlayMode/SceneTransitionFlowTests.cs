using System.Collections;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.Field;
using GuildAcademy.MonoBehaviours.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GuildAcademy.Tests.PlayMode
{
    [TestFixture]
    public class SceneTransitionFlowTests
    {
        private readonly System.Collections.Generic.List<GameObject> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _createdObjects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }

            _createdObjects.Clear();
        }

        [UnityTest]
        public IEnumerator LoadSceneImmediate_SwitchesBetweenConfiguredScenes()
        {
            var managerObject = new GameObject("SceneTransitionManager");
            _createdObjects.Add(managerObject);
            var manager = managerObject.AddComponent<SceneTransitionManager>();

            manager.LoadSceneImmediate(SceneNames.Title);
            yield return null;

            Assert.AreEqual(SceneNames.Title, SceneManager.GetActiveScene().name);

            manager.LoadSceneImmediate(SceneNames.Field);
            yield return null;

            Assert.AreEqual(SceneNames.Field, SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator SceneSpawnResolver_Start_RepositionsPlayerToSpawnPoint()
        {
            var spawnObject = new GameObject("Spawn_default");
            _createdObjects.Add(spawnObject);
            spawnObject.transform.position = new Vector3(4f, -2f, 0f);
            spawnObject.AddComponent<SceneSpawnPoint>();

            var playerObject = new GameObject("Player");
            _createdObjects.Add(playerObject);
            playerObject.AddComponent<Rigidbody2D>();
            playerObject.AddComponent<SceneSpawnResolver>();

            yield return null;

            Assert.AreEqual(spawnObject.transform.position, playerObject.transform.position);
        }
    }
}
