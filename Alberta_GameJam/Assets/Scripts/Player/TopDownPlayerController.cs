using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownPlayerController : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public Game.Core.InputSystem_Actions inputActions;
        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;


            if (inputActions == null)
            {
                inputActions = new Game.Core.InputSystem_Actions();
            }
            // ...existing code...
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            // WASD movement
            _rb.linearVelocity = _moveInput * moveSpeed;

            // Flip player horizontally when moving left/right (A/D)
            if (_moveInput.x > 0.01f)
            {
                transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            }
            else if (_moveInput.x < -0.01f)
            {
                transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            }

        }
    }
}