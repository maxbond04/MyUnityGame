using UnityEngine;
using Game.Core;
using Game.Input;

namespace Game.Gameplay
{
    public class PlayerLaneMover : MonoBehaviour
    {
        [SerializeField] private LaneInput input;
        [SerializeField] private float laneDistance = 3f;
        [SerializeField] private float laneChangeSpeed = 20f;
        [SerializeField] private bool ignoreInputWhileSnapping = false;

        private int laneIndex; // -1 left, 0 center, 1 right

        private float TargetX => laneIndex * laneDistance;

        private void OnEnable()
        {
            input.LaneChangeRequested += OnLaneChangeRequested;
        }

        private void OnDisable()
        {
            input.LaneChangeRequested -= OnLaneChangeRequested;
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
            // New run (not a resume from pause): back to the center lane
            if (to == GameState.Playing && from != GameState.Paused)
            {
                laneIndex = 0;
                var pos = transform.position;
                pos.x = 0f;
                transform.position = pos;
            }
        }

        private void OnLaneChangeRequested(int direction)
        {
            if (GameStateManager.Instance.Current != GameState.Playing) return;
            if (ignoreInputWhileSnapping && !Mathf.Approximately(transform.position.x, TargetX)) return;

            laneIndex = Mathf.Clamp(laneIndex + direction, -1, 1);
        }

        private void Update()
        {
            if (GameStateManager.Instance.Current != GameState.Playing) return;

            var pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, TargetX, laneChangeSpeed * Time.deltaTime);
            transform.position = pos;
        }
    }
}