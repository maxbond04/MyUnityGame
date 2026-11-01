using System;
using UnityEngine;

namespace Game.Core
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState Current { get; private set; } = GameState.Boot;

        public event Action<GameState, GameState> StateChanged; // (from, to)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start() => SetState(GameState.MainMenu);

        public void SetState(GameState next)
        {
            if (next == Current) return;

            var prev = Current;
            Current = next;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;

            Debug.Log($"[GameState] {prev} -> {next}");
            StateChanged?.Invoke(prev, next);
        }

        public void StartRun() => SetState(GameState.Playing);
        public void Pause() { if (Current == GameState.Playing) SetState(GameState.Paused); }
        public void Resume() { if (Current == GameState.Paused) SetState(GameState.Playing); }
        public void EndRun() { if (Current == GameState.Playing) SetState(GameState.GameOver); }
        public void ToMenu() => SetState(GameState.MainMenu);
    }
}