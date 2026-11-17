using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Game.Core;

namespace Game.Gameplay
{
    public class TrackSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform chunkParent;
        [SerializeField] private TrackChunk[] chunkPrefabs;

        [Header("Streaming")]
        [SerializeField] private float spawnAheadDistance = 120f;
        [SerializeField] private float despawnBehindDistance = 30f;
        [SerializeField] private float startOffset = 15f;      // first chunk starts this far behind the player
        [SerializeField] private int safeChunksAtStart = 2;    // empty chunks at the start of a run

        [Header("Difficulty")]
        [SerializeField] private float metersPerDifficultyStep = 250f;
        [SerializeField] private int maxDifficulty = 3;

        [Header("Pooling")]
        [SerializeField] private int prewarmPerPrefab = 3;
        [SerializeField] private int maxPoolSize = 10;

        private ObjectPool<TrackChunk>[] pools;
        private readonly Queue<TrackChunk> active = new Queue<TrackChunk>();
        private float startZ;
        private float nextSpawnZ;
        private int spawnedCount;
        private int lastIndex = -1;

        private void Start()
        {
            if (player == null || chunkPrefabs == null || chunkPrefabs.Length == 0)
            {
                Debug.LogError("TrackSpawner: assign Player and at least one chunk prefab.", this);
                enabled = false;
                return;
            }

            startZ = player.position.z;
            BuildPools();
            GameStateManager.Instance.StateChanged += OnStateChanged;
            ResetTrack();
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState from, GameState to)
        {
            // New run (not a resume from pause): rebuild the track from the start
            if (to == GameState.Playing && from != GameState.Paused)
                ResetTrack();
        }

        private void Update()
        {
            if (GameStateManager.Instance.Current != GameState.Playing) return;

            FillAhead(player.position.z);
            RecycleBehind(player.position.z);
        }

        private void BuildPools()
        {
            pools = new ObjectPool<TrackChunk>[chunkPrefabs.Length];

            for (int i = 0; i < chunkPrefabs.Length; i++)
            {
                TrackChunk prefab = chunkPrefabs[i];
                ObjectPool<TrackChunk> pool = null;

                pool = new ObjectPool<TrackChunk>(
                    createFunc: () =>
                    {
                        TrackChunk chunk = Instantiate(prefab, chunkParent);
                        chunk.Pool = pool;
                        return chunk;
                    },
                    actionOnGet: chunk => chunk.gameObject.SetActive(true),
                    actionOnRelease: chunk => chunk.gameObject.SetActive(false),
                    actionOnDestroy: chunk => { if (chunk != null) Destroy(chunk.gameObject); },
                    collectionCheck: true,
                    defaultCapacity: prewarmPerPrefab + 1,
                    maxSize: maxPoolSize);

                pools[i] = pool;
                Prewarm(pool, prewarmPerPrefab);
            }
        }

        // Instantiate everything up front (at load), so nothing is created during the run
        private static void Prewarm(ObjectPool<TrackChunk> pool, int count)
        {
            var temp = new List<TrackChunk>(count);
            for (int i = 0; i < count; i++) temp.Add(pool.Get());
            for (int i = 0; i < temp.Count; i++) pool.Release(temp[i]);
        }

        private void ResetTrack()
        {
            while (active.Count > 0) active.Dequeue().Release();

            nextSpawnZ = startZ - startOffset;
            spawnedCount = 0;
            lastIndex = -1;

            // Use the start position, not the player's current one: the player may not be teleported yet
            FillAhead(startZ);
        }

        private void FillAhead(float playerZ)
        {
            while (nextSpawnZ < playerZ + spawnAheadDistance)
                SpawnNext();
        }

        private void RecycleBehind(float playerZ)
        {
            while (active.Count > 0 && active.Peek().EndZ < playerZ - despawnBehindDistance)
                active.Dequeue().Release();
        }

        private void SpawnNext()
        {
            int index = PickPrefabIndex();
            TrackChunk chunk = pools[index].Get();

            chunk.transform.SetPositionAndRotation(new Vector3(0f, 0f, nextSpawnZ), Quaternion.identity);
            nextSpawnZ += chunk.Length;

            active.Enqueue(chunk);
            spawnedCount++;
            lastIndex = index;
        }

        private int CurrentMaxDifficulty()
        {
            if (spawnedCount < safeChunksAtStart) return 0;

            float distance = Mathf.Max(0f, nextSpawnZ - startZ);
            return Mathf.Min(maxDifficulty, 1 + Mathf.FloorToInt(distance / metersPerDifficultyStep));
        }

        // Random pick among chunks not harder than the current level; never the same chunk twice in a row.
        // Written without lists or LINQ so it creates no garbage.
        private int PickPrefabIndex()
        {
            int max = CurrentMaxDifficulty();
            int exclude = lastIndex;

            int count = CountEligible(max, exclude);
            if (count == 0)
            {
                exclude = -1; // only one eligible chunk: allow a repeat
                count = CountEligible(max, exclude);
            }
            if (count == 0) return 0; // nothing matches the difficulty: fall back to the first prefab

            int pick = Random.Range(0, count);
            for (int i = 0; i < chunkPrefabs.Length; i++)
            {
                if (i == exclude || chunkPrefabs[i].Difficulty > max) continue;
                if (pick-- == 0) return i;
            }
            return 0;
        }

        private int CountEligible(int max, int exclude)
        {
            int n = 0;
            for (int i = 0; i < chunkPrefabs.Length; i++)
                if (i != exclude && chunkPrefabs[i].Difficulty <= max) n++;
            return n;
        }
    }
}