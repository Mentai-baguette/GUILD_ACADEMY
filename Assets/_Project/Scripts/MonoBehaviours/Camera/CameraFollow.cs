using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Camera
{
    /// <summary>
    /// シンプルなカメラ追従スクリプト。
    /// プレイヤーオブジェクト（Player タグ）を追従し、Z軸（奥行き）は固定。
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float _smoothSpeed = 0.1f;
        [SerializeField] private Vector3 _offset = Vector3.zero;

        private Transform _target;
        private float _initialZ;

        private void Start()
        {
            _target = GameObject.FindWithTag("Player")?.transform;
            if (_target == null)
            {
                Debug.LogWarning("CameraFollow: Player tagged object not found!");
                enabled = false;
                return;
            }

            _initialZ = transform.position.z;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // ターゲット位置を計算（オフセット適用）
            Vector3 targetPos = _target.position + _offset;
            targetPos.z = _initialZ; // Z軸は固定（カメラの深度）

            // スムーズに移動
            transform.position = Vector3.Lerp(transform.position, targetPos, _smoothSpeed);
        }
    }
}
