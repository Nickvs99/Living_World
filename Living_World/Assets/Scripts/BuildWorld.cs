using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildWorld : MonoBehaviour {
    // Builds a procedural generated world

    // The different GameObjects each tile can have
    public GameObject Grass, Water, Bridge, House, Tree, Asphalt_default, Bridge_main;

    public GameObject Camera, gridPiece, cityEmpty, gridHier, bridgeHier;

    // The size of the world
    public static int sizeX, sizeZ;
    public int _sizeX, _sizeZ;

    // The number of cities allowed, if it's less than 0 fill the grid with as many cities as possible
    public int cityCount;

    // A 2D array with the GameObjects
    public static GameObject[,] gridObjects;
    public List<GameObject> cities = new List<GameObject>();

    // The number of points to represent the river
    private int numPoints;
    public List<Vector3> River0 = new List<Vector3>();
    public List<Vector3> River1 = new List<Vector3>();

    private int surfaceArea;

    // The percentage of asphalt needed for when a city is considered finished.
    public float asphaltPerc = 0.1f;

    // Used for organising the GameObjects in the hierarchy
    private GameObject[] rows;

    private int buildState = 0;

    void Start() {
        // Initialize world

        Utility.Print("start");

        // Create a random seed. Use this seed to recreate the same world. Used for debuging
        int seed = Random.Range(0, 100000);
        Utility.Print($"Seed: {seed}");
        Random.InitState(seed);

        // assigns the inspector values to 
        sizeX = _sizeX;
        sizeZ = _sizeZ;

        if (sizeX < 20) {
            Debug.LogWarning("Size x has to be greater or equel to 20 for the best result");
        }
        if (sizeZ < 20) {
            Debug.LogWarning("Size z has to be greater or equel to 20 for the best result");
        }

        if (cityCount < 0) {
            cityCount = int.MaxValue;
        }

        surfaceArea = sizeX * sizeZ;

        Camera.GetComponent<MoveCamera>().Initialize();

        gridObjects = new GameObject[sizeX, sizeZ];

        // Used to organize the object in the hierarchy
        rows = new GameObject[sizeX];

        // Number of points used to represent the river
        numPoints = (int)Mathf.Sqrt(surfaceArea) * 4;

    }

    void Update() {

        // All the functions to create the world, if the conditions in the functions are met, increment the buildState by one to continue the building process
        System.Action[] actions = new System.Action[] { CreateEmpty, CreateGrass, CreateRiver, CreateCities, CreateHighway, PlaceIntersect, PlaceHouses, PlaceTrees, StartLiving };
        actions[buildState]();
    }

    void MoveCamera() {
        // Moves the camera to a somewhat right spot, will have to make this more accurate

        float height;
        if (sizeX > sizeZ) {
            height = sizeX;
        }
        else {
            height = sizeZ;
        }
        Camera.transform.position = new Vector3(sizeX / 2, height, sizeZ / 2);
    }

    void CreateEmpty() {
        // Creates empty GameObjects as a building block for the upcoming states

        Debug.Log("Start Building");
        for (int i = 0; i < sizeX; i++) {

            // Creates rows for organisation
            GameObject row = new GameObject();
            row.transform.parent = gridHier.transform;
            row.transform.position = Utility.CalcPosition(i, 0);
            row.name = "Row " + i;
            rows[i] = row;

            for (int j = 0; j < sizeZ; j++) {

                //Initialize the empty object
                GameObject tempPiece = (GameObject)Instantiate(gridPiece);
                tempPiece.transform.parent = row.transform;
                tempPiece.transform.position = Utility.CalcPosition(i, j);
                tempPiece.name = "Grid " + i + ", " + j;

                gridObjects[i, j] = tempPiece;
                gridObjects[i, j].GetComponent<General>().xCor = i;
                gridObjects[i, j].GetComponent<General>().zCor = j;
            }
        }

        buildState += 1;
    }

    void CreateGrass() {
        // Places grass at every spot

        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeZ; j++) {

                gridObjects[i, j].GetComponent<General>().AddPiece(Grass);
                gridObjects[i, j].tag = "Grass";
            }
        }

        buildState += 1;
    }

    void CreateRiver() {
        // Creates a river within the world. This river follows a beziercurve, which does not cross itself.

        //TODO Make the riverWidth variable
        float riverWidth = 2;

        // Calculate initial points
        Vector3[] pBoundaries = PointOnBoundaries();
        Vector3 p0 = pBoundaries[0];
        Vector3 p1 = PointWithinBoundary();
        Vector3 p3 = pBoundaries[1];

        Vector3 p2 = CalcP2(p0, p1, p3);

        Vector3[] coordinatesBezier = createBezierPoints(p0, p1, p2, p3);

        // TODO when the riverWidht < 2, the same two rivers are created. This would cause boats to "drive" into eachother.
        if (riverWidth < 2) {
            River0 = new List<Vector3>(coordinatesBezier);
            River1 = new List<Vector3>(coordinatesBezier);
        }

        for (int i = 0; i < coordinatesBezier.Length - 2; i++) {
            for (float j = -riverWidth; j <= riverWidth; j += 0.5f) {

                Vector3 dif = coordinatesBezier[i + 1] - coordinatesBezier[i];

                // Normal Vector on diff
                float x4 = -dif[2] / dif[0];
                Vector3 normalVec = new Vector3(x4, 0, 1);

                // Normalizes vector
                float mag = Vector3.Magnitude(normalVec);
                Vector3 normVec = normalVec / mag;

                // Flip orientation from normal when the beziercurve curves "moves to the right".
                if (coordinatesBezier[i][0] < coordinatesBezier[i + 1][0]) {
                    normVec *= -1;
                }

                // Add normalized vector to beziercurve to give the curve a width
                Vector3 addedVec = coordinatesBezier[i] + j * normVec;

                int xCor = (int)addedVec[0];
                int zCor = (int)addedVec[2];

                if (Utility.CheckWithinBounds(xCor, zCor)) {
                    if (gridObjects[xCor, zCor].tag != "Water") {
                        // Remove all info about previous states and start with the wate state.

                        gridObjects[xCor, zCor].GetComponent<General>().RemovePiece("all");

                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Water);
                        gridObjects[xCor, zCor].tag = "Water";
                    }
                }

                if (riverWidth >= 2) {
                    if (j == -1f) {
                        River0.Add(addedVec);

                    }
                    else if (j == 1f) {
                        River1.Add(addedVec);

                    }
                }
            }
        }

        River1.Reverse();

        buildState += 1;
    }

    Vector3 CalcP2(Vector3 p0, Vector3 p1, Vector3 p3) {
        // Calculates p2 based on the others. This ensure that the beziercurve does not 
        // cross itself.

        Vector3 p2 = new Vector3();
        int while_count = 0;
        while (while_count < 500) {
            p2 = PointWithinBoundary();

            // Calculates the linear coefficients between two points
            float[] p0_1Coefficients = LinearCoefficients(p0, p1);
            float[] p2_3Coefficients = LinearCoefficients(p2, p3);

            // Calculates where these two lines intersect
            float xInter = xIntersect(p0_1Coefficients, p2_3Coefficients);

            // When the x-intersection falls between p0.x and p1.x AND p2.x and p3.x, return p2
            if (!(((p0[0] <= xInter && xInter <= p1[0]) || (p1[0] <= xInter && xInter <= p0[0])) &&
                ((p2[0] <= xInter && xInter <= p3[0]) || (p3[0] <= xInter && xInter <= p2[0])))) {
                return p2;
            }
        }

        Utility.PrintError("Error: After 500 tries p2 does not satisfy the conditions.");
        return new Vector3(0, 0, 0);
    }

    float[] LinearCoefficients(Vector3 v1, Vector3 v2) {
        // Returns the linear coefficient from a line through v1 and v2
        // y = a*x + b

        // dz/dx
        float a = (v2[2] - v1[2]) / (v2[0] - v1[0]);

        float b = v1[2] - a * v1[0];

        return new float[] { a, b };
    }

    float xIntersect(float[] line1, float[] line2) {
        // Calculates the intersection between two lines.

        return (line2[1] - line1[1]) / (line1[0] - line2[0]);
    }

    void CreateCities() {
        // Creates a cityCount amount of cities. These cities do not overlap eachother.

        // Creates the Cities tab in the hierarchy
        GameObject cityHier = new GameObject();
        cityHier.name = "Cities";

        for (int i = 0; i < cityCount; i++) {

            // Creates a new city and initialize it
            GameObject city = (GameObject)Instantiate(cityEmpty);
            city.GetComponent<CityProperties>().Initialize();

            if (city.GetComponent<CityProperties>().succeeded) {

                //Organize in hierarchy
                city.name = "City " + i;
                city.transform.parent = cityHier.transform;


                // All tiles in the city get a reference to the city
                int xCor = city.GetComponent<CityProperties>().xCor;
                int zCor = city.GetComponent<CityProperties>().zCor;
                int radius = city.GetComponent<CityProperties>().radius;
                List<GameObject> surround = Utility.SurroundingObjects(xCor, zCor, radius);

                gridObjects[xCor, zCor].GetComponent<General>().city = city;

                foreach (GameObject tile in surround) {
                    tile.GetComponent<General>().city = city;
                }

                // All objects within a certain radius can't be in another city
                surround = Utility.SurroundingObjects(xCor, zCor, radius * 2);
                foreach (GameObject obj in surround) {
                    obj.GetComponent<General>().CityPossible = false;
                }
                gridObjects[xCor, zCor].GetComponent<General>().CityPossible = false;

                city.tag = "City";
                cities.Add(city);
            }

            // If a city fails to be made, it means we have reached the maximum number of cities
            // Therefore cityCount = i, since i are the number of cities already made.
            // We want to exit this loop, thus i = 1 > citycount
            // And we want to get rid of the failed city which still exists, thus it gets destroyed
            else {
                cityCount = i;
                Destroy(city);
                break;
            }
        }

        buildState += 1;
    }

    void CreateHighway() {
        // Using Prim's algoritm, finds the shortest minimal spanning tree (MST)
        // It finds the shortest distance between to connect all cities with eachother
        // It determines which cities are linked with a highway

        // Links keeps track which city is connected with another
        GameObject[,] links = new GameObject[cities.Count - 1, 2];

        // Available cities are not yet in the MST
        List<GameObject> cityAvailable = new List<GameObject>();
        for (int i = 0; i < cities.Count; i++) {
            cityAvailable.Add(cities[i]);
        }

        // Cities which are already part of the MST
        List<GameObject> MSTset = new List<GameObject>();

        MSTset.Add(cities[0]);
        cityAvailable.Remove(cities[0]);

        for (int h = 0; h < cities.Count - 1; h++) {
            float distMin = (float)surfaceArea;
            for (int i = 0; i < MSTset.Count; i++) {
                for (int j = 0; j < cityAvailable.Count; j++) {

                    int startXCor = MSTset[i].GetComponent<CityProperties>().xCor;
                    int startZCor = MSTset[i].GetComponent<CityProperties>().zCor;

                    int cityXCor = cityAvailable[j].GetComponent<CityProperties>().xCor;
                    int cityZCor = cityAvailable[j].GetComponent<CityProperties>().zCor;

                    int dx = Mathf.Abs(startXCor - cityXCor);
                    int dz = Mathf.Abs(startZCor - cityZCor);
                    int dist = dz + dx;

                    if (dist < distMin) {
                        distMin = dist;
                        links[h, 0] = MSTset[i];
                        links[h, 1] = cityAvailable[j];

                    }
                }
            }

            cityAvailable.Remove(links[h, 1]);
            MSTset.Add(links[h, 1]);
        }


        // If there is only one city and thus no link, place a Asphalt in the middle to start PlaceIntersect()
        if (cities.Count == 1) {
            int x0 = cities[0].GetComponent<CityProperties>().xCor;
            int z0 = cities[0].GetComponent<CityProperties>().zCor;

            if (gridObjects[x0, z0].tag == "Water") {
                gridObjects[x0, z0].GetComponent<General>().AddPiece(Bridge);
                gridObjects[x0, z0].tag = "Bridge";
                gridObjects[x0, z0].AddComponent<Asphalt>();
            }
            else if (gridObjects[x0, z0].tag == "Grass") {
                gridObjects[x0, z0].GetComponent<General>().AddPiece(Asphalt_default);
                gridObjects[x0, z0].tag = "Asphalt";
                gridObjects[x0, z0].GetComponent<General>().RemovePiece("Grass");
                Destroy(gridObjects[x0, z0].GetComponent<Grass>());
            }
        }

        // Places the road from city0 to city1
        for (int i = 0; i < links.GetLength(0); i++) {

            int x0 = links[i, 0].GetComponent<CityProperties>().xCor;
            int z0 = links[i, 0].GetComponent<CityProperties>().zCor;
            int x1 = links[i, 1].GetComponent<CityProperties>().xCor;
            int z1 = links[i, 1].GetComponent<CityProperties>().zCor;

            RoadLine(x0, x1, z0, true);
            RoadLine(z0, z1, x1, false);
        }

        // All corners can't have an intersection within two tiles
        GameObject[] asphalts = GameObject.FindGameObjectsWithTag("Asphalt");
        foreach (GameObject asphalt in asphalts) {
            int xCor = asphalt.GetComponent<General>().xCor;
            int zCor = asphalt.GetComponent<General>().zCor;

            if ((gridObjects[xCor - 1, zCor].tag == "Asphalt" || gridObjects[xCor + 1, zCor].tag == "Asphalt") &&
                (gridObjects[xCor, zCor - 1].tag == "Asphalt" || gridObjects[xCor, zCor + 1].tag == "Asphalt")) {

                List<GameObject> surround = Utility.SurroundingObjects(xCor, zCor, 2);
                foreach (GameObject obj in surround) {
                    obj.GetComponent<General>().intersectPossible = false;
                }
            }
        }

        buildState += 1;
    }

    void RoadLine(int cor1, int cor2, int constCor, bool xDir) {
        //Places Asphalt from cor1 to cor2 in either the x or z direction

        // TODO replace cor1 and cor2 with vector1 and vector2.

        //Finds the minimal and maximum value
        int minCor, maxCor;
        if (cor1 < cor2) {
            minCor = cor1;
            maxCor = cor2;
        }
        else {
            minCor = cor2;
            maxCor = cor1;
        }

        int deltaCor = maxCor - minCor;
        for (int i = 0; i <= deltaCor; i++) {
            if (xDir) {
                int xCor = minCor + i;
                int zCor = constCor;
                if (Utility.CheckWithinBounds(xCor, zCor)) {
                    if (gridObjects[xCor, zCor].tag == "Grass") {
                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Asphalt_default);
                        gridObjects[xCor, zCor].tag = "Asphalt";
                        gridObjects[xCor, zCor].GetComponent<General>().RemovePiece("Grass");
                        Destroy(gridObjects[xCor, zCor].GetComponent<Grass>());
                    }
                    else if (gridObjects[xCor, zCor].tag == "Water") {
                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Bridge);
                        gridObjects[xCor, zCor].tag = "Bridge";
                        gridObjects[xCor, zCor].AddComponent<Asphalt>();
                    }
                    gridObjects[xCor, zCor].GetComponent<Asphalt>().highway = true;
                    gridObjects[xCor, zCor].GetComponent<Asphalt>().speed_limit = 0.25f;
                }
            }
            else {
                int xCor = constCor;
                int zCor = minCor + i;
                if (Utility.CheckWithinBounds(xCor, zCor)) {
                    if (gridObjects[xCor, zCor].tag == "Grass") {
                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Asphalt_default);
                        gridObjects[xCor, zCor].tag = "Asphalt";
                        gridObjects[xCor, zCor].GetComponent<General>().RemovePiece("Grass");
                        Destroy(gridObjects[xCor, zCor].GetComponent<Grass>());
                    }
                    else if (gridObjects[xCor, zCor].tag == "Water") {
                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Bridge);
                        gridObjects[xCor, zCor].tag = "Bridge";
                        gridObjects[xCor, zCor].AddComponent<Asphalt>();
                    }
                    gridObjects[xCor, zCor].GetComponent<Asphalt>().highway = true;
                    gridObjects[xCor, zCor].GetComponent<Asphalt>().speed_limit = 0.25f;
                }
            }
        }
    }

    void PlaceIntersect() {
        // Create the intersections

        // Set to 1-5 to see how the world gets build, set to 10000 to have the world build at an instant
        int IntersectsPerFrame = int.MaxValue;

        int cityState = 0;

        for (int h = 0; h < IntersectsPerFrame && cityState < cityCount; h++) {
            GameObject city = cities[cityState];

            // TODO store roadCount and citySurface somewhere
            int roadCount = 0;
            int xCor, zCor;
            int citySurface = 0;

            // Count the citySurface and the number of Asphalt objects, to calculate wheter there is enough asphalt
            for (int i = 0; i < sizeX; i++) {
                for (int j = 0; j < sizeZ; j++) {
                    GameObject tile = gridObjects[i, j];
                    if (tile.GetComponent<General>().city == city) {

                        citySurface += 1;

                        if (tile.GetComponent<General>().tag == "Asphalt") {
                            roadCount += 1;
                        }
                    }
                }
            }

            float perc = (float)roadCount / citySurface;
            int count = 0;

            // If there isn't enough asphalt proceed with placing even more asphalt
            if (perc < asphaltPerc) {

                // Assigns the possible intersections 
                count = 0;
                List<GameObject> possibleIntersects = new List<GameObject>();
                for (int i = 0; i < sizeX; i++) {
                    for (int j = 0; j < sizeZ; j++) {
                        GameObject tile = gridObjects[i, j];
                        if (tile.GetComponent<General>().city == city && tile.GetComponent<General>().intersectPossible && tile.GetComponent<General>().tag == "Asphalt") {

                            possibleIntersects.Add(tile);
                        }
                    }
                }

                if (possibleIntersects.Count > 0) {

                    //Pick one of the possible intersections
                    int r = Random.Range(0, possibleIntersects.Count);
                    xCor = possibleIntersects[r].GetComponent<General>().xCor;
                    zCor = possibleIntersects[r].GetComponent<General>().zCor;

                    // All surrounding object within a radius of 2 from the new intersection can't become a new intersection
                    List<GameObject> surround = Utility.SurroundingObjects(xCor, zCor, 2);
                    for (int i = 0; i < surround.Count; i++) {
                        surround[i].GetComponent<General>().intersectPossible = false;
                    }

                    // Places intersection
                    if (gridObjects[xCor, zCor].tag == "Water") {
                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Bridge);
                        gridObjects[xCor, zCor].tag = "Bridge";
                        gridObjects[xCor, zCor].AddComponent<Asphalt>();
                    }
                    else if (gridObjects[xCor, zCor].tag == "Grass") {
                        gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Asphalt_default);
                    }

                    // Determines how long the streets of the intersection are
                    int maxRange = 15;
                    int minRange = 4;
                    int xLeft = Random.Range(minRange, maxRange);
                    int xRight = Random.Range(minRange, maxRange);
                    int zUp = Random.Range(minRange, maxRange);
                    int zDown = Random.Range(minRange, maxRange);

                    // For each of the 4 directions
                    // Place asphalt as long as it is less then the random length and there isn't a connection with other asphalt
                    bool connection = false;
                    bool inCity = true;
                    count = 1;
                    while (count <= xLeft && !connection && inCity) {
                        int xTemp = xCor - count;
                        inCity = PlaceRoad(xTemp, zCor, city);
                        connection = ValidateIntersect(xTemp, zCor);
                        count += 1;
                    }

                    inCity = true;
                    connection = false;
                    count = 1;
                    while (count <= xRight && !connection) {
                        int xTemp = xCor + count;
                        inCity = PlaceRoad(xTemp, zCor, city);
                        connection = ValidateIntersect(xTemp, zCor);
                        count += 1;
                    }

                    connection = false;
                    inCity = true;
                    count = 1;
                    while (count <= zDown && !connection) {
                        int zTemp = zCor - count;
                        inCity = PlaceRoad(xCor, zTemp, city);
                        connection = ValidateIntersect(xCor, zTemp);
                        count += 1;
                    }

                    connection = false;
                    inCity = true;
                    count = 1;
                    while (count <= zUp && !connection) {
                        int zTemp = zCor + count;
                        inCity = PlaceRoad(xCor, zTemp, city);
                        connection = ValidateIntersect(xCor, zTemp);
                        count += 1;
                    }
                }
                else {
                    // If no possible intersections

                    cityState += 1;
                }
            }

            else {
                // If perc > asphaltPerc

                cityState += 1;
            }

            //Once the citstate equals the cityCount, proceed towards the next build state
            if (cityState == cityCount) {
                buildState += 1;
            }
        }

        // Specialise all asphalt objects
        GameObject[] asphalt_objects = GameObject.FindGameObjectsWithTag("Asphalt");
        foreach (GameObject obj in asphalt_objects) {
            obj.GetComponent<Asphalt>().Specialize();
        }

        // Bridges which are connected to each other get linked to the same bridge_main object, which stores all the info over the bridge.
        List<GameObject> bridge_objects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Bridge"));
        int bridgeCount = 0;
        while (bridge_objects.Count > 0) {

            // Instantiate a main bridge
            GameObject bridge_main = (GameObject)Instantiate(Bridge_main);
            bridge_main.name = "Bridge_main " + bridgeCount;
            bridge_main.transform.parent = bridgeHier.transform;

            // For all bridges who are connected to temp, add them to bridge_main
            GameObject temp = bridge_objects[0];
            List<GameObject> attached_bridges = Utility.FindConnectedPieces(temp.GetComponent<General>().xCor, temp.GetComponent<General>().zCor, "Bridge");
            foreach (GameObject obj in attached_bridges) {

                obj.GetComponent<Bridge_child>().bridge_main = bridge_main;
                bridge_objects.Remove(obj);
                bridge_main.GetComponent<Bridge>().bridge_children.Add(obj);
            }

            temp.GetComponent<Bridge_child>().bridge_main = bridge_main;
            bridge_objects.Remove(temp);
            bridge_main.GetComponent<Bridge>().bridge_children.Add(temp);

            bridgeCount += 1;
        }
    }

    bool PlaceRoad(int xCor, int zCor, GameObject city) {
        // Places a road object and returns the cit

        if (Utility.CheckWithinBounds(xCor, zCor)) {
            if (CheckInCity(xCor, zCor, city)) {
                if (gridObjects[xCor, zCor].tag == "Water") {
                    gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Bridge);
                    gridObjects[xCor, zCor].tag = "Bridge";
                    gridObjects[xCor, zCor].AddComponent<Asphalt>();
                }
                else if (gridObjects[xCor, zCor].tag == "Grass") {
                    gridObjects[xCor, zCor].GetComponent<General>().AddPiece(Asphalt_default);
                    gridObjects[xCor, zCor].tag = "Asphalt";
                    gridObjects[xCor, zCor].GetComponent<General>().RemovePiece("Grass");
                    Destroy(gridObjects[xCor, zCor].GetComponent<Grass>());
                }
                return true;
            }
        }
        return false;
    }

    bool ValidateIntersect(int xCor, int zCor) {
        // Checks if an inersection is allowed on these coordinates.
        // If there is a connection with another road, no new intersection can be placed
        // within 2 tiles       
        if (CheckConnection(xCor, zCor)) {
            List<GameObject> surround = Utility.SurroundingObjects(xCor, zCor, 2);
            for (int i = 0; i < surround.Count; i++) {
                surround[i].GetComponent<General>().intersectPossible = false;
            }
            gridObjects[xCor, zCor].GetComponent<General>().intersectPossible = false;
            return true;
        }
        return false;
    }

    bool CheckConnection(int xCor, int zCor) {
        //  Return true if an road object has an connection to two or more road/bridge objects.
        // else false.

        List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
        int count = 0;
        foreach (GameObject obj in surround) {
            if (obj.GetComponent<General>().tag == "Asphalt" || obj.GetComponent<General>().tag == "Bridge") {
                count += 1;
            }
        }

        if (count >= 2) {
            return true;
        }
        else {
            return false;
        }
    }

    bool CheckInCity(int xCor, int zCor, GameObject city) {
        // Returns true when the coordinates belong to city.

        if (gridObjects[xCor, zCor].GetComponent<General>().city == city) {
            return true;
        }
        return false;

    }

    void PlaceHouses() {
        // Places a House for each Grass obj next to an Asphalt obj

        GameObject[] asphaltObjects = GameObject.FindGameObjectsWithTag("Asphalt");
        foreach (GameObject asphalt in asphaltObjects) {

            int xPos = asphalt.GetComponent<General>().xCor;
            int zPos = asphalt.GetComponent<General>().zCor;

            List<GameObject> surround = Utility.SurroundingObjectsPlus(xPos, zPos, 1);

            foreach (GameObject obj in surround) {

                int xCor = obj.GetComponent<General>().xCor;
                int zCor = obj.GetComponent<General>().zCor;
                if (gridObjects[xCor, zCor].tag == "Grass") {

                    gridObjects[xCor, zCor].GetComponent<General>().AddPiece(House);
                    gridObjects[xCor, zCor].tag = "House";
                }
            }

        }
        buildState += 1;
    }

    void PlaceTrees() {
        // Places trees when there are clear of any nonnatural objects

        GameObject[] grasses = GameObject.FindGameObjectsWithTag("Grass");

        foreach (GameObject grass in grasses) {

            int xCor = grass.GetComponent<General>().xCor;
            int zCor = grass.GetComponent<General>().zCor;

            if (CheckTreeSpace(xCor, zCor)) {
                grass.GetComponent<General>().AddPiece(Tree);
                gridObjects[xCor, zCor].tag = "Tree";
            }
        }
        buildState += 1;
    }

    bool CheckTreeSpace(int xCor, int zCor) {
        // Returns true when there is enough distance between a grass object and a non natural object.
        // This distance is random to give it a more natural feeling.


        bool treeSpace = true;
        int r = Random.Range(1, 4);
        List<GameObject> surround = Utility.SurroundingObjects(xCor, zCor, r);

        foreach (GameObject obj in surround) {
            if (obj.tag != "Grass" && obj.tag != "Tree" && obj.tag != "Water") {
                treeSpace = false;
            }
        }

        return treeSpace;
    }

    private Vector3[] PointOnBoundaries() {
        // Returns two vectors on a square edge, given by sizeX and sizeY
        // The two vectors can't be on the same edge

        Vector3[] vectors = new Vector3[2];
        int r = 0;
        for (int i = 0; i < 2; i++) {

            Vector3 vector;

            // Makes sure the vectors aren't on the same edge
            if (i == 0) {
                r = Random.Range(0, 4);
            }
            else {
                int r2 = Random.Range(0, 4);

                while (r == r2) {
                    r2 = Random.Range(0, 4);
                }
                r = r2;
            }

            // Random x and z coordinates
            float xCor = Random.Range(0f, sizeX);
            float zCor = Random.Range(0f, sizeZ);

            // Based on a random r picks the boundary
            if (r == 0) {
                vector = new Vector3(xCor, 0f, 0f);
            }
            else if (r == 1) {
                vector = new Vector3(0f, 0f, zCor);
            }
            else if (r == 2) {
                vector = new Vector3(xCor, 0f, sizeZ);
            }
            else {
                vector = new Vector3(sizeX, 0f, zCor);
            }
            vectors[i] = vector;
        }

        return vectors;
    }

    private Vector3 PointWithinBoundary() {
        // Returns a vector within a square 

        float xCor = Random.Range(0f, sizeX);
        float zCor = Random.Range(0f, sizeZ);

        Vector3 vector = new Vector3(xCor, 0, zCor);

        return vector;
    }

    private Vector3[] createBezierPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        // Returns all bezierpoints according to the theory for a bezier curve
        // p0 and p3 are the ends. They have been limited to the edge of the created world (sizeX, sizeZ)
        // p1 and p2 represent the gradient of the line. They have been limited to within de created world

        Vector3[] positions = new Vector3[numPoints];

        // Points get calculated
        for (int i = 1; i < numPoints + 1; i++) {

            float t = i / (float)numPoints;
            Vector3 p = calculateBezierPoints(t, p0, p1, p2, p3);
            positions[i - 1] = p;
        }

        return positions;
    }

    private Vector3 calculateBezierPoints(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        // Calculates one bezierpoint given the argument and this formula:
        // B(t) = (1 - t) ** 3 * p0 + 3 * t * p1 * (1 - t) ** 2 + 3 *(1 - t) * t ** 2 * p2 + t ** 3 * p3 

        // simplification
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float ttt = t * t * t;
        float uuu = u * u * u;
        Vector3 p = uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;

        return p;
    }

    private void UnitTest() {
        // Checks if there are duplicate child objects or script components.

        Debugging.ComponentCheck();
        Debugging.ChildCheck();

        Debug.Break();
    }

    void StartLiving() {
        // Building phase complete. Now we start living!

        UnitTest();
        gameObject.GetComponent<LiveWorld>().enabled = true;
    }
}
