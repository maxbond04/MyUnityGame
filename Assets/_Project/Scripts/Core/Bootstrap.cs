using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string firstScene = "Game";

        private void Start() => SceneManager.LoadScene(firstScene);
    }
}