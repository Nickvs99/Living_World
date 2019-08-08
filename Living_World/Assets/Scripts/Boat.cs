using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour {

    private List<Vector3> RiverPoints;
    public GameObject centerHub, boatHier;
    public int lifetime = 0;
    public float speed = 1.0f;
    public int xCor, zCor;

    public bool moving = true;

    // Use this for initialization
    public void Initialize() {
        gameObject.transform.parent = boatHier.transform;

        int r = (int)Random.Range(0, 2);
        if(r == 0) {
            RiverPoints = centerHub.GetComponent<BuildWorld>().River0;
        }
        else {
            RiverPoints = centerHub.GetComponent<BuildWorld>().River1;
        }

        // The boat gameobject begings at the start of the river, index 1 since index 0 does not have a width, therefore they would spawn in the middle of the river and not to one side
        gameObject.transform.position = RiverPoints[1];

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
            }
            else {
                Destroy(gameObject);
            }
        }
    }



    bool checkFreeSpace() {
        bool freeSpace = true;
        Vector3 dir = RiverPoints[lifetime + 1] - transform.position;
        float mag = Vector3.Magnitude(dir);
        Vector3 normDir = dir / mag;

        Debug.DrawRay(transform.position, normDir , Color.red);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, normDir, out hit, 1)) {
            GameObject obj = hit.transform.gameObject;
            if (obj.tag == "Bridge" && obj.GetComponent<Bridge>().open == false) {
                if (!obj.GetComponent<Bridge>().closing) {

                    obj.GetComponent<Bridge>().OpenBridge();
                }
                freeSpace = false;
            }

            if (obj.tag == "Boat") {
                freeSpace = false;
            }
        }
        return freeSpace;
    }
}
