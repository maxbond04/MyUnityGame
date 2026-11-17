using System;
using UnityEngine;

namespace Game.Gameplay
{
    // TEMPORARY: FPS and memory readout for profiling the pooling (2.1.4). Delete with the other dev code.
    public class DevStats : MonoBehaviour
    {
        private float timer;
        private int frames;
        private string text = "";
        private GUIStyle style;

        private void Update()
        {
            frames++;
            timer += Time.unscaledDeltaTime;
            if (timer < 0.5f) return;

            float fps = frames / timer;
            float memoryMb = GC.GetTotalMemory(false) / (1024f * 1024f);
            text = $"FPS: {fps:F0}\nMem: {memoryMb:F1} MB\nGC runs: {GC.CollectionCount(0)}";
            frames = 0;
            timer = 0f;
        }

        private void OnGUI()
        {
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 22 };
            GUI.Label(new Rect(20, 110, 400, 120), text, style);
        }
    }
}