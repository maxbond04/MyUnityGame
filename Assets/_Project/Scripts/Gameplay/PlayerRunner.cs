using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class PlayerRunner : MonoBehaviour
    {
        [SerializeField] private float baseSpeed = 10f;
        [SerializeField] private float speedIncrease = 0.5f;
        [SerializeField] private float rampInterval = 10f;

        public float CurrentSpeed { get; private set; }
        public float Distance => transform.position.z - startZ;

        private Vector3 startPosition;
        private float startZ;
        private float survivedTime;
        private bool running;

        private void Start()
        {
            startPosition = transform.position;
            startZ = startPosition.z;
            CurrentSpeed = baseSpeed;

            var manager = GameStateManager.Instance;
            manager.StateChanged += OnStateChanged;
            running = manager.Current == GameState.Playing;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState from, GameState to)
        {
            // New run (not a resume from pause): reset position and timer
            if (to == GameState.Playing && from != GameState.Paused)
            {
                transform.position = startPosition;
                survivedTime = 0f;
                CurrentSpeed = baseSpeed;
            }
            running = to == GameState.Playing;
        }

        private void Update()
        {
            if (!running) return;

            survivedTime += Time.deltaTime;
            CurrentSpeed = baseSpeed + speedIncrease * Mathf.Floor(survivedTime / rampInterval);
            transform.Translate(Vector3.forward * (CurrentSpeed * Time.deltaTime), Space.World);
        }

        private GUIStyle hudStyle;

        // TEMPORARY debug HUD, replace with real UI
        private void OnGUI()
        {
            hudStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 28 };
            GUI.Label(new Rect(20, 20, 500, 100),
                $"Speed: {CurrentSpeed:F1}\nDistance: {Distance:F0} m", hudStyle);
        }
    }
}