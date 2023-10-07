using UnityEngine;
using UnityEditor;

namespace _Framework.VoxelMap
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class VoxelWorld : MonoBehaviour
    {
        [Header("Size")]
        public int Width;
        public int Height;
        public int Length;
        
        private bool[,,] _points;

        public bool[,,] Points
        {
            get => _points;
            set => _points = value;
        }

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private MarchingCubes.CubeMesh _cubeMesh;
    
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _cubeMesh = new MarchingCubes.CubeMesh();
        }
        
        private void Start()
        {
            _points = new bool[Width, Height, Length];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height - 2; j++)
                {
                    for (int k = 0; k < Length; k++)
                    {
                        _points[i, j, k] = true;
                    }
                }
            }

            RecalculateChunk();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    Vector3 cameraLook = Camera.main.transform.forward;
                    int x = cameraLook.x > 0 ? Mathf.RoundToInt(hit.point.x + 0.1f) : Mathf.RoundToInt(hit.point.x - 0.1f);
                    int y = cameraLook.y > 0 ? Mathf.RoundToInt(hit.point.y + 0.1f) : Mathf.RoundToInt(hit.point.y - 0.1f);
                    int z = cameraLook.z > 0 ? Mathf.RoundToInt(hit.point.z + 0.1f) : Mathf.RoundToInt(hit.point.z - 0.1f);
                    _points[x, y, z] = false;
                    Debug.Log($"{x}, {y}, {z}");
                    RecalculateChunk();
                }
            }
        }

        public void RecalculateChunk()
        {
            var combine = new CombineInstance[(Width - 1) * (Height - 1) * (Length - 1)];

            for (int i = 0; i < _points.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < _points.GetLength(1) - 1; j++)
                {
                    for (int k = 0; k < _points.GetLength(2) - 1; k++)
                    {
                        int cubeConfiguration = 0;
                        if (_points[i, j, k]) cubeConfiguration += 1;
                        if (_points[i + 1, j, k]) cubeConfiguration += 2;
                        if (_points[i, j + 1, k]) cubeConfiguration += 4;
                        if (_points[i + 1, j + 1, k]) cubeConfiguration += 8;
                        if (_points[i, j, k + 1]) cubeConfiguration += 16;
                        if (_points[i + 1, j, k + 1]) cubeConfiguration += 32;
                        if (_points[i, j + 1, k + 1]) cubeConfiguration += 64;
                        if (_points[i + 1, j + 1, k + 1]) cubeConfiguration += 128;
                        _cubeMesh.CreateMesh(cubeConfiguration);
                        combine[i * ((Height - 1) * (Length -1)) + j * (Length - 1) + k].mesh = _cubeMesh.GeneratedMesh;
                        Vector3 cubePosition = transform.position + new Vector3(0.5f + i, 0.5f + j, 0.5f + k);
                        combine[i * ((Height - 1) * (Length -1)) + j * (Length - 1) + k].transform = Matrix4x4.Translate(cubePosition);
                    }
                }
            }

            var mesh = new Mesh { name = "Voxel World" };
            mesh.CombineMeshes(combine);
            mesh.Optimize();
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }
    }
    
    [CustomEditor(typeof(VoxelWorld))]
    public class VoxelEditor : Editor
    {
        public void OnSceneGUI()
        {
            var t = target as VoxelWorld;
            var tr = t.transform;
            var pos = tr.position;
            var colorWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            var colorRed = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        
            if (Application.isPlaying)
            {
                for (int i = 0; i < t.Points.GetLength(0); i++)
                {
                    for (int j = 0; j < t.Points.GetLength(1); j++)
                    {
                        for (int k = 0; k < t.Points.GetLength(2); k++)
                        {
                            if (t.Points[i, j, k]) Handles.color = colorRed;
                            else Handles.color = colorWhite;
                            Handles.SphereHandleCap(1, pos + new Vector3(i, j, k), Quaternion.identity, 0.05f, EventType.Repaint);
                            if (Handles.Button(pos + new Vector3(i, j, k), 
                                    Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), 
                                    0.05f, 0.05f, Handles.SphereHandleCap))
                            {
                                t.Points[i, j, k] = !t.Points[i, j, k];
                                t.RecalculateChunk();
                            }
                        }
                    }
                }
            }
        }
    }/**/
}