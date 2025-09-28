using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownPlayerController : Singleton<TopDownPlayerController>
    {
        public enum State
        {
            Idle,
            Moving
        }

        public float moveSpeed = 6f;
        public float batteryDrainRate;
        public Game.Core.InputSystem_Actions inputActions;
        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private SoundWord _soundWord;
        private Animator _animator;
        public State state { get; private set; }
        public float battery { get; private set; }
        public Action<float> BatteryChanged;

        public override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            _soundWord = GetComponentInChildren<SoundWord>();
            _animator = GetComponentInChildren<Animator>();


            if (inputActions == null)
            {
                inputActions = new Game.Core.InputSystem_Actions();
            }
            battery = 1f;
        }

        private void OnEnable()
        {
            inputActions.Enable();
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnCancelMove;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnCancelMove;
            inputActions.Disable();
            BatteryChanged = null;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
            EnterMoving();
        }

        private void OnCancelMove(InputAction.CallbackContext ctx)
        {
            _moveInput = Vector2.zero;
            EnterIdle();
        }

        void Update()
        {
            switch (state)
            {
                case State.Idle:
                    HandleIdle();
                    break;
                case State.Moving:
                    HandleMoving();
                    break;
            }
            BatteryDrain();
        }

        private void FixedUpdate()
        {
            switch (state)
            {
                case State.Idle:
                    HandleIdlePhysics();
                    break;
                case State.Moving:
                    HandleMovingPhysics();
                    break;
            }
        }

        void EnterIdle()
        {
            _rb.linearVelocity = Vector3.zero;
            state = State.Idle;
            _animator.SetBool("Moving", false);
        }

        void EnterMoving()
        {
            state = State.Moving;
            _animator.SetBool("Moving", true);
        }

        void HandleIdle()
        {

        }

        void HandleIdlePhysics()
        {

        }

        void HandleMoving()
        {
            _soundWord.Spawn(transform.position, Vector3.up, 1f);
        }

        void HandleMovingPhysics()
        {
            _rb.linearVelocity = _moveInput * moveSpeed;

            if (_moveInput.x > 0.01f)
            {
                transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            }
            else if (_moveInput.x < -0.01f)
            {
                transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            }
        }

        void BatteryDrain()
        {
            battery -= batteryDrainRate * Time.deltaTime;
            battery = Mathf.Max(0, battery);
            BatteryChanged?.Invoke(battery);
        }

        public void ChargeBattery(float amount)
        {
            battery += amount;
            battery = Mathf.Min(1f, battery);
            BatteryChanged?.Invoke(battery);
        }
    }
}