using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    public GameObject startHouse, endHouse;
    public GameObject carHier;
    int lifeTime = 0;
    List<GameObject> path = new List<GameObject>();
    // Start is called before the first frame update
    public void Initialize() {
        Debug.Log("Spawn Car");
        if (gameObject == null) {
            Debug.Log("ok");
        }

        gameObject.transform.parent = carHier.transform;

        GameObject[] houses = GameObject.FindGameObjectsWithTag("House");
        int r1 = Random.Range(0, houses.Length);
        int r2 = Random.Range(0, houses.Length);

        startHouse = houses[r1];
        endHouse = houses[r2];

        startHouse.transform.GetComponent<Renderer>().material.color = Color.red;
        endHouse.transform.GetComponent<Renderer>().material.color = Color.blue;






    }

    private void Update() {

        lifeTime++;
        if (lifeTime == 1) {
            path = Navigation2(startHouse, endHouse);
        }


        /*
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < 100; i++) {
            Navigation(startHouse, endHouse);
        }
        float endTime = Time.realtimeSinceStartup;
        Debug.Log("Duration original: " + (endTime - startTime));


        startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < 100; i++) {
            Navigation2(startHouse, endHouse);
        }
        endTime = Time.realtimeSinceStartup;
        Debug.Log("Duration new: " + (endTime - startTime));
        */
        Navigation2(startHouse, endHouse);

    }

    GameObject FindRoad(GameObject house) {
        GameObject road = null;
        int xCor = house.GetComponent<General>().xCor;
        int zCor = house.GetComponent<General>().zCor;
        List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
        foreach (GameObject obj in surround) {
            if (obj.tag == "Asphalt") {
                road = obj;
            }
        }
        return road;
    }

    List<GameObject> Navigation(GameObject house1, GameObject house2) {
        GameObject startRoad = FindRoad(startHouse);
        GameObject endRoad = FindRoad(endHouse);

        startRoad.transform.GetComponent<Renderer>().material.color = Color.red;
        endRoad.transform.GetComponent<Renderer>().material.color = Color.blue;

        gameObject.transform.position = startRoad.transform.position;

        bool connected = false;
        List<List<GameObject>> allAsphalt = new List<List<GameObject>>() { new List<GameObject>() { startRoad, null } };

        List<GameObject> newAsph = new List<GameObject>() { startRoad };
        int count = 0;
        while (!connected && count < 500) {

            List<GameObject> tempAsph = new List<GameObject>();
            foreach (GameObject asphalt in newAsph) {
                int xCor = asphalt.GetComponent<General>().xCor;
                int zCor = asphalt.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
                foreach (GameObject obj in surround) {
                    if (obj.tag == "Asphalt" || obj.tag == "Bridge") {
                        bool alreadyUsed = false;

                        for (int i = 0; i < allAsphalt.Count; i++) {
                            GameObject o = allAsphalt[i][0];
                            if (o == obj) {
                                alreadyUsed = true;
                                break;
                            }
                        }
                        if (!alreadyUsed) {
                            tempAsph.Add(obj);
                            allAsphalt.Add(new List<GameObject>() { obj, asphalt });


                            if (obj == endRoad) {
                                connected = true;
                            }
                        }
                    }
                }
            }


            //prevOuterAsph = new List<GameObject>(outerAsph);
            newAsph = tempAsph;
            count += 1;
        }
        bool pathFound = false;
        GameObject end = endRoad;

        List<GameObject> path = new List<GameObject>() { end };
        count = 0;
        while (!pathFound && count < 500) {

            for (int i = allAsphalt.Count - 1; i > 0 ; i--) {

                if (allAsphalt[i][0] == end) {
                    path.Add(allAsphalt[i][1]);
                    if (allAsphalt[i][1] == startRoad) {
                        pathFound = true;
                    }
                    end = allAsphalt[i][1];

                }
            }
            count++;
        }

        path.Reverse();

        /*
        foreach (GameObject obj in path) {
            if (obj.tag != "Bridge") {
                obj.transform.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
        */
        

        return path;
    }

    List<GameObject> Navigation2(GameObject house1, GameObject house2) {

        // Finds the start and endRoad
        GameObject startRoad = FindRoad(startHouse);
        GameObject endRoad = FindRoad(endHouse);

        // The path the car has to drive
        List<GameObject> path = new List<GameObject>();

        startRoad.transform.GetComponent<Renderer>().material.color = Color.red;
        endRoad.transform.GetComponent<Renderer>().material.color = Color.blue;

        // Set the car to its starting position
        gameObject.transform.position = startRoad.transform.position;


        GameObject startCity = startRoad.GetComponent<General>().city;
        GameObject endCity = endRoad.GetComponent<General>().city;

        bool onHighway = false;
        if (startRoad.GetComponent<Asphalt>().highway) {
            onHighway = true;
        }

        /*
         If a road is not in a city, this means that the road is a highway, therefore we only have to look through the highway roads
         When both cities are null, we are only looking for highway Roads
         When the startcity is null, we find a path from endRoad to highway and from there we look through all the highways
         When the endCity is null, we find a path from startRoad to highway and from there we look through all the highways
         When both cities are not null, 
            find a path from startRoad to a higway tile, 
            then find a path to the endCity walking over highway tiles 
            and finally find the endRoad       
        */
        if (startCity == null && endCity == null) {
            path.Add(startRoad);
            path.AddRange(FindRoadOnHighway(startRoad, endRoad));
        }

        else if (startCity == null) {
            path.Add(endRoad);

            if (!endRoad.GetComponent<Asphalt>().highway) {
                path.AddRange(FindHighway(endRoad));
            }

            path.AddRange(FindRoadOnHighway(path[path.Count - 1], startRoad));
            path.Reverse();

        }

        else if (endCity == null) {
            path.Add(startRoad);

            if (!onHighway) {
                path.AddRange(FindHighway(startRoad));
            }

            path.AddRange(FindRoadOnHighway(path[path.Count - 1], endRoad));
        }
        else{
            path.Add(startRoad);
            if (!startRoad.GetComponent<Asphalt>().highway) {
                path.AddRange(FindHighway(startRoad));
            }
            if (startRoad.GetComponent<General>().city != endCity) {
                path.AddRange(FindCity(path[path.Count - 1], endCity));
            }
            path.AddRange(FindRoad(path[path.Count - 1], endRoad));
        }
        
        // Visualisation of the path
        float index = 1;
        Debug.Log("PathCount" + path.Count);
        foreach (GameObject obj in path) {
            if (obj.tag != "Bridge") {
                obj.transform.GetComponent<Renderer>().material.color = new Color(index, index, index);
            }
            Debug.Log(obj.name, obj);
            index -= (float)(1/(float)path.Count);
        }
        
        

        return path;
    }

    List<GameObject> FindPath(GameObject startRoad, GameObject endRoad, List<List<GameObject>> options) {
        // Finds a path from startRoad to endRoad, startRoad not included

        GameObject end = endRoad;
        List<GameObject> path = new List<GameObject>() { end };

        // If they happen to already be the same, then the path is just the endRoad
        if (startRoad == endRoad) {
            path.Add(endRoad);
        }

        // Find the path from startRoad to endRoad
        for (int i = options.Count - 1; i > 0; i--) {

            if (options[i][0] == end) {
                if (options[i][1] == startRoad) {
                }
                else {
                    path.Add(options[i][1]);
                }
                end = options[i][1];
            }
        }

        /*
        foreach (GameObject obj in path) {
            Debug.Log(obj.name, obj);
        }
        */

        return path;
    }

    List<GameObject> FindHighway(GameObject startRoad) {
        // Finds the closest highway from a starting point

        Debug.Log("Find highway");
        bool highwayFound = false;
        int count = 0;
        List<GameObject> path = new List<GameObject>();
        List<GameObject> newAsph = new List<GameObject>() { startRoad };
        List<GameObject> prevAsph = new List<GameObject>();
        List<List<GameObject>> testAsphalt = new List<List<GameObject>>() { new List<GameObject>() { startRoad, null } };

        while (!highwayFound && count < 500) {
            List<GameObject> tempAsph = new List<GameObject>();
            foreach (GameObject asphalt in newAsph) {


                int xCor = asphalt.GetComponent<General>().xCor;
                int zCor = asphalt.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
                foreach (GameObject obj in surround) {
                    if (obj.tag == "Asphalt" || obj.tag == "Bridge") {

                        bool alreadyUsed = false;
                        foreach (GameObject o in prevAsph) {
                            if (o == obj) {
                                alreadyUsed = true;
                                break;
                            }
                        }

                        if (!alreadyUsed) {
                            testAsphalt.Add(new List<GameObject>() { obj, asphalt });
                            tempAsph.Add(obj);

                            if (obj.GetComponent<Asphalt>().highway) {
                                highwayFound = true;

                                if (testAsphalt.Count >= 1) {
                                    List<GameObject> tempPath = FindPath(startRoad, obj, testAsphalt);
                                    tempPath.Reverse();
                                    path.AddRange(tempPath);
                                }
                                else {
                                    path.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            prevAsph = newAsph;
            newAsph = tempAsph;
            count++;
        }




        return path;
    }

    List<GameObject> FindCity(GameObject startRoad, GameObject city) {
        // Finds the city from a starting point, since cities are connected with eachother only through highways
        // We only have to walk over the higways
        // StartRoad has to be a highway

        Debug.Log("Find city");
        bool cityFound = false;
        int count = 0;
        List<GameObject> path = new List<GameObject>();
        List<GameObject> newAsph = new List<GameObject>() { startRoad };
        List<GameObject> prevAsph = new List<GameObject>();
        List<List<GameObject>> testAsphalt = new List<List<GameObject>>() { new List<GameObject>() { startRoad, null } };

        while (!cityFound && count < 500) {
            List<GameObject> tempAsph = new List<GameObject>();

            foreach (GameObject asphalt in newAsph) {

                int xCor = asphalt.GetComponent<General>().xCor;
                int zCor = asphalt.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
                foreach (GameObject obj in surround) {
                    if ((obj.tag == "Asphalt" || obj.tag == "Bridge") && obj.GetComponent<Asphalt>().highway) {

                        bool alreadyUsed = false;
                        foreach (GameObject o in prevAsph) {
                            if (o == obj) {
                                alreadyUsed = true;
                                break;
                            }
                        }

                        if (!alreadyUsed) {
                            tempAsph.Add(obj);
                            testAsphalt.Add(new List<GameObject>() { obj, asphalt });

                            if (obj.GetComponent<General>().city == city) {
                                cityFound = true;

                                if (testAsphalt.Count >= 1) {

                                    List<GameObject> tempPath = FindPath(startRoad, obj, testAsphalt);
                                    tempPath.Reverse();

                                    path.AddRange(tempPath);
                                }
                                else {
                                    path.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            prevAsph = newAsph;
            newAsph = tempAsph;
            count++;
        }

        return path;
    }

    List<GameObject> FindRoad(GameObject startRoad, GameObject endRoad) {
        // Finds a path between startRoad and endRoad

        Debug.Log("Find road");
        bool roadFound = false;
        int count = 0;
        List<GameObject> path = new List<GameObject>();
        List<GameObject> newAsph = new List<GameObject>() { startRoad };
        List<GameObject> prevAsph = new List<GameObject>();
        List<List<GameObject>> testAsphalt = new List<List<GameObject>>() { new List<GameObject>() { startRoad, null } };

        while (!roadFound && count < 500) {

            List<GameObject> tempAsph = new List<GameObject>();
            foreach (GameObject asphalt in newAsph) {

                int xCor = asphalt.GetComponent<General>().xCor;
                int zCor = asphalt.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
                foreach (GameObject obj in surround) {
                    if (obj.tag == "Asphalt" || obj.tag == "Bridge") {

                        bool alreadyUsed = false;
                        foreach (GameObject o in prevAsph) {
                            if (o == obj) {
                                alreadyUsed = true;
                                break;
                            }
                        }
                        if (!alreadyUsed) {
                            tempAsph.Add(obj);
                            testAsphalt.Add(new List<GameObject>() { obj, asphalt });

                            if (obj == endRoad) {
                                roadFound = true;

                                if (testAsphalt.Count >= 1) {

                                    List<GameObject> tempPath = FindPath(startRoad, endRoad, testAsphalt);
                                    tempPath.Reverse();

                                    path.AddRange(tempPath);
                                }
                                else {
                                    path.Add(obj);
                                }
                            }
                        }
                    }
                }
            }

            prevAsph = newAsph;
            newAsph = tempAsph;
            count++;
        }

        return path;
    }

    List<GameObject> FindRoadOnHighway(GameObject startRoad, GameObject endRoad) {
        // Finds a path from startRoad to endRoad
        // endRoad has to be on a highway

        Debug.Log("Find road on highway");
        bool roadFound = false;
        int count = 0;
        List<GameObject> path = new List<GameObject>();
        List<GameObject> newAsph = new List<GameObject>() { startRoad };
        List<GameObject> prevAsph = new List<GameObject>();
        List<List<GameObject>> testAsphalt = new List<List<GameObject>>() { new List<GameObject>() { startRoad, null } };

        while (!roadFound && count < 500) {

            List<GameObject> tempAsph = new List<GameObject>();
            foreach (GameObject asphalt in newAsph) {


                int xCor = asphalt.GetComponent<General>().xCor;
                int zCor = asphalt.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xCor, zCor, 1);
                foreach (GameObject obj in surround) {
                    if ((obj.tag == "Asphalt" || obj.tag == "Bridge") && obj.GetComponent<Asphalt>().highway) {

                        bool alreadyUsed = false;
                        foreach (GameObject o in prevAsph) {
                            if (o == obj) {
                                alreadyUsed = true;
                                break;
                            }
                        }
                        if (!alreadyUsed) {
                            tempAsph.Add(obj);

                            testAsphalt.Add(new List<GameObject>() { obj, asphalt });
                        
                            if (obj == endRoad) {
                                roadFound = true;
                                if (testAsphalt.Count >= 1) {

                                    List<GameObject> tempPath = FindPath(startRoad, endRoad, testAsphalt);
                                    tempPath.Reverse();

                                    path.AddRange(tempPath);
                                }
                                else {
                                    path.Add(obj);
                                }
                            }
                        }
                    }
                }
            }

            prevAsph = newAsph;
            newAsph = tempAsph;
            count++;
        }
        return path;
    }
}
