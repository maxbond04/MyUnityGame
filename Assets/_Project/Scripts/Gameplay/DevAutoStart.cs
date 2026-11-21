using System.Collections;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    // TEMPORARY: starts the run without UI. Delete when 2.4 is done.
    public class DevAutoStart : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null; // wait one frame so GameStateManager finishes Boot -> MainMenu
            GameStateManager.Instance.StartRun();
        }
    }
}