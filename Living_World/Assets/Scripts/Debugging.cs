using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugging : MonoBehaviour {
    // Class is responsible for visualing several variables. It shows cities, allowed intersections, highways, 
    // and duplicate script and children on a object.


    public bool intersectCheck = false;
    public bool cityCheck = false;
    public bool highwayCheck = false;
    public bool _buildCheck = false;
    public static bool buildCheck;

    public void Start() {
        buildCheck = _buildCheck;
    }

    public void Update() {
        IntersectCheck();
        DrawCities();
        HighwayCheck();
    }

    public void IntersectCheck() {
        // Colors asphalt pieces which can't have an intersection blue

        if (!intersectCheck) {
            return;
        }

        GameObject[] asphaltObj = GameObject.FindGameObjectsWithTag("Asphalt");

        foreach (GameObject obj in asphaltObj) {

            if (obj.GetComponent<General>().intersectPossible == false) {

                List<GameObject> children = obj.GetComponent<General>().GetChildren("Asphalt");
                ColorChildren(children, Color.blue);
            }
        }
    }

    public void DrawCities() {
        // Draws the cities in a white color

        if (!cityCheck) {
            return;
        }

        foreach (GameObject obj in BuildWorld.gridObjects) {

            if (obj.GetComponent<General>().city != null) {
                ColorChildren(obj.GetComponent<General>().GetAllChildren(), Color.white);
            }
        }
    }

    public void HighwayCheck() {
        // Colors highway objects black

        if (!highwayCheck) {
            return;
        }

        GameObject[] asphalts = GameObject.FindGameObjectsWithTag("Asphalt");
        foreach (GameObject obj in asphalts) {
            if (obj.GetComponent<Asphalt>().highway) {

                ColorChildren(obj.GetComponent<General>().GetAllChildren(), Color.black);

            }
        }
    }

    public static void ChildCheck() {
        // Checks for duplicate child objects. Returns true when a duplicate
        // is found, else False.

        if (!buildCheck) {
            return;
        }

        foreach (GameObject obj in BuildWorld.gridObjects) {
            if (obj.GetComponent<General>().ChildCheck()) {
                ColorChildren(obj.GetComponent<General>().GetAllChildren(), Color.yellow);
            }
        }
    }

    public static void ComponentCheck() {
        // Checks for duplicate script components. Returns true when a duplicate is found, else false.
        if (!buildCheck) {
            return;
        }
        foreach (GameObject obj in BuildWorld.gridObjects) {
            if (obj.GetComponent<General>().ComponentCheck()) {
                ColorChildren(obj.GetComponent<General>().GetAllChildren(), Color.yellow);
            }
        }
    }

    private static void ColorChildren(List<GameObject> children, Color newColor) {
        // Sets Renderer.material.color to newColor for all children.

        foreach (GameObject child in children) {
            if (child.GetComponent<Renderer>() != null) {
                child.transform.GetComponent<Renderer>().material.color = newColor;
            }
        }
    }
}
