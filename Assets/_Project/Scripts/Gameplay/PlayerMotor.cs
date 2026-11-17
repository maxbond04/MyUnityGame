using UnityEngine;
using Game.Core;
using Game.Input;

namespace Game.Gameplay
{
    // The only script that moves the player: forward (Z), lanes (X), jump and gravity (Y).
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private LaneInput input;
        [SerializeField] private PlayerRunner runner;
        [SerializeField] private PlayerLaneMover laneMover;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravityScale = 2f;

        private CharacterController controller;
        private Vector3 startPosition;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            startPosition = transform.position;
        }

        private void OnEnable()
        {
            input.JumpRequested += OnJumpRequested;
        }

        private void OnDisable()
        {
            input.JumpRequested -= OnJumpRequested;
        }

        private void Start()
        {
            GameStateManager.Instance.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState from, GameState to)
        {
            // New run (not a resume from pause): back to the start position
            if (to == GameState.Playing && from != GameState.Paused)
            {
                controller.enabled = false; // CharacterController must be off to teleport
                transform.position = startPosition;
                controller.enabled = true;
                verticalVelocity = 0f;
            }
        }

        private void OnJumpRequested()
        {
            if (GameStateManager.Instance.Current != GameState.Playing) return;
            if (!controller.isGrounded) return;

            // Initial upward velocity that reaches exactly jumpHeight under our gravity
            float gravity = -Physics.gravity.y * gravityScale;
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }

        private void Update()
        {
            if (GameStateManager.Instance.Current != GameState.Playing) return;

            float dt = Time.deltaTime;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f; // keeps the controller pressed to the ground
            verticalVelocity += Physics.gravity.y * gravityScale * dt;

            Vector3 pos = transform.position;
            float newX = laneMover.StepX(pos.x, dt);

            controller.Move(new Vector3(newX - pos.x, verticalVelocity * dt, runner.CurrentSpeed * dt));
        }
    }
}