using System.Collections;
using System.Collections.Generic;
using _Framework.VoxelMap;
using _Framework.VoxGen;
using Unity.VisualScripting;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [Header("Chest")]
    public int chestHeightMin;
    public int chestHeightMax;
    public int chestQuantity;
    [SerializeField] private GameObject chest;
    [Header("World")]
    [SerializeField] private WorldBuilder world;

    private List<Vector3> spawnablePoints = new List<Vector3>();

    void Start()
    {
        Invoke("SpawnChests", 0.5f);
    }

    private void SpawnChests()
    {
        spawnablePoints.Clear();
        for (int x = 0; x < world.width * 16; x++)
        {
            for (int y = chestHeightMin; y < chestHeightMax; y++)
            {
                for (int z = 0; z < world.width * 16; z++)
                {
                    bool spawnable = true;
                    spawnable &= world.GetPoint(x, y, z) == 1.0f;
                    if (x != world.width * 16 - 1) spawnable &= world.GetPoint(x + 1, y, z) == 1.0f; else spawnable = false;
                    if (x != 0) spawnable &= world.GetPoint(x - 1, y, z) == 1.0f; else spawnable = false;
                    if (y != 15) spawnable &= world.GetPoint(x, y + 1, z) == 1.0f; else spawnable = false;
                    if (y != 0) spawnable &= world.GetPoint(x, y - 1, z) == 1.0f; else spawnable = false;
                    if (z != world.width * 16 - 1) spawnable &= world.GetPoint(x, y, z + 1) == 1.0f; else spawnable = false;
                    if (z != 0) spawnable &= world.GetPoint(x, y, z - 1) == 1.0f; else spawnable = false;

                    if (spawnable)
                        spawnablePoints.Add(new Vector3(x, y, z));
                }
            }
        }

        for (int i = 0; i < chestQuantity; i++)
        {
            if (spawnablePoints.Count <= 1) break;
            int randomSpot = Random.Range(0, spawnablePoints.Count);
            Instantiate(chest, spawnablePoints[randomSpot], Quaternion.identity, transform);
            spawnablePoints.RemoveAt(randomSpot);
        }
    }
}
