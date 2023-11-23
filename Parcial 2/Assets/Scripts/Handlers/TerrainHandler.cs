using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    [Header("Terrain Data")]
    [SerializeField] private GameObject terrainPrefab = default;
    [SerializeField] private Vector2Int terrainCount = Vector2Int.zero;
    public Vector2Int TerrainCount => terrainCount;
    [SerializeField] private Vector3 eachTerrainSize = Vector3.one;

    public void CreateTerrain(Transform parent)
    {
        for(int x = 0; x < terrainCount.x; x++)
        {
            for(int y = 0; y < terrainCount.y; y++)
            {
                Vector3 position = Vector3.zero;
                position.x = eachTerrainSize.x * x;
                position.y = eachTerrainSize.y * y;
                Instantiate(terrainPrefab, position, Quaternion.identity, parent);
            }
        }
    }
}
