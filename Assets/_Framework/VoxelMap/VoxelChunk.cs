using _Framework.VoxelMap.MarchingCubes;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

namespace _Framework.VoxelMap
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class VoxelChunk : MonoBehaviour
    {
        [Header("Size")]
        public int width;
        public int height;
        public int length;

        [Header("Fill")]
        public int fillHeight;

        public bool usePerlin;
        public float perlinScale;

        private bool[,,] _points;
        public bool[,,] Points
        {
            get => _points;
            set => _points = value;
        }

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private CubeMesh _cubeMesh;
        private CombineInstance[] _combineInstance;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _cubeMesh = new CubeMesh();
        }

        private void Start()
        {
            _points = new bool[width + 1, height + 1, length + 1];

            for (var i = 0; i < width + 1; i++)
            {
                for (var k = 0; k < length + 1; k++)
                {
                    if (usePerlin)
                    {
                        float noise = Mathf.PerlinNoise((float)i / (width * perlinScale),
                            (float)k / (length * perlinScale));
                        int perlinHeight = Mathf.RoundToInt(noise * height);
                        for (var j = 0; j < perlinHeight + 1; j++)
                        {
                            _points[i, j, k] = true;
                        }
                    }
                    else
                    {
                        for (var j = 0; j < fillHeight + 1; j++)
                        {
                            _points[i, j, k] = true;
                        }
                    }
                }
            }

            RecalculateChunk();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100.0f))
                {
                    var cameraLook = _camera.transform.forward;
                    var x = cameraLook.x > 0 ? Mathf.RoundToInt(hit.point.x - transform.position.x + 0.1f) : Mathf.RoundToInt(hit.point.x - transform.position.x - 0.1f);
                    var y = cameraLook.y > 0 ? Mathf.RoundToInt(hit.point.y - transform.position.y + 0.1f) : Mathf.RoundToInt(hit.point.y - transform.position.y - 0.1f);
                    var z = cameraLook.z > 0 ? Mathf.RoundToInt(hit.point.z - transform.position.z + 0.1f) : Mathf.RoundToInt(hit.point.z - transform.position.z - 0.1f);
                    _points[x, y, z] = false;
                    UpdatePoint(x, y, z);
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100.0f))
                {
                    var cameraLook = _camera.transform.forward;
                    var x = cameraLook.x <= 0 ? Mathf.RoundToInt(hit.point.x - transform.position.x + 0.1f) : Mathf.RoundToInt(hit.point.x - transform.position.x - 0.1f);
                    var y = cameraLook.y <= 0 ? Mathf.RoundToInt(hit.point.y - transform.position.y + 0.1f) : Mathf.RoundToInt(hit.point.y - transform.position.y - 0.1f);
                    var z = cameraLook.z <= 0 ? Mathf.RoundToInt(hit.point.z - transform.position.z + 0.1f) : Mathf.RoundToInt(hit.point.z - transform.position.z - 0.1f);
                    _points[x, y, z] = true;
                    UpdatePoint(x, y, z);
                }
            }
        }

        public void UpdatePoint(int x, int y, int z)
        {
            CalculateCube(x, y, z);
            CalculateCube(x - 1, y, z);
            CalculateCube(x, y - 1, z);
            CalculateCube(x, y, z - 1);
            CalculateCube(x - 1, y - 1, z);
            CalculateCube(x - 1, y, z - 1);
            CalculateCube(x, y - 1, z - 1);
            CalculateCube(x - 1, y - 1, z - 1);

            GenerateMesh();
        }

        private void RecalculateChunk()
        {
            _combineInstance = new CombineInstance[width * height * length];

            for (var i = 0; i < width; i++)
                for (var j = 0; j < height; j++)
                    for (var k = 0; k < length; k++)
                        CalculateCube(i, j, k);

            GenerateMesh();
        }

        private void CalculateCube(int i, int j, int k)
        {
            if (i < 0 || j < 0 || k < 0) return;
            if (i > width - 1 || j > height - 1 || k > length - 1) return;
            
            var cubePosition = new Vector3(0.5f + i, 0.5f + j, 0.5f + k);
            _combineInstance[i * height * length + j * length + k].transform = Matrix4x4.Translate(cubePosition);
            
            var cubeConfiguration = 0;
            if (_points[i, j, k]) cubeConfiguration += 1;
            if (_points[i + 1, j, k]) cubeConfiguration += 2;
            if (_points[i, j + 1, k]) cubeConfiguration += 4;
            if (_points[i + 1, j + 1, k]) cubeConfiguration += 8;
            if (_points[i, j, k + 1]) cubeConfiguration += 16;
            if (_points[i + 1, j, k + 1]) cubeConfiguration += 32;
            if (_points[i, j + 1, k + 1]) cubeConfiguration += 64;
            if (_points[i + 1, j + 1, k + 1]) cubeConfiguration += 128;

            if (cubeConfiguration == 0 || cubeConfiguration == 255)
                _cubeMesh.ClearMesh();
            else            
                _cubeMesh.CreateMesh(cubeConfiguration);
            _combineInstance[i * height * length + j * length + k].mesh = _cubeMesh.GeneratedMesh;
        }

        private void GenerateMesh()
        {
            var mesh = new Mesh { name = "Voxel World" };
            mesh.CombineMeshes(_combineInstance);
            mesh.Optimize();
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }

        private void OnDrawGizmos()
        {
            Vector3 wireSize = new Vector3(width, height, length);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + wireSize/2f, wireSize);
            if (Application.isPlaying) return;
            Vector3 boxSize = new Vector3(width, fillHeight + 0.5f, length);
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Gizmos.DrawCube(transform.position + boxSize/2f, boxSize);
        }
    }
    
    /*[CustomEditor(typeof(VoxelWorld))]
    public class VoxelEditor : Editor
    {
        public void OnSceneGUI()
        {
            var t = target as VoxelWorld;
            var tr = t.transform;
            var pos = tr.position;

            var colorWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            var colorRed = new Color(1.0f, 0.0f, 0.0f, 1.0f);

            if (!Application.isPlaying) return;
            for (var i = 0; i <= t.width; i++)
            {
                for (var j = 0; j <= t.height; j++)
                {
                    for (var k = 0; k <= t.length; k++)
                    {
                        Handles.color = t.Points[i, j, k] ? colorRed : colorWhite;
                        if (t.Points[i, j, k])
                        {
                            bool canPlace = true;
                            canPlace = i != t.width ? canPlace && t.Points[i + 1, j, k] : canPlace;
                            canPlace = j != t.height ? canPlace && t.Points[i, j + 1, k] : canPlace;
                            canPlace = k != t.length ? canPlace && t.Points[i, j, k + 1] : canPlace;
                            canPlace = i != 0 ? canPlace && t.Points[i - 1, j, k] : canPlace;
                            canPlace = j != 0 ? canPlace && t.Points[i, j - 1, k] : canPlace;
                            canPlace = k != 0 ? canPlace && t.Points[i, j, k - 1] : canPlace;
                            if (canPlace) continue;
                        }
                        else
                        {
                            bool canPlace = true;
                            canPlace = i != t.width ? canPlace && !t.Points[i + 1, j, k] : canPlace;
                            canPlace = j != t.height ? canPlace && !t.Points[i, j + 1, k] : canPlace;
                            canPlace = k != t.length ? canPlace && !t.Points[i, j, k + 1] : canPlace;
                            canPlace = i != 0 ? canPlace && !t.Points[i - 1, j, k] : canPlace;
                            canPlace = j != 0 ? canPlace && !t.Points[i, j - 1, k] : canPlace;
                            canPlace = k != 0 ? canPlace && !t.Points[i, j, k - 1] : canPlace;
                            if (canPlace) continue;
                        }

                        if (!Handles.Button(pos + new Vector3(i, j, k),
                                Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up),
                                0.1f, 0.1f, Handles.SphereHandleCap)) continue;
                        t.Points[i, j, k] = !t.Points[i, j, k];
                        t.UpdatePoint(i, j, k);
                    }
                }
            }
        }
    }*/
}