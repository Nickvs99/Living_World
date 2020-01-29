using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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
 
        // Utility.Profile(GenerateTerrain);
        GenerateTerrain();

        Utility.Profile(AddVegetation);
        // AddVegetation();

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
    /// Adds vegetation tot the terrain.
    /// </summary>
    private void AddVegetation() {

        Debug.Log("Adding vegetation");

        // Resolution of vegetationMap
        int resolution = 10;
        
        // This map stores the density of vegetation
        float[,] vegetationMap = new float[xSize / resolution, zSize / resolution];

        // Get a random vegetation map. In the future this will depend on objects in the vicinity.
        for (int i = 0; i < vegetationMap.GetLength(0); i++) {
            for (int j = 0; j < vegetationMap.GetLength(1); j++) {
                vegetationMap[i, j] = Random.Range(0f, 1f);
            }
        }

        // Create empty vegetation object for organizing objects
        GameObject vegetation = new GameObject();
        vegetation.transform.parent = gameObject.transform;
        vegetation.name = "Vegetation";

        Utility.Profile(AddVegetationObjects(tree, vegetation, 10, vegetationMap));
        AddVegetationObjects(grass, vegetation, 25, vegetationMap);
                
    }

    /// <summary>
    /// Adds objects at a random place on the map. The vegetationMap desribes the density of objects per "tile"
    /// </summary>
    /// <param name="obj">GameObject: Objects which are instantiated</param>
    /// <param name="parent">GameObject: Parent of the instantiated object</param>
    /// <param name="maxPopulation">int: The maximum population for each section of the vegetationmap</param>
    /// <param name="vegetationMap">float[,]: The vegetationMap, describes the vegetation density of the terrain.</param>
    private void AddVegetationObjects(GameObject obj, GameObject parent, int maxPopulation, float[,] vegetationMap){
        
        int resolution = xSize / vegetationMap.GetLength(0);

        // Creates a folder object for all instantiated objects
        GameObject folder = new GameObject();
        folder.transform.parent = parent.transform;
        folder.name = obj.name + "s";

        // All newly added objects
        List<GameObject> objects = new List<GameObject>();
        
        // Loop over all sections of the vegetationMap
        for(int i = 0; i < vegetationMap.GetLength(0); i++){
            for(int j = 0; j < vegetationMap.GetLength(1); j++){

                // Try to add all objects
                for (int k = 0; k < maxPopulation; k++) {
                    
                    // Check if it is allowed based on the vegetationMap
                    float r = Random.Range(0f, 1f);
                    if (r > vegetationMap[i, j]) {
                        continue;
                    }

                    int while_count = 0;
                    while(while_count < 100){

                        // Pick a random x and z coordinate within the section
                        float xCor = Random.Range((float)i * resolution, (i + 1) * resolution);
                        float zCor = Random.Range((float)j * resolution, (j + 1) * resolution);
                        float yCor = GetHeightTerrain(xCor, zCor);

                        // Check if there is enough room for the object
                        bool valid = true;
                        foreach(GameObject treeObj in objects){

                            // TODO dist 1 should depend on the obj, it shouldn't be a magic number
                            if (Utility.dist(treeObj.transform.position, new Vector3(xCor, yCor, zCor)) < 1){
                                valid = false;
                                break;

                            }
                        }
                        if (!valid){
                            while_count += 1;
                            continue;
                        }

                        GameObject newObj = Instantiate(obj, new Vector3(xCor, yCor, zCor), Quaternion.identity);
                        newObj.transform.parent = folder.transform;
                        newObj.name = obj.name + "_" + objects.Count;
                        objects.Add(newObj);
                        break;       
                    }

                    if (while_count == 100){
                        Debug.LogWarning("Placement of {obj} went wrong. No room found!");
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get the height of the terrain at a x and z coordinate.
    /// </summary>
    /// <param name="x">float: x coordinate</param>
    /// <param name="z">float: z coordinate</param>
    /// <returns></returns>
    public float GetHeightTerrain(float x, float z){
        
        // TODO shouldm't be a magic number, now the terrain would be limited to 100 meters
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
