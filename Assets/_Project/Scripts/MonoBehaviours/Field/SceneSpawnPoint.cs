using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId = "default";
        [SerializeField] private Vector2 _facingDirection = Vector2.down;

        public string SpawnId => _spawnId;
        public Vector2 FacingDirection => _facingDirection;

        public static string NormalizeSpawnId(string spawnId)
        {
            return string.IsNullOrWhiteSpace(spawnId) ? "default" : spawnId;
        }

        public static Vector2 NormalizeFacingDirection(Vector2 facingDirection)
        {
            if (facingDirection.sqrMagnitude < 0.0001f)
                return Vector2.down;

            return facingDirection.normalized;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spawnId = NormalizeSpawnId(_spawnId);
            _facingDirection = NormalizeFacingDirection(_facingDirection);
        }
#endif
    }
}
