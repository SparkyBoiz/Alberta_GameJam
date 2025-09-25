using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class TopDownPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f; // deg/sec towards aim direction

        [Header("Input")]
        public Game.Core.InputSystem_Actions inputActions;

        private CharacterController _cc;
        private Camera _cam;
        private Vector2 _moveInput;
        private Vector2 _aimInput; // from mouse delta or right stick

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cam = Camera.main;

            if (inputActions == null)
            {
                inputActions = new Game.Core.InputSystem_Actions();
            }
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;

            inputActions.Player.Aim.performed += OnAim;
            inputActions.Player.Aim.canceled += OnAim;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;

            inputActions.Player.Aim.performed -= OnAim;
            inputActions.Player.Aim.canceled -= OnAim;
            inputActions.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void OnAim(InputAction.CallbackContext ctx)
        {
            _aimInput = ctx.ReadValue<Vector2>();
        }

        private void Update()
        {
            // Movement on XZ plane (top-down)
            Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
            _cc.SimpleMove(move * moveSpeed);

            // Aim priority: right stick vector -> mouse world pos -> move direction
            Vector3 aimDir = Vector3.zero;

            // 1) Gamepad right stick or other look vector
            if (_aimInput.sqrMagnitude > 0.01f)
            {
                aimDir = new Vector3(_aimInput.x, 0f, _aimInput.y);
            }
            else if (Mouse.current != null && _cam != null)
            {
                // 2) Mouse world ray to plane
                var mousePos = Mouse.current.position.ReadValue();
                Ray ray = _cam.ScreenPointToRay(mousePos);
                if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    aimDir = (hit - transform.position);
                    aimDir.y = 0f;
                }
            }

            if (aimDir.sqrMagnitude <= 0.0001f)
            {
                // 3) Fallback to movement direction
                aimDir = move;
            }

            if (aimDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(aimDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }
}