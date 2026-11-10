using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class LaneInput : MonoBehaviour
    {
        // -1 = left, +1 = right
        public event Action<int> LaneChangeRequested;

        private InputAction moveLeft;
        private InputAction moveRight;

        private void Awake()
        {
            moveLeft = new InputAction("MoveLeft", InputActionType.Button);
            moveLeft.AddBinding("<Keyboard>/leftArrow");
            moveLeft.AddBinding("<Keyboard>/a");

            moveRight = new InputAction("MoveRight", InputActionType.Button);
            moveRight.AddBinding("<Keyboard>/rightArrow");
            moveRight.AddBinding("<Keyboard>/d");
        }

        private void OnEnable()
        {
            moveLeft.performed += OnLeft;
            moveRight.performed += OnRight;
            moveLeft.Enable();
            moveRight.Enable();
        }

        private void OnDisable()
        {
            moveLeft.performed -= OnLeft;
            moveRight.performed -= OnRight;
            moveLeft.Disable();
            moveRight.Disable();
        }

        private void OnDestroy()
        {
            moveLeft?.Dispose();
            moveRight?.Dispose();
        }

        private void OnLeft(InputAction.CallbackContext _) => LaneChangeRequested?.Invoke(-1);
        private void OnRight(InputAction.CallbackContext _) => LaneChangeRequested?.Invoke(1);
    }
}