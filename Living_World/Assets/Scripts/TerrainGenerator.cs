using System.Collections.Generic;
using UnityEngine;
//TODO diff between count and counter
/// <summary>
/// Creates a procedurally generated world.
/// </summary>
public class TerrainGenerator : MonoBehaviour {

    public int xSize = 10;
    public int zSize = 10;

    public GameObject tree;
    public GameObject grass;
    public GameObject house;
    
    /* 
    Variables used for creating the terrain with perlinNoise.
    scale: Determines the roughness of the terrain. Low values will result in a smooth surface.
    heightScale: Determines the max height of the terrain.
    */
    public float scale = 0.05f;
    public float heightScale = 5f;

    private void Start() {
        
        Utility.SetSeed(88055);
 
        GenerateTerrain();

        CreateCities();

        // AddVegetation();

        // Debug.Break();
    }

    // TODO Unity only allows 65,534 vertices per mesh. This means the max allowed area is 255 * 255.
    // Make multiple meshes when a larger area is requested.
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

        AddVegetationObjects(tree, vegetation, 5, vegetationMap);
        AddVegetationObjects(grass, vegetation, 10, vegetationMap);              
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
                        Debug.LogWarning($"Placement of {obj} went wrong. No room found!");
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
        if (GetComponent<MeshCollider>().Raycast(ray, out RaycastHit hit, maxHeight * 2)) {
            return hit.point.y;
        }
        else {
            Debug.DrawRay(new Vector3(x,maxHeight,z), Vector3.down * maxHeight, Color.red, maxHeight * 2);
            Debug.LogError($"{x}, {z} is not a valid point.");
            return 0f;
        }
    }

    public List<Node> nodes = new List<Node>();
    private void CreateCities(){
        Debug.Log("Creating city");

        // TODO when a object has to take a route which is n times longer than 
        // as the crow flies, connect them.
        
        CreateNodes();

        ConnectNodes();

        ConnectClusters();

        AddHouses();
    }

    /// <summary>
    /// Creates a set of nodes on the world. These node represent all intersections or corners.
    /// </summary>
    private void CreateNodes(){
        int nodeCount = 30;
        
        // Add initial random node
        float xCor = Random.Range(0f, xSize);
        float zCor = Random.Range(0f, zSize);
        float yCor = GetHeightTerrain(xCor, zCor);
        nodes.Add(new Node(new Vector3(xCor, yCor, zCor)));    

        for(int i = 0; i < nodeCount; i++){

            // If a node failed to be placed
            if(!createNode()){
                break;
            };
        }
    }

    /// <summary>
    /// Creates a node a random distance away from existing nodes.
    /// </summary>
    /// <returns>bool: true when the node is placed</returns>
    private bool createNode(){

        int while_counter = 0;
        while(true){

            // Get a random node
            int randomNodeIndex = Random.Range(0, nodes.Count);
            Node randomNode = nodes[randomNodeIndex];

            // Get random displacment
            float theta = Random.Range(0f, 360);
            float radius = Random.Range(15f, 100);

            // Convert polar coordinates to cartesion
            float xCor = randomNode.position.x + Mathf.Cos(theta) * radius;
            float zCor = randomNode.position.z + Mathf.Sin(theta) * radius;

            // Check if coordinates fall in bound
            if(!CheckInBounds(xCor, zCor)){
                continue;
            }

            float yCor = GetHeightTerrain(xCor, zCor);
            Vector3 pos =  new Vector3(xCor, yCor, zCor);

            // Check if the node is at least a n meters away from all other nodes
            bool valid = true;
            foreach(Node node in nodes){
                if (Utility.dist(node.position, pos) < 15){
                    valid = false;
                    break;
                }
            }

            if (valid){
                nodes.Add(new Node(pos));
                return true;
            }

            if(while_counter == 1000){
                Debug.LogWarning("No space found for node");
                return false;
            }

            while_counter += 1;
        }
    }

    /// <summary>
    /// Adds connections between the nodes. 
    /// This is achieved by connecting the node closest to another node. Excluding connections which have previously been made.
    /// </summary>
    private void ConnectNodes(){

        foreach(Node allNode in nodes){
            
            float minDist = Mathf.Infinity;
            Node closestNode = null;

            foreach(Node node2 in nodes){

                // Skip if the two noded are the same
                if(allNode == node2){
                    continue;
                }

                // // Skip when the connection already exists
                // if (node2.connections.Contains(allNode)){
                //     continue;
                // }

                float dist = Utility.dist(allNode.position, node2.position);
                if(dist < minDist){
                    minDist = dist;
                    closestNode = node2;
                }
            }

            allNode.connections.Add(closestNode);
            closestNode.connections.Add(allNode);
        }
    }

    /// <summary>
    /// Connects clusters
    /// </summary>
    private void ConnectClusters(){

        int while_count = 0;
            
        // Get all Nodes connected to the first node
        List<Node> clusterNodes = new List<Node>(){nodes[0]};
        Queue<Node> queueNodes = new Queue<Node>();
        queueNodes.Enqueue(nodes[0]);
        while(true){

            // Get all nodes connected to the nodes in the queue
            while(queueNodes.Count > 0){

                Node node = queueNodes.Peek();

                foreach(Node connectedNode in node.connections){
                    if(!clusterNodes.Contains(connectedNode)){
                        queueNodes.Enqueue(connectedNode);
                        clusterNodes.Add(connectedNode);
                    }
                }
                queueNodes.Dequeue();
            }
        
        
            // Check if the cluster is as big as all nodes
            if(clusterNodes.Count == nodes.Count){
                break;
            }

            // Create smallest connection from a cluster node to a non cluster node
            float minDist = Mathf.Infinity;
            Node closestNodeCluster = null;
            Node closestNodeAll = null;
            foreach(Node clusterNode in clusterNodes){
                foreach(Node allNode in nodes){
                    if(clusterNodes.Contains(allNode)){
                        continue;
                    }

                    float dist = Utility.dist(clusterNode.position, allNode.position);
                    if(dist < minDist){
                        minDist = dist;
                        closestNodeCluster = clusterNode;
                        closestNodeAll = allNode;
                    }
                }
            }

            closestNodeCluster.connections.Add(closestNodeAll);
            closestNodeAll.connections.Add(closestNodeCluster);

            queueNodes.Enqueue(closestNodeAll);

            while_count += 1;

            if(while_count == 1000){
                Debug.Log("I've done fucked up");
                break;
            }
        }

    }

    private void AddHouses(){

        // Creates a folder object for all instantiated objects
        GameObject folder = new GameObject();
        folder.transform.parent = gameObject.transform;
        folder.name = house.name + "s";

        List<Node> nodesCovered = new List<Node>();
        for(int i = 0; i < nodes.Count; i++){
            foreach(Node node in nodes[i].connections){

                if(nodesCovered.Contains(node)){
                    continue;
                }

                placeHouses(nodes[i].position, node.position, folder);


            }
            nodesCovered.Add(nodes[i]);

        }
    }

    private void placeHouses(Vector3 start, Vector3 end, GameObject folder){
        // Places houses adjecent to the line from start to end

        Vector3 dir = start - end;

        Vector3 normal = Vector3.Cross(dir, Vector3.up).normalized;

        int houseWidth = 6;

        float timeStep = houseWidth / dir.magnitude;

        for(float t = 0; t < 1; t += timeStep){
            int[] signs = new int[]{1,-1};
            foreach(int sign in signs){

                int roadWidth = 2;
                Vector3 pos = Vector3.Lerp(start, end, t) + normal * sign * (roadWidth + houseWidth / 2);
                pos.y = GetHeightTerrain(pos.x, pos.z);
                GameObject newHouse = Instantiate(house, pos, Quaternion.identity, folder.transform);

                // Check for collision with road

                // Check for collsion with other houses

                // Rotate houses acoording to the direction of road
                
            }
        }
    }

    private void OnDrawGizmos() {
        if(nodes == null){
            return;
        }

        Gizmos.color = Color.red;
        foreach(Node node in nodes){
            Gizmos.DrawSphere(node.position, 5f);

            foreach(Node tempNode in node.connections){
                Gizmos.DrawLine(node.position, tempNode.position);
            }
        }

    }
    
    /// <summary>
    /// Checks if a given x and z coordinate are within the world boundaries
    /// </summary>
    /// <param name="x">float: x-coordinate</param>
    /// <param name="z">float z-coordinate</param>
    /// <returns>bool: true when the x and z coordinate are within the boundaries</returns>
    private bool CheckInBounds(float x, float z){

        if (x >= 0 && x < xSize && z >= 0 && z < zSize) {
            return true;
        }
        else {
            return false;
        }
    }
}

public class Node {
    public Vector3 position;
    public List<Node> connections;

    public Node(Vector3 _position){
        position = _position;
        connections = new List<Node>();
    }
}