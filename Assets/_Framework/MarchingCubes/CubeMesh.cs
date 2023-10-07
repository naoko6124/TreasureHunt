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

            GeneratedMesh.vertices = global::MarchingCubes.LookupTables.CubeVertices;
            GeneratedMesh.triangles = global::MarchingCubes.LookupTables.CubeConfigurations[cubeConfiguration];
        }
    }
}
