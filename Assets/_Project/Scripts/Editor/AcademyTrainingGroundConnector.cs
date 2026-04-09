using System;
using GuildAcademy.MonoBehaviours.Field;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GuildAcademy.EditorTools
{
    public static class AcademyTrainingGroundConnector
    {
        private const string HallwayScenePath = "Assets/Scenes/Academy/Academy_Hallway.unity";
        private const string TrainingGroundScenePath = "Assets/Scenes/Academy/Academy_TrainingGround.unity";

        [MenuItem("GuildAcademy/Maps/Connect Hallway <-> TrainingGround")]
        public static void ConnectHallwayAndTrainingGround()
        {
            if (!System.IO.File.Exists(HallwayScenePath) || !System.IO.File.Exists(TrainingGroundScenePath))
            {
                EditorUtility.DisplayDialog("TrainingGround Connector", "必要なシーンが見つかりません。Hallway / TrainingGround のシーンを先に作成してください。", "OK");
                return;
            }

            ConnectFromHallway();
            ConnectFromTrainingGround();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("TrainingGround Connector", "Hallway と TrainingGround の往復接続を更新しました。", "OK");
        }

        private static void ConnectFromHallway()
        {
            var hallwayScene = EditorSceneManager.OpenScene(HallwayScenePath, OpenSceneMode.Single);

            var portals = EnsureRoot("Portals");
            EnsurePortal(
                portals,
                "ToTrainingGround",
                new Vector2(0f, -6f),
                new Vector2(1.6f, 1.6f),
                "Academy_TrainingGround",
                "from_hallway");

            var spawnPoints = EnsureRoot("SpawnPoints");
            EnsureSpawnPoint(
                spawnPoints,
                "Spawn_from_training_ground",
                new Vector2(0f, -5f),
                "from_training_ground",
                Vector2.up);

            EditorSceneManager.MarkSceneDirty(hallwayScene);
            EditorSceneManager.SaveScene(hallwayScene);
        }

        private static void ConnectFromTrainingGround()
        {
            var trainingScene = EditorSceneManager.OpenScene(TrainingGroundScenePath, OpenSceneMode.Single);

            var portals = EnsureRoot("Portals");
            EnsurePortal(
                portals,
                "ToHallway",
                new Vector2(0f, 7f),
                new Vector2(1.6f, 1.6f),
                "Academy_Hallway",
                "from_training_ground");

            var spawnPoints = EnsureRoot("SpawnPoints");
            EnsureSpawnPoint(
                spawnPoints,
                "Spawn_from_hallway",
                new Vector2(0f, -5f),
                "from_hallway",
                Vector2.up);

            EditorSceneManager.MarkSceneDirty(trainingScene);
            EditorSceneManager.SaveScene(trainingScene);
        }

        private static Transform EnsureRoot(string rootName)
        {
            var rootObject = GameObject.Find(rootName);
            if (rootObject == null)
                rootObject = new GameObject(rootName);

            return rootObject.transform;
        }

        private static void EnsurePortal(
            Transform parent,
            string portalName,
            Vector2 localPosition,
            Vector2 colliderSize,
            string targetScene,
            string targetSpawnPoint)
        {
            var portalObject = FindOrCreateChild(parent, portalName);
            portalObject.transform.localPosition = localPosition;

            var collider = portalObject.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = portalObject.AddComponent<BoxCollider2D>();

            collider.isTrigger = true;
            collider.size = colliderSize;

            var portal = portalObject.GetComponent<ScenePortal2D>();
            if (portal == null)
                portal = portalObject.AddComponent<ScenePortal2D>();

            var serialized = new SerializedObject(portal);
            serialized.FindProperty("_targetSceneName").stringValue = targetScene;
            serialized.FindProperty("_targetSpawnPointId").stringValue = targetSpawnPoint;
            serialized.FindProperty("_playerTag").stringValue = "Player";
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureSpawnPoint(
            Transform parent,
            string spawnObjectName,
            Vector2 localPosition,
            string spawnId,
            Vector2 facingDirection)
        {
            var spawnObject = FindOrCreateChild(parent, spawnObjectName);
            spawnObject.transform.localPosition = localPosition;

            var spawnPoint = spawnObject.GetComponent<SceneSpawnPoint>();
            if (spawnPoint == null)
                spawnPoint = spawnObject.AddComponent<SceneSpawnPoint>();

            var serialized = new SerializedObject(spawnPoint);
            serialized.FindProperty("_spawnId").stringValue = spawnId;
            serialized.FindProperty("_facingDirection").vector2Value = facingDirection.normalized;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindOrCreateChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
                return child.gameObject;

            var childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            return childObject;
        }
    }
}
