using GuildAcademy.Core.Data;
using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class SceneSpawnResolver : MonoBehaviour
    {
        private const string SpawnPointKey = "spawnPointId";

        [SerializeField] private string _fallbackSpawnPointId = "default";

        private void Start()
        {
            var targetSpawnId = SceneTransitionData.Get(SpawnPointKey, _fallbackSpawnPointId);
            var spawnPoints = FindObjectsByType<SceneSpawnPoint>();

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
