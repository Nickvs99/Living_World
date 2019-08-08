using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugging : MonoBehaviour {


    public bool intersectCheck = false;
    public bool cityCheck = false;
    public bool highwayCheck = false;

    public void Update() {
        IntersectCheck();
        DrawCities();
        HighwayCheck();
    }

    public void IntersectCheck() {
        if (intersectCheck) {
            GameObject[] asphaltObj = GameObject.FindGameObjectsWithTag("Asphalt");

            foreach (GameObject obj in asphaltObj) {

                if (obj.GetComponent<General>().intersectPossible == false) {
                    obj.transform.GetComponent<Renderer>().material.color = Color.blue;
                }

            }
        }
    }

    public void DrawCities() {

        // Draws the cities in a white color
        // They get drawn as soon as the cities are created

        if (cityCheck) {
            GameObject[] allObj = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObj) {
                
                // Since I'm lazy I don't want to check if the object even has a child, 
                // therefore it's in a try catch statement
                try {
                    if (obj.GetComponent<General>().city != null) {
                        obj.transform.GetComponent<Renderer>().material.color = Color.white;
                    }
                }
                catch {

                }
            }         
        }
    }

    public void HighwayCheck() {
        if (highwayCheck) {

            GameObject[] asphalts = GameObject.FindGameObjectsWithTag("Asphalt");
            foreach (GameObject obj in asphalts) {
                    if (obj.GetComponent<Asphalt>().highway) {

                        obj.transform.GetComponent<Renderer>().material.color = Color.grey;

                    }
                }
            
        }
    }
}
