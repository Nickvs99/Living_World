using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour {
    // Manages a boat instance

    private List<Vector3> RiverPoints;
    public GameObject centerHub, boatHier;
    public int lifetime = 0;
    public float speed = 1.0f;
    public int xCor, zCor;

    public bool moving = true;

    public void Initialize() {
        // Inititialeze object.

        gameObject.transform.parent = boatHier.transform;

        // Get the path
        int r = (int)Random.Range(0, 2);
        if (r == 0) {
            RiverPoints = centerHub.GetComponent<BuildWorld>().River0;
        }
        else {
            RiverPoints = centerHub.GetComponent<BuildWorld>().River1;
        }

        // The boat gameobject begins at the start of the river.
        // Start at index 1 since index 0 does not have a width, therefore they would spawn in the middle of the river and not to one side
        gameObject.transform.position = RiverPoints[1];

        // If the spawn location is occupied, destroy gameobject
        if (!checkFreeSpace()) {
            Destroy(gameObject);
        }

    }

    void Update() {

        if (checkFreeSpace()) {

            float distLeft = speed;

            // As long as the distLeft is greater than the distance between currentPos and nextPos keep updating
            // and the lifetime of the boat is less than the number of riverpoints
            while (lifetime < RiverPoints.Count - 1 && distLeft > Vector3.Distance(transform.position, RiverPoints[lifetime + 1])) {

                distLeft -= Vector3.Distance(transform.position, RiverPoints[lifetime + 1]);
                transform.position = RiverPoints[lifetime + 1];
                lifetime += 1;
            }

            // The remainder of distLeft is added to the currentPos
            if (lifetime < RiverPoints.Count - 1) {
                float dist = Vector3.Distance(transform.position, RiverPoints[lifetime + 1]);
                float perc = distLeft / dist;

                transform.position = Vector3.Lerp(transform.position, RiverPoints[lifetime + 1], perc);
                transform.LookAt(RiverPoints[lifetime + 1]);

            }
            else {
                Destroy(gameObject);
            }
        }
    }

    bool checkFreeSpace() {
        // Returns true if there is enough room for the boat to travel into.

        bool freeSpace = true;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 1, LayerMask.GetMask("Bridge"))) {
            // When a bridge object is hit, the boat behaves differently for the four possible states from the bridge:
            //      open: The boat can move forward
            //      closed: The boat can't move forward. If there is no car on the bridge, open the bridge.
            //      opening: The boat has to wait until the bridge is open.
            //      closing: The boat has to wait until the bridge is closed. The cars are then allowed to drive over the bridge and then the bridge opens.

            GameObject obj = hit.transform.parent.gameObject;
            GameObject bridge_main = obj.GetComponent<Bridge_child>().bridge_main;

            if (bridge_main.GetComponent<Bridge>().state == "open") {
                if (!bridge_main.GetComponent<Bridge>().objects.Contains(gameObject)) {

                    bridge_main.GetComponent<Bridge>().objects.Add(gameObject);
                }
            }
            else if (bridge_main.GetComponent<Bridge>().state == "closed") {
                if (bridge_main.GetComponent<Bridge>().occupied_state != "Car") {

                    bridge_main.GetComponent<Bridge>().state = "opening";
                }
                return false;
            }
            else if (bridge_main.GetComponent<Bridge>().state == "opening") {
                if (!bridge_main.GetComponent<Bridge>().objects.Contains(gameObject)) {

                    bridge_main.GetComponent<Bridge>().objects.Add(gameObject);
                }
                return false;
            }
            else if (bridge_main.GetComponent<Bridge>().state == "closing") {

                return false;
            }
        }

        // When a boat is detected, it cannot move forward.
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 1, LayerMask.GetMask("Boat"))) {

            return false;
        }
        return freeSpace;
    }
}
