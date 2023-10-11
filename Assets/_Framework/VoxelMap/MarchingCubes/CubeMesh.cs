using UnityEngine;

namespace _Framework.VoxelMap.MarchingCubes
{
    public class CubeMesh
    {
        public Mesh GeneratedMesh { get; private set; }

        public void ClearMesh()
        {
            GeneratedMesh = new Mesh { name = "Cube Mesh" };
            GeneratedMesh.Clear();
        }
        
        public void CreateMesh(int cubeConfiguration)
        {
            GeneratedMesh = new Mesh { name = "Cube Mesh" };
            GeneratedMesh.Clear();

            GeneratedMesh.vertices = LookupTables.CubeVertices;
            GeneratedMesh.uv = LookupTables.CubeUV;
            GeneratedMesh.triangles = LookupTables.CubeConfigurations[cubeConfiguration];
            
            GeneratedMesh.Optimize();
            GeneratedMesh.RecalculateNormals();
        }
    }
}
