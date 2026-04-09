using GuildAcademy.Core.Data;
using System;
using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class SceneSpawnResolver : MonoBehaviour
    {
        private const string SpawnPointKey = "spawnPointId";

        public static Func<SceneSpawnPoint[]> SpawnPointProvider { get; set; }

        [SerializeField] private string _fallbackSpawnPointId = "default";

        private void Start()
        {
            var targetSpawnId = SceneTransitionData.Get(SpawnPointKey, _fallbackSpawnPointId);
            var spawnPoints = SpawnPointProvider != null ? SpawnPointProvider() : FindObjectsByType<SceneSpawnPoint>();

            if (spawnPoints == null)
                spawnPoints = Array.Empty<SceneSpawnPoint>();

            foreach (var spawnPoint in spawnPoints)
            {
                if (!string.Equals(spawnPoint.SpawnId, targetSpawnId, System.StringComparison.Ordinal))
                    continue;

                transform.position = spawnPoint.transform.position;
                SceneTransitionData.Remove(SpawnPointKey);
                return;
            }

            SceneTransitionData.Remove(SpawnPointKey);
        }
    }
}
