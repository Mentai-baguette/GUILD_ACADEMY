using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId = "default";
        [SerializeField] private Vector2 _facingDirection = Vector2.down;

        public string SpawnId => _spawnId;
        public Vector2 FacingDirection => _facingDirection;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_spawnId))
                _spawnId = "default";

            if (_facingDirection.sqrMagnitude < 0.0001f)
                _facingDirection = Vector2.down;
            else
                _facingDirection.Normalize();
        }
#endif
    }
}
