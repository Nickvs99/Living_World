using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {
    // Moves the camera

    public void Initialize() {
        // Set camera to starting position

        float height;
        if (BuildWorld.sizeX > BuildWorld.sizeZ) {
            height = BuildWorld.sizeX;
        }
        else {
            height = BuildWorld.sizeZ;
        }

        gameObject.transform.position = new Vector3(BuildWorld.sizeX / 2, height, BuildWorld.sizeZ / 2);
    }

    private void Update() {
        // Move Camera depending on input

        // Moveup
        if (Input.GetKey("w")) {
            gameObject.transform.position += new Vector3(0, 0, 1) * (transform.position[1] * 0.1f);
        }

        // Move left
        if (Input.GetKey("a")) {
            gameObject.transform.position += new Vector3(-1, 0, 0) * (transform.position[1] * 0.1f);
        }

        // Move down
        if (Input.GetKey("s")) {
            gameObject.transform.position += new Vector3(0, 0, -1) * (transform.position[1] * 0.1f);
        }

        // Move right
        if (Input.GetKey("d")) {
            gameObject.transform.position += new Vector3(1, 0, 0) * (transform.position[1] * 0.1f);
        }

        // Zoom in
        if (Input.GetKey("o")) {
            gameObject.transform.position += new Vector3(0, -1, 0);
        }

        // Zoom out
        if (Input.GetKey("k")) {
            gameObject.transform.position += new Vector3(0, 1, 0);
        }
    }
}
