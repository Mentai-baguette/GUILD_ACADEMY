using System.Collections;
using GuildAcademy.Core.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class SceneSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId = "default";
        [SerializeField] private Vector2 _facingDirection = Vector2.down;

        private static int _lastSceneHandle = -1;
        private static bool _spawnApplied;

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

        private IEnumerator Start()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (_lastSceneHandle != activeScene.handle)
            {
                _lastSceneHandle = activeScene.handle;
                _spawnApplied = false;
            }

            yield return null;
            TryApplySpawn();
        }

        private void TryApplySpawn()
        {
            if (_spawnApplied) return;

            var requestedSpawnId = SceneTransitionData.Get(ScenePortal2D.SpawnPointKey, string.Empty);
            var useDefaultSpawn = string.IsNullOrEmpty(requestedSpawnId);

            if (useDefaultSpawn)
            {
                if (!string.Equals(_spawnId, "default", System.StringComparison.Ordinal)) return;
            }
            else
            {
                if (!string.Equals(_spawnId, requestedSpawnId, System.StringComparison.Ordinal)) return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            player.transform.position = transform.position;
            ApplyFacing(player);

            _spawnApplied = true;
            SceneTransitionData.Set(ScenePortal2D.SpawnPointKey, string.Empty);
        }

        private void ApplyFacing(GameObject player)
        {
            var animator = player.GetComponent<Animator>();
            if (animator == null) return;

            animator.SetFloat("MoveX", _facingDirection.x);
            animator.SetFloat("MoveY", _facingDirection.y);
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
