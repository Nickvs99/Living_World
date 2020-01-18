using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a procedurally generated world.
/// </summary>
public class TerrainGenerator : MonoBehaviour {

    public int xSize = 10;
    public int zSize = 10;

    // Variables used for creating the terrain with perlinNoise.
    // scale: Determines the roughness of the terrain. Low values will result in a smooth surface.
    // heightScale: Determines the max height of the terrain.
    public float scale = 0.05f;
    public float heightScale = 5f;

    private void Start() {

        SetSeed();
 
        GenerateTerrain();
    }

    /// <summary>
    /// Initializes the random number generator state with a randomseed.
    /// </summary>
    private void SetSeed() {
        SetSeed(Random.Range(0, 100000));
    }

    /// <summary>
    /// Initializes the random number generator state with the given seed.
    /// </summary>
    /// <param name="seed">int: seed</param>
    private void SetSeed(int seed) {

        Debug.Log($"Seed: {seed}");
        Random.InitState(seed);
    }

    /// <summary>
    /// Creates the terrain for the world.
    /// </summary>
    private void GenerateTerrain() {

        
        Debug.Log("Create terrain");

        Mesh mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "Procedural grid";

        // Initialize vertices
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        // Random offset used for the perlinnoise 
        float xOffset = Random.Range(0, 99999);
        float yOffset = Random.Range(0, 99999);

        // Create a all vertices for terrain
        int i = 0;
        for (int z = 0; z < zSize + 1; z++) {
            for (int x = 0; x < xSize + 1; x++) {
                
                // Set the height for a given x and z coordinate
                float xPerlin = x * scale + xOffset;
                float zPerlin = z * scale + yOffset;
                float y = Mathf.PerlinNoise(xPerlin, zPerlin) * heightScale;

                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        mesh.vertices = vertices;

        // Create the triangles associated with the vertices
        int[] triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {

                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;

            }
            vert++;
        }
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}
