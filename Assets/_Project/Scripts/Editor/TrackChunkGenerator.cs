using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Gameplay;

namespace Game.EditorTools
{
    public static class TrackChunkGenerator
    {
        private const float ChunkLength = 30f;
        private const float TrackWidth = 10f;
        private const float LaneDistance = 3f;
        private const string PrefabFolder = "Assets/_Project/Prefabs/Chunks";
        private const string MaterialFolder = "Assets/_Project/Art/Materials";

        private struct Layout
        {
            public string Name;
            public int Difficulty;
            public Vector2[] Obstacles; // x = lane (-1, 0, 1), y = z position inside the chunk
        }

        private static Layout L(string name, int difficulty, params Vector2[] obstacles)
            => new Layout { Name = name, Difficulty = difficulty, Obstacles = obstacles };

        [MenuItem("Tools/Track/Create Sample Chunks")]
        public static void CreateSampleChunks()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(MaterialFolder);

            Material floorMat = GetMaterial("Track_Floor", new Color(0.25f, 0.27f, 0.30f));
            Material obstacleMat = GetMaterial("Track_Obstacle", new Color(0.9f, 0.3f, 0.2f));

            // Every row of obstacles leaves at least one free lane
            Layout[] layouts =
            {
                L("Chunk_Empty", 0),
                L("Chunk_A", 1, new Vector2(0, 15)),
                L("Chunk_B", 1, new Vector2(-1, 10), new Vector2(1, 22)),
                L("Chunk_C", 2, new Vector2(-1, 8), new Vector2(0, 8), new Vector2(1, 22)),
                L("Chunk_D", 2, new Vector2(0, 8), new Vector2(1, 8), new Vector2(-1, 20), new Vector2(0, 20)),
                L("Chunk_E", 3, new Vector2(-1, 6), new Vector2(0, 12), new Vector2(1, 18),
                               new Vector2(-1, 24), new Vector2(0, 24)),
            };

            foreach (Layout layout in layouts)
                BuildChunk(layout, floorMat, obstacleMat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created {layouts.Length} sample chunks in {PrefabFolder}");
        }

        private static void BuildChunk(Layout layout, Material floorMat, Material obstacleMat)
        {
            var root = new GameObject(layout.Name);

            var chunk = root.AddComponent<TrackChunk>();
            var so = new SerializedObject(chunk);
            so.FindProperty("length").floatValue = ChunkLength;
            so.FindProperty("difficulty").intValue = layout.Difficulty;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Chunk origin is its start edge: the floor spans z = 0 .. ChunkLength
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.1f, ChunkLength / 2f);
            floor.transform.localScale = new Vector3(TrackWidth, 0.2f, ChunkLength);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            foreach (Vector2 o in layout.Obstacles)
            {
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = "Obstacle";
                obstacle.transform.SetParent(root.transform, false);
                obstacle.transform.localPosition = new Vector3(o.x * LaneDistance, 0.5f, o.y);
                obstacle.transform.localScale = new Vector3(2f, 1f, 1f);
                obstacle.GetComponent<Renderer>().sharedMaterial = obstacleMat;
                // Trigger so the CharacterController passes through; hit logic comes in 2.1.6 / 2.1.8
                obstacle.GetComponent<BoxCollider>().isTrigger = true;
            }

            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{layout.Name}.prefab");
            Object.DestroyImmediate(root);
        }

        private static Material GetMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}