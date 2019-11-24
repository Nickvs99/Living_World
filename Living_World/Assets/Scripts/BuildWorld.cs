using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a procedurally generated world.
/// </summary>
public class BuildWorld : MonoBehaviour
{
    private GameObject terrainObj;
    private TerrainData terrainData;
    private Terrain terrainMain;

    void Start()
    {
        CreateTerrain();
    }

    /// <summary>
    /// Creates the terrain for the world.
    /// </summary>
    private void CreateTerrain() 
    {
        Debug.Log("Create terrain");

        terrainData = new TerrainData();

        float maxWidth = 100;
        float maxLength = 150;
        float maxHeight = 25;

        terrainData.size = new Vector3(maxWidth, maxHeight, maxLength);

        terrainObj = Terrain.CreateTerrainGameObject(terrainData);
        terrainMain = terrainObj.GetComponent<Terrain>();

        int heightmapWidth = terrainData.heightmapWidth;
        int heightmapHeight = terrainData.heightmapHeight;
        
        float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        // Raise terrain to allow for holes, rivers etc.
        for (int i = 0; i < heightmapWidth; i++) {
            for (int j = 0; j<  heightmapHeight; j++) {
                heights[i, j] = 5 / maxHeight;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
}
