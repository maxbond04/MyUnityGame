using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class LaneInput : MonoBehaviour
    {
        // -1 = left, +1 = right
        public event Action<int> LaneChangeRequested;
        public event Action JumpRequested;

        private InputAction moveLeft;
        private InputAction moveRight;
        private InputAction jump;

        private void Awake()
        {
            moveLeft = new InputAction("MoveLeft", InputActionType.Button);
            moveLeft.AddBinding("<Keyboard>/leftArrow");
            moveLeft.AddBinding("<Keyboard>/a");

            moveRight = new InputAction("MoveRight", InputActionType.Button);
            moveRight.AddBinding("<Keyboard>/rightArrow");
            moveRight.AddBinding("<Keyboard>/d");

            jump = new InputAction("Jump", InputActionType.Button);
            jump.AddBinding("<Keyboard>/space");
            jump.AddBinding("<Keyboard>/upArrow");
            jump.AddBinding("<Keyboard>/w");
        }

        private void OnEnable()
        {
            moveLeft.performed += OnLeft;
            moveRight.performed += OnRight;
            jump.performed += OnJump;
            moveLeft.Enable();
            moveRight.Enable();
            jump.Enable();
        }

        private void OnDisable()
        {
            moveLeft.performed -= OnLeft;
            moveRight.performed -= OnRight;
            jump.performed -= OnJump;
            moveLeft.Disable();
            moveRight.Disable();
            jump.Disable();
        }

        private void OnDestroy()
        {
            moveLeft?.Dispose();
            moveRight?.Dispose();
            jump?.Dispose();
        }

        private void OnLeft(InputAction.CallbackContext _) => LaneChangeRequested?.Invoke(-1);
        private void OnRight(InputAction.CallbackContext _) => LaneChangeRequested?.Invoke(1);
        private void OnJump(InputAction.CallbackContext _) => JumpRequested?.Invoke();
    }
}