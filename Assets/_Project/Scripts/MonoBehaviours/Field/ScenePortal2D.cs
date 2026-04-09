using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.UI;
using System;
using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Field
{
    [RequireComponent(typeof(Collider2D))]
    public class ScenePortal2D : MonoBehaviour
    {
        private const string SpawnPointKey = "spawnPointId";

        public static Func<string, bool> SceneLoadOverride { get; set; }
        [Header("Target")]
        [SerializeField] private string _targetSceneName;
        [SerializeField] private string _targetSpawnPointId = "default";

        [Header("Trigger")]
        [SerializeField] private string _playerTag = "Player";

        private void Reset()
        {
            var collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (string.IsNullOrWhiteSpace(_targetSceneName))
                return;

            if (!other.CompareTag(_playerTag))
                return;

            SceneTransitionData.Set(SpawnPointKey, _targetSpawnPointId);

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(_targetSceneName);
            else if (SceneLoadOverride != null && SceneLoadOverride(_targetSceneName))
                return;
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(_targetSceneName);
        }
    }
}
