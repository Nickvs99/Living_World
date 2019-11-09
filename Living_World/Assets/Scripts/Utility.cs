using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility {
    // Class of functions who are


    public static int[] CheckCordinates(Vector3 position) {
        // Returnes the cordinates of a vector3

        int[] cordinates = new int[2];
        cordinates[0] = Mathf.FloorToInt(position[0]);
        cordinates[1] = Mathf.FloorToInt(position[2]);

        return cordinates;
    }

    public static bool CheckWithinBounds(int xCor, int zCor) {
        // Returns wheter the given coordiantes are between 0 and sizeX,sizeZ

        if (xCor >= 0 && xCor < BuildWorld.sizeX && zCor >= 0 && zCor < BuildWorld.sizeZ) {
            return true;
        }
        else {
            return false;
        }
    }

    public static float Dist(int x1, int z1, int x2, int z2) {
        // Returns the distance between two cordinates

        return Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(z1 - z2, 2));
    }

    //TODO include original piece
    public static List<GameObject> FindConnectedPieces(int xCor, int zCor, string state) {
        // Returns all object who the state tag and are connected to the [xCor, zCor] object.


        bool newFound = true;
        List<GameObject> connectedObjects = new List<GameObject>();

        List<GameObject> newFoundObjects = new List<GameObject>() { BuildWorld.gridObjects[xCor, zCor] };
        int count = 0;
        while (newFound) {
            newFound = false;

            List<GameObject> tempBridges = new List<GameObject>();

            foreach (GameObject newObject in newFoundObjects) {

                int xTemp = newObject.GetComponent<General>().xCor;
                int zTemp = newObject.GetComponent<General>().zCor;

                List<GameObject> surround = Utility.SurroundingObjectsPlus(xTemp, zTemp, 1);
                foreach (GameObject obj in surround) {
                    if (obj.tag == state && !connectedObjects.Contains(obj)) {
                        connectedObjects.Add(obj);
                        tempBridges.Add(obj);
                        newFound = true;
                    }
                }
            }

            newFoundObjects = new List<GameObject>();
            foreach (GameObject obj in tempBridges) {
                newFoundObjects.Add(obj);
            }
            count += 1;
        }
        return connectedObjects;
    }

    public static Vector3[] FloorVectors(Vector3[] UnroundedVectors) {
        // Floors all vector components for every vector within the list

        Vector3[] RoundedVectors = new Vector3[UnroundedVectors.Length];
        for (int i = 0; i < UnroundedVectors.Length; i++) {

            Vector3 coordinates = UnroundedVectors[i];
            for (int j = 0; j < 3; j++) {

                coordinates[j] = Mathf.Floor(coordinates[j]);
            }

            RoundedVectors[i] = coordinates;
        }

        return RoundedVectors;
    }

    public static GameObject GetNeighbour(GameObject origin, string direction) {
        // Returns a specific neighbour, "up", "down", "left", "right" are the possible directions
        // If the neighbour is out of bounds, then return null.

        int xCor = origin.GetComponent<General>().xCor;
        int zCor = origin.GetComponent<General>().zCor;

        GameObject neighbour = null;
        if (direction == "up") {
            if (CheckWithinBounds(xCor, zCor + 1)) {
                neighbour = BuildWorld.gridObjects[xCor, zCor + 1];
            }
        }
        else if (direction == "down") {
            if (CheckWithinBounds(xCor, zCor - 1)) {
                neighbour = BuildWorld.gridObjects[xCor, zCor - 1];
            }
        }
        else if (direction == "left") {
            if (CheckWithinBounds(xCor - 1, zCor)) {
                neighbour = BuildWorld.gridObjects[xCor - 1, zCor];
            }
        }
        else if (direction == "right") {
            if (CheckWithinBounds(xCor + 1, zCor)) {
                neighbour = BuildWorld.gridObjects[xCor + 1, zCor];
            }
        }

        return neighbour;
    }

    public static void Print(params object[] args) {
        // Prints the given statements

        string log = "";
        for (int i = 0; i < args.Length; i++) {

            log += args[i];
            if (i != args.Length - 1) {
                log += ", ";
            }
        }
        Debug.Log(log);
    }

    public static void PrintError(string s) {
        Debug.LogError(s);
        Debug.Break();
    }

    public static void PrintError(string s, GameObject obj) {
        Debug.LogError(s, obj);
        Debug.Break();
    }

    public static List<GameObject> SurroundingObjects(int xCor, int zCor, int size) {
        //Returns all surrounding objects, does not include the middle

        List<GameObject> surroundingObjects = new List<GameObject>();
        for (int i = -size; i <= size; i++) {
            for (int j = -size; j <= size; j++) {

                int xTemp = xCor + i;
                int zTemp = zCor + j;

                if (CheckWithinBounds(xTemp, zTemp) && !(i == 0 && j == 0)) {
                    surroundingObjects.Add(BuildWorld.gridObjects[xTemp, zTemp]);
                }
            }
        }

        return surroundingObjects;
    }

    public static List<GameObject> SurroundingObjectsPlus(int xCor, int zCor, int size) {
        // Collects the surrounding objects in a plus like way, no diagnonals

        List<GameObject> surroundingObjects = new List<GameObject>();
        for (int i = -size; i <= size; i++) {
            int xTemp = xCor + i;
            if (CheckWithinBounds(xTemp, zCor) && !(i == 0)) {
                surroundingObjects.Add(BuildWorld.gridObjects[xTemp, zCor]);
            }
        }
        for (int i = -size; i <= size; i++) {
            int zTemp = zCor + i;
            if (CheckWithinBounds(xCor, zTemp) && !(i == 0)) {
                surroundingObjects.Add(BuildWorld.gridObjects[xCor, zTemp]);

            }
        }
        return surroundingObjects;
    }

    public static void UpdateCordinates(GameObject obj) {
        // Updates the xCor and zCor based on its transform

        int xCor = (int)obj.transform.position[0];
        int zCor = (int)obj.transform.position[2];
    }

    public static Vector3 CalcPosition(int x, int z) {
        // Calaculates the position depening on there x and z coordinate.

        return new Vector3(x + 0.5f, 0, z + 0.5f);
    }
}
