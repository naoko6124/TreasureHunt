using UnityEngine;

namespace _Framework.VoxelMap
{
    public class VoxelWorld : MonoBehaviour
    {
        [Header("Chunk")]
        public GameObject chunk;
        
        [Header("Size (in chunks")]
        public int width;
        public int length;
        
        private void Start()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    VoxelChunk voxelChunk = Instantiate(chunk, new Vector3(i * 16, 0, j * 16), Quaternion.identity, transform).GetComponent<VoxelChunk>();
                    voxelChunk.GenerateTerrain();
                }
            }
        }
    }
}