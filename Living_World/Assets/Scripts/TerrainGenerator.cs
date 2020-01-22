using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a procedurally generated world.
/// </summary>
public class TerrainGenerator : MonoBehaviour {

    public int xSize = 10;
    public int zSize = 10;

    public GameObject tree;
    public GameObject grass;

    /* 
    Variables used for creating the terrain with perlinNoise.
    scale: Determines the roughness of the terrain. Low values will result in a smooth surface.
    heightScale: Determines the max height of the terrain.
    */
    public float scale = 0.05f;
    public float heightScale = 5f;

    private void Start() {

        Utility.SetSeed(12294);
 
        GenerateTerrain();

        AddVegetation();
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

        // Create triangle meshes associated with the vertices
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

        // Update MeshCollider to new mesh
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void AddVegetation() {

        Debug.Log("Adding vegetation");

        // This map stores the density of vegetation
        float[,] vegetationMap = new float[xSize / 10, zSize / 10];

        // Get a random vegetation map. In the future this will depend on objects in the vicinity.
        for (int i = 0; i < vegetationMap.GetLength(0); i++) {
            for (int j = 0; j < vegetationMap.GetLength(1); j++) {
                vegetationMap[i, j] = Random.Range(0f, 1f);
            }
        }

        for (int i = 0; i < vegetationMap.GetLength(0); i++) {
            for (int j = 0; j < vegetationMap.GetLength(1); j++) {
                
                List<GameObject> trees = new List<GameObject>();
                // Add trees
                for (int k = 0; k < 10; k++) {

                    float r = Random.Range(0f, 1f);
                    if (r > vegetationMap[i, j]) {
                        continue;
                    }

                    // float xCor = Random.Range(i * 10f, (i + 1) * 10f);
                    // float zCor = Random.Range(j * 10f, (j + 1) * 10f);
                    // float yCor = GetHeightTerrain(xCor, zCor);
                    // Instantiate(tree, new Vector3(xCor, yCor, zCor), Quaternion.identity);

                    int while_count = 0;
                    while(while_count < 100){
                        float xCor = Random.Range(i * 10f, (i + 1) * 10f);
                        float zCor = Random.Range(j * 10f, (j + 1) * 10f);
                        float yCor = GetHeightTerrain(xCor, zCor);

                        bool valid = true;
                        foreach(GameObject treeObj in trees){

                            // TODO dist 1 should depend on the tree, it shouldn't be a magic number
                            if (Utility.dist(treeObj.transform.position, new Vector3(xCor, yCor, zCor)) < 1){
                                valid = false;
                                Debug.Log("Not Valid");
                                Debug.Log($"{xCor}, {zCor}");
                                Debug.Log($"{treeObj.transform.position}");
                            }
                        }
                        if (valid){
                            GameObject newTree = Instantiate(tree, new Vector3(xCor, yCor, zCor), Quaternion.identity);
                            trees.Add(newTree);
                            break;

                        }
                        while_count += 1;
                    }
                    
                }
                
                // Add grass
                for( int k = 0; k < 25; k++) {

                    float r = Random.Range(0f, 1f);
                    if (r > vegetationMap[i, j]) {
                        continue;
                    }

                    float xCor = Random.Range(i * 10, (i + 1) * 10);
                    float zCor = Random.Range(j * 10, (j + 1) * 10);
                    float yCor = GetHeightTerrain(xCor, zCor);

                    Instantiate(grass, new Vector3(xCor, yCor, zCor), Quaternion.identity);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private float GetHeightTerrain(float x, float z){

        float maxHeight = 100f;
        Ray ray = new Ray(new Vector3(x,maxHeight,z), Vector3.down);
        if (GetComponent<MeshCollider>().Raycast(ray, out RaycastHit hit, maxHeight)) {
            return hit.point.y;
        }
        else {
            Debug.LogError($"{x}, {z} is not a valid point.");
            return 0f;
        }
    }
}
