using UnityEngine;
using UnityEngine.Pool;

namespace Game.Gameplay
{
    // Sits on the root of every track chunk prefab.
    public class TrackChunk : MonoBehaviour
    {
        [SerializeField] private float length = 30f;
        [SerializeField, Range(0, 3)] private int difficulty = 1;

        public float Length => Mathf.Max(1f, length);
        public int Difficulty => difficulty;
        public float StartZ => transform.position.z;
        public float EndZ => StartZ + Length;

        public IObjectPool<TrackChunk> Pool { get; set; }

        public void Release()
        {
            if (Pool != null) Pool.Release(this);
            else gameObject.SetActive(false);
        }
    }
}