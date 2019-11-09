using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityProperties : MonoBehaviour {
    // Manages a city.

    public int xCor;
    public int zCor;
    public int radius;
    public bool succeeded;

    public GameObject centerHub;

    public void Initialize() {
        // Initialize a city

        int sizeX = BuildWorld.sizeX;
        int sizeZ = BuildWorld.sizeZ;

        // Random properties of the city
        // The range is due to not wanting the center of the city on the edge of the map
        xCor = Random.Range(1, sizeX - 2);
        zCor = Random.Range(1, sizeZ - 2);
        radius = Random.Range(5, 10);

        // Keep searching for new city properties until new city doesnt overlap with any of the previous
        // TODO Find a non brute forcing method for placing a new city
        int count = 0;
        while (!CheckFreeSpace(xCor, zCor, radius) && count < 500) {
            xCor = Random.Range(1, sizeX - 2);
            zCor = Random.Range(1, sizeZ - 2);
            radius = Random.Range(10, 20);

            count += 1;
        }

        gameObject.transform.position = new Vector3(xCor, 1, zCor);

        // If it took too many tries there was too little room for the city
        if (count < 500) {
            succeeded = true;
        }
        else {
            succeeded = false;
        }
    }

    bool CheckFreeSpace(int xCor, int zCor, int radius) {
        // Checks if there is enough room to fit the new city

        bool freeSpace = true;
        GameObject[] cities = GameObject.FindGameObjectsWithTag("City");

        // Foreach already existing city checks wheter the new city overlaps with any of the old ones
        foreach (GameObject city in cities) {

            int cityXCor = city.GetComponent<CityProperties>().xCor;
            int cityZCor = city.GetComponent<CityProperties>().zCor;
            int cityRadius = city.GetComponent<CityProperties>().radius;

            float dist = Mathf.Sqrt(Mathf.Pow(cityXCor - xCor, 2) + Mathf.Pow(cityZCor - zCor, 2));


            if (dist < (radius + cityRadius) * Mathf.Sqrt(2)) {
                freeSpace = false;
            }
        }

        return freeSpace;
    }
}
