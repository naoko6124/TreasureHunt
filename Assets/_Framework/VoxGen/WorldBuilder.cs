using UnityEditor;
using UnityEngine;

namespace _Framework.VoxGen
{
    public class WorldBuilder : MonoBehaviour
    {
        [Header("Chunk")]
        public int width;
        [SerializeField] private GameObject chunk;
        [SerializeField] private LayerMask groundLayer;
        [Header("Dig")]
        public float digPotency;
        private ChunkLoader[,] chunks;
        private Camera _camera;

        void Awake()
        {
            chunks = new ChunkLoader[width, width];
            _camera = Camera.main;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    ChunkLoader chunkLoader = Instantiate(chunk, new Vector3(x * 15, 0, y * 15), Quaternion.identity, transform).GetComponent<ChunkLoader>();
                    chunkLoader.gameObject.name = "Chunk " + x + "," + y;
                    chunkLoader.FillVoxels(x, y);
                    chunkLoader.GenerateMesh();
                    chunks[x, y] = chunkLoader;
                }
            }
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 2.0f, groundLayer))
                {
                    var cameraLook = _camera.transform.forward;
                    var x = cameraLook.x > 0 ? Mathf.RoundToInt(hit.point.x - transform.position.x + 0.01f) : Mathf.RoundToInt(hit.point.x - transform.position.x - 0.01f);
                    var y = cameraLook.y > 0 ? Mathf.RoundToInt(hit.point.y - transform.position.y + 0.01f) : Mathf.RoundToInt(hit.point.y - transform.position.y - 0.01f);
                    var z = cameraLook.z > 0 ? Mathf.RoundToInt(hit.point.z - transform.position.z + 0.01f) : Mathf.RoundToInt(hit.point.z - transform.position.z - 0.01f);
                    Debug.Log($"Original XYZ: {hit.point.x}, {hit.point.y}, {hit.point.z}");
                    RemovePoint(x, y, z, digPotency * Time.deltaTime);
                }
            }
        }

        public float GetPoint(int x, int y, int z)
        {
            x = Mathf.Clamp(x, 0, width * 16);
            y = Mathf.Clamp(y, 0, 16);
            z = Mathf.Clamp(z, 0, width * 16);
            int chunkX = Mathf.Clamp(Mathf.FloorToInt(((float)x) / 15.0f), 0, width - 1);
            int chunkZ = Mathf.Clamp(Mathf.FloorToInt(((float)z) / 15.0f), 0, width - 1 );
            int localX = Mathf.Clamp(x - chunkX * 15, 0, 15);
            int localZ = Mathf.Clamp(z - chunkZ * 15, 0, 15);
            ChunkLoader ck = chunks[chunkX, chunkZ];
            return ck.Voxels[localZ, y, localX];
        }

        private void RemovePoint(int x, int y, int z, float removeAmount)
        {
            x = Mathf.Clamp(x, 0, width * 16);
            y = Mathf.Clamp(y, 0, 16);
            z = Mathf.Clamp(z, 0, width * 16);
            int chunkX = Mathf.FloorToInt(((float)x) / 15.0f);
            int chunkZ = Mathf.FloorToInt(((float)z) / 15.0f);
            int localX = Mathf.Clamp(x - chunkX * 15, 0, 15);
            int localZ = Mathf.Clamp(z - chunkZ * 15, 0, 15);
            Debug.Log($"World XYZ: {x}, {y}, {z} | Chunk {chunkX}, {chunkZ} | Local XYZ: {localX}, {y}, {localZ}");
            chunks[chunkX, chunkZ].Voxels[localZ, y, localX] = Mathf.Clamp(chunks[chunkX, chunkZ].Voxels[localZ, y, localX] - removeAmount, 0f, 1f);
            chunks[chunkX, chunkZ].GenerateMesh();
            if (localX == 15 && chunkX != width - 1)
            {
                chunks[chunkX + 1, chunkZ].Voxels[localZ, y, 0] = Mathf.Clamp(chunks[chunkX + 1, chunkZ].Voxels[localZ, y, 0] - removeAmount, 0f, 1f);
                chunks[chunkX + 1, chunkZ].GenerateMesh();
                if (localZ == 0 && chunkZ != 0)
                {
                    chunks[chunkX + 1, chunkZ - 1].Voxels[15, y, 0] = Mathf.Clamp(chunks[chunkX + 1, chunkZ - 1].Voxels[15, y, 0] - removeAmount, 0f, 1f);
                    chunks[chunkX + 1, chunkZ - 1].GenerateMesh();
                }
                if (localZ == 15 && chunkZ != width - 1)
                {
                    chunks[chunkX + 1, chunkZ + 1].Voxels[0, y, 0] = Mathf.Clamp(chunks[chunkX + 1, chunkZ + 1].Voxels[0, y, 0] - removeAmount, 0f, 1f);
                    chunks[chunkX + 1, chunkZ + 1].GenerateMesh();
                }
            }
            if (localX == 0 && chunkX != 0)
            {
                chunks[chunkX - 1, chunkZ].Voxels[localZ, y, 15] = Mathf.Clamp(chunks[chunkX - 1, chunkZ].Voxels[localZ, y, 15] - removeAmount, 0f, 1f);
                chunks[chunkX - 1, chunkZ].GenerateMesh();
                if (localZ == 0 && chunkZ != 0)
                {
                    chunks[chunkX - 1, chunkZ - 1].Voxels[15, y, 15] = Mathf.Clamp(chunks[chunkX - 1, chunkZ - 1].Voxels[15, y, 15] - removeAmount, 0f, 1f);
                    chunks[chunkX - 1, chunkZ - 1].GenerateMesh();
                }
                if (localZ == 15 && chunkZ != width - 1)
                {
                    chunks[chunkX - 1, chunkZ + 1].Voxels[0, y, 15] = Mathf.Clamp(chunks[chunkX - 1, chunkZ + 1].Voxels[0, y, 15] - removeAmount, 0f, 1f);
                    chunks[chunkX - 1, chunkZ + 1].GenerateMesh();
                }
            }
            if (localZ == 15 && chunkZ != width - 1)
            {
                chunks[chunkX, chunkZ + 1].Voxels[0, y, localX] = Mathf.Clamp(chunks[chunkX, chunkZ + 1].Voxels[0, y, localX] - removeAmount, 0f, 1f);
                chunks[chunkX, chunkZ + 1].GenerateMesh();
            }
            if (localZ == 0 && chunkZ != 0)
            {
                chunks[chunkX, chunkZ - 1].Voxels[15, y, localX] = Mathf.Clamp(chunks[chunkX, chunkZ - 1].Voxels[15, y, localX] - removeAmount, 0f, 1f);
                chunks[chunkX, chunkZ - 1].GenerateMesh();
            }
        }
    }
    // [CustomEditor(typeof(WorldBuilder))]
    // public class VoxelEditor : Editor
    // {
    //     public void OnSceneGUI()
    //     {
    //         var t = target as WorldBuilder;
    //         var tr = t.transform;
    //         var pos = tr.position;

    //         var colorWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    //         var colorRed = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    //         if (!Application.isPlaying) return;
    //         for (var i = 0; i <= t.width * 16; i++)
    //         {
    //             for (var j = 0; j <= 16; j++)
    //             {
    //                 for (var k = 0; k <= t.width * 16; k++)
    //                 {
    //                     Handles.color = t.Points[i, j, k] == 1.0f ? colorRed : colorWhite;
    //                     if (!Handles.Button(pos + new Vector3(i, j, k),
    //                             Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up),
    //                             0.1f, 0.1f, Handles.SphereHandleCap)) continue;
    //                 }
    //             }
    //         }
    //     }
    // }
}
