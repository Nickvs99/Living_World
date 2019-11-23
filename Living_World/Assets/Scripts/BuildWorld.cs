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

        terrainObj = Terrain.CreateTerrainGameObject(terrainData);
        terrainMain = terrainObj.GetComponent<Terrain>();
    }
}
