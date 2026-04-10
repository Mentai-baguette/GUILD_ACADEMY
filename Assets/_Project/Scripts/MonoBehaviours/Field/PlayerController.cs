using UnityEngine;
using UnityEngine.InputSystem;

namespace GuildAcademy.MonoBehaviours.Field
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 3.0f;

        private Rigidbody2D _rb;
        private Animator _animator;
        private Vector2 _moveInput;
        private bool _canMove = true;

        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");

        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
                if (!_canMove)
                {
                    _moveInput = Vector2.zero;
                    if (_rb != null)
                        _rb.linearVelocity = Vector2.zero;
                }
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();

            if (GetComponent<SceneSpawnResolver>() == null)
                gameObject.AddComponent<SceneSpawnResolver>();
        }

        public void OnMove(InputValue value)
        {
            if (!_canMove)
            {
                _moveInput = Vector2.zero;
                return;
            }
            _moveInput = value.Get<Vector2>();
            if (_moveInput.sqrMagnitude > 1f)
                _moveInput.Normalize();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _moveInput * _moveSpeed;
        }

        private void LateUpdate()
        {
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;
            bool moving = _moveInput.sqrMagnitude > 0.01f;
            _animator.SetBool(IsMoving, moving);
            if (moving)
            {
                _animator.SetFloat(MoveX, _moveInput.x);
                _animator.SetFloat(MoveY, _moveInput.y);
            }
        }
    }
}
