using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    // TODO Remove count dependancy in while loops

    public GameObject startHouse, endHouse;
    public GameObject carHier;
    public float speed = 0.1f;
    private List<GameObject> path = new List<GameObject>();
    private Vector3 target;

    public void Initialize() {
        // Initialize car

        gameObject.transform.parent = carHier.transform;

        GameObject[] houses = GameObject.FindGameObjectsWithTag("House");
        int r1 = Random.Range(0, houses.Length - 1);

        //TODO make sure r2 != r1
        int r2 = Random.Range(0, houses.Length - 1);

        startHouse = houses[r1];
        endHouse = houses[r2];

        path = Navigation(startHouse, endHouse);

        //Stops when the path of the car is of length 1.
        if (path.Count == 1) {

            GameObject.Find("World_field").GetComponent<LiveWorld>().SpawnCar();
            Destroy(gameObject);
            return;
        }

        // Set position to middle of path[0]
        transform.position = path[0].transform.position;

        // Rotate car to next path
        transform.LookAt(path[1].transform);

        // Move car to the right lane
        transform.position += transform.right * 0.2f;

        target = FindTarget();

    }

    private void Update() {

        DriveCar();
    }

    private void DriveCar() {
        // Drives car

        if (checkFreeSpace()) {

            // Makes sure the car doesn't crash the simulation
            int while_count = 0;

            float distLeft = Get_asphalt_object(transform.position).GetComponent<Asphalt>().speed_limit;

            // Keeps track of many asphalt_colliders the car has found
            int collide_count = 0;

            while (path.Count > 0 && distLeft > Vector3.Distance(transform.position, target) && while_count < 50) {

                distLeft -= Vector3.Distance(transform.position, target);
                transform.position = target;

                // If we reach the next asphalt object in path, then reset the collide count per asphalt
                if (Get_asphalt_object(transform.position) == path[0]) {
                    collide_count = 0;
                    path.Remove(path[0]);
                }
                else {
                    collide_count += 1;
                }

                //If there isn't any path left stop
                if (path.Count == 0) {
                    break;
                }

                // Each asphalt object has four colliders in each quadrant. The first collisions with such a colliders checks if the path is to the right of the car.
                // The second checks if the path is to the left.
                //TODO move Get_asphalt_object to Utility.cs
                if (collide_count == 0) {
                    if (Get_asphalt_object(transform.position + transform.TransformDirection(Vector3.right)) == path[0]) {
                        transform.Rotate(0, 90, 0);
                    }
                }
                else if (collide_count == 1) {
                    if (Get_asphalt_object(transform.position + transform.TransformDirection(Vector3.left)) == path[0]) {
                        transform.Rotate(0, -90, 0);
                    }
                }
                else if (collide_count == 3) {
                    Debug.Break();
                    Debug.Log("WHOOPS, This should not have happend");
                }

                target = FindTarget();


                if (!checkFreeSpace()) {

                    return;
                }

                while_count += 1;

            }

            // The remainder of distLeft is added to the currentPos
            if (path.Count > 0) {
                float dist = Vector3.Distance(transform.position, target);
                float perc = distLeft / dist;

                transform.position = Vector3.Lerp(transform.position, target, perc);
            }
            else {
                Destroy(gameObject);
                GameObject.Find("World_field").GetComponent<LiveWorld>().SpawnCar();
            }
        }
    }

    bool checkFreeSpace() {
        // return true if there is enough room for the car to travel into.

        bool freeSpace = true;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 0.5f, LayerMask.GetMask("Bridge"))) {
            // When a bridge object is hit, the car behaves differently for the four possible states from the bridge:
            //      closed: The boat can move forward
            //      open: The boat can't move forward. If there is no car on the bridge, open the bridge.
            //      closing: The boat has to wait until the bridge is closed.
            //      opening: The boat has to wait until the bridge is open. The boats are then allowed to move under the bridge and then the bridge closes.


            GameObject obj = hit.transform.parent.gameObject;

            GameObject bridge_main = obj.GetComponent<Bridge_child>().bridge_main;

            if (bridge_main.GetComponent<Bridge>().state == "closed") {
                if (!bridge_main.GetComponent<Bridge>().objects.Contains(gameObject)) {

                    bridge_main.GetComponent<Bridge>().objects.Add(gameObject);
                }
                freeSpace = true;
            }
            else if (bridge_main.GetComponent<Bridge>().state == "open") {
                if (bridge_main.GetComponent<Bridge>().occupied_state != "Boat") {

                    bridge_main.GetComponent<Bridge>().state = "closing";
                }
                freeSpace = false;
            }

            else if (bridge_main.GetComponent<Bridge>().state == "opening") {

                freeSpace = false;
            }

            else if (bridge_main.GetComponent<Bridge>().state == "closing") {
                if (!bridge_main.GetComponent<Bridge>().objects.Contains(gameObject)) {
                    bridge_main.GetComponent<Bridge>().objects.Add(gameObject);
                }
                freeSpace = false;
            }
        }

        // checks for a hit against a car
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 1, LayerMask.GetMask("Car"))) {
            freeSpace = false;
        }

        return freeSpace;
    }

    GameObject Find_connected_road(GameObject house) {
        // Finds a road which is connected to house obj.

        int xCor = house.GetComponent<General>().xCor;
        int zCor = house.GetComponent<General>().zCor;
        List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
        foreach (GameObject obj in surround) {
            if (obj.tag == "Asphalt") {
                return obj;
            }
        }

        Debug.Log("No road found");
        Debug.Break();
        return null;
    }

    Vector3 FindTarget() {
        // Finds the next target to drive towards.

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * Mathf.Infinity, Color.cyan);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Asphalt"))) {
            return hit.transform.position;
        }
        else {
            //If no target is found, visualise the path, color the start and endhouse, and stop the simulation.

            float index = 1;
            Debug.Log("No target found", gameObject);
            foreach (GameObject obj in path) {
                Debug.Log("obj in path" + obj.name, obj);
                foreach (Transform child in obj.transform) {
                    if (!child.name.Contains("Asphalt_colliders")) {
                        child.gameObject.GetComponent<Renderer>().material.color = new Color(index, index, index);
                    }
                }

                index -= (float)(1 / (float)path.Count);
            }

            startHouse.GetComponent<General>().GetChild("House").GetComponent<Renderer>().material.color = Color.red;
            endHouse.GetComponent<General>().GetChild("House").GetComponent<Renderer>().material.color = Color.blue;

            Debug.Break();
            return new Vector3(0, 0, 0);
        }
    }

    GameObject Get_asphalt_object(Vector3 position) {
        // Get the asphalt object through the transform of the car.

        //TODO xCor and zCor can fall outside of gridObjects.size
        int xCor = (int)Mathf.Floor(position.x);
        int zCor = (int)Mathf.Floor(position.z);

        return BuildWorld.gridObjects[xCor, zCor];
    }

    List<GameObject> Navigation(GameObject house1, GameObject house2) {
        /*
         * Looks for a path from house1 to house2. It does this by starting from the startRoad and then searching it's
         * neighbours for other asphalt objects. These new asphalt objects are then checked if they correspond with 
         * endRoad. If it does not correspond, we look for new asphalt objects from the just found asphalt objects. 
         * This area grows until endRoad is found.
         * 
         * Optimasation: 
         * If a road is not in a city, this implies that the road is a highway, therefore we only have to look through the highway roads 
         * until we find a city.
         * When both cities are null, we are only looking for highway Roads.
         * When the startcity is null, we find a path from endRoad to highway and from there we look through all the highways
         * When the endCity is null, we find a path from startRoad to highway and from there we look through all the highways
         * When both cities are not null,
         *      Check if startcity == endcity
         *          find endroad
         *      else
         *          find a path from startRoad to a higway tile, 
         *          then find a path to the endCity walking over highway tiles 
         *          and finally find the endRoad.
         */

        // Finds startRoad and endRoad
        GameObject startRoad = Find_connected_road(startHouse);
        GameObject endRoad = Find_connected_road(endHouse);

        // The path the car has to drive
        List<GameObject> path = new List<GameObject>();

        GameObject startCity = startRoad.GetComponent<General>().city;
        GameObject endCity = endRoad.GetComponent<General>().city;

        if (startCity == null && endCity == null) {
            path.Add(startRoad);
            path.AddRange(FindPath(path[path.Count - 1], endRoad, null, "Road on highway"));
        }

        else if (startCity == null) {
            path.Add(endRoad);

            if (!endRoad.GetComponent<Asphalt>().highway) {
                path.AddRange(FindPath(path[path.Count - 1], null, null, "Highway"));
            }

            path.AddRange(FindPath(path[path.Count - 1], startRoad, null, "Road on highway"));
            path.Reverse();
        }

        else if (endCity == null) {
            path.Add(startRoad);

            if (!startRoad.GetComponent<Asphalt>().highway) {
                path.AddRange(FindPath(path[path.Count - 1], null, null, "Highway"));
            }

            path.AddRange(FindPath(path[path.Count - 1], endRoad, null, "Road on highway"));
        }

        else {

            path.Add(startRoad);

            if (startRoad.GetComponent<General>().city != endCity) {

                if (!startRoad.GetComponent<Asphalt>().highway) {

                    path.AddRange(FindPath(path[path.Count - 1], null, null, "Highway"));
                }

                path.AddRange(FindPath(path[path.Count - 1], null, endCity, "City"));
            }
            path.AddRange(FindPath(path[path.Count - 1], endRoad, null, "Road"));
        }

        return path;
    }

    List<GameObject> GetLinkedPaths(GameObject startRoad, GameObject endRoad, List<List<GameObject>> options) {
        // Finds a path from startRoad to endRoad, startRoad not included.
        // Looping over al the options.

        GameObject end = endRoad;
        List<GameObject> path = new List<GameObject>() { end };

        // Find the path from startRoad to endRoad
        for (int i = options.Count - 1; i > 0; i--) {

            if (options[i][0] == end) {
                if (options[i][1] == startRoad) {
                    return path;
                }
                else {
                    path.Add(options[i][1]);
                }
                end = options[i][1];
            }
        }

        Debug.Break();
        Debug.Log("No links found from startRoad to endRoad.");
        return path;
    }

    List<GameObject> FindPath(GameObject startRoad, GameObject endRoad, GameObject city, string cond) {
        // Finds a road from startRoad to an asphalt object which satisfies the conditions, found in Navigation_end_condition. 

        // If they happen to be the same, return an empty list.
        if (startRoad == endRoad) {
            return new List<GameObject>();
        }

        //The path that gets returned.
        List<GameObject> path = new List<GameObject>();

        //All new asphalt objects which are found. These will be used for the next loop.
        List<GameObject> currentAsph = new List<GameObject>() { startRoad };

        //All previously found asphalt objects, newly found objects get checked to this list to make sure we aren't re-using the same asphalt objects.
        List<GameObject> prevAsph = new List<GameObject>();

        //All the links between two asphalt pieces, this is used to get the path.
        List<List<GameObject>> testAsphalt = new List<List<GameObject>>() { new List<GameObject>() { startRoad, null } };

        int while_count = 0;
        while (true) {

            // A list with all asphalt object who will be used in the next iteration
            List<GameObject> nextAsph = new List<GameObject>();

            // Loop over all current asphalt objects, and check if one their surrounding is endRoad
            foreach (GameObject asphalt in currentAsph) {

                int xCor = asphalt.GetComponent<General>().xCor;
                int zCor = asphalt.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);

                foreach (GameObject obj in surround) {

                    // Checks if obj satisfies the conditions based on cond
                    if (Navigation_allowed_objects(obj, cond)) {

                        // Checks if obj is alreadyUsed in a previous iteration.
                        bool alreadyUsed = false;
                        foreach (GameObject o in prevAsph) {
                            if (o == obj) {
                                alreadyUsed = true;
                                break;
                            }
                        }

                        if (!alreadyUsed) {

                            nextAsph.Add(obj);
                            testAsphalt.Add(new List<GameObject>() { obj, asphalt });

                            // Endcondition
                            if (Navigation_end_condition(obj, endRoad, city, cond)) {

                                if (testAsphalt.Count >= 1) {

                                    List<GameObject> tempPath = GetLinkedPaths(startRoad, obj, testAsphalt);
                                    tempPath.Reverse();

                                    path.AddRange(tempPath);
                                }
                                else {
                                    path.Add(obj);
                                }
                                return path;
                            }
                        }
                    }
                }
            }

            prevAsph = currentAsph;
            currentAsph = nextAsph;

            // Makes sure there is no infinite loop
            // TODO replace 500 with a funcion of sizeX and sizeZ
            while_count++;
            if (while_count > 500) {

                Utility.PrintError($"Path is not found after {while_count} tries.", gameObject);
                return null;
            }
        }
    }

    bool Navigation_allowed_objects(GameObject obj, string cond) {
        // Checks if a desired state of an asphalt object is met.

        if (cond == "City" || cond == "Road on highway") {
            return (obj.tag == "Asphalt" || obj.tag == "Bridge") && obj.GetComponent<Asphalt>().highway;
        }
        else if (cond == "Highway" || cond == "Road") {
            return obj.tag == "Asphalt" || obj.tag == "Bridge";
        }
        else {
            Debug.Log("Not the right condition given.");
            return false;
        }
    }

    bool Navigation_end_condition(GameObject obj, GameObject endRoad, GameObject city, string cond) {
        // Checks if a desired state of an asphalt object is met.

        if (cond == "City") {
            return obj.GetComponent<General>().city == city;
        }
        else if (cond == "Highway") {
            return obj.GetComponent<Asphalt>().highway;
        }
        else if (cond == "Road" || cond == "Road on highway") {
            return obj == endRoad;
        }
        else {
            Debug.Log("Not the right condition given");
            return false;
        }
    }
}
