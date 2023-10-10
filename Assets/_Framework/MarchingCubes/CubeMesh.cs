using UnityEngine;

namespace _Framework.MarchingCubes
{
    public class CubeMesh
    {
        public Mesh GeneratedMesh { get; private set; }

        public void CreateMesh(int cubeConfiguration)
        {
            GeneratedMesh = new Mesh { name = "Cube Mesh" };
            
            GeneratedMesh.Clear();

            GeneratedMesh.vertices = LookupTables.CubeVertices;
            GeneratedMesh.uv = LookupTables.CubeUV;
            GeneratedMesh.triangles = LookupTables.CubeConfigurations[cubeConfiguration];
        }
    }
}
