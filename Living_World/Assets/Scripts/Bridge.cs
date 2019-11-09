using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour {

    // Time it takes to close a bridge
    public int closeCount = 100;

    public bool occupied = false;

    // Possible states: opening, open, closing and closed
    public string state = "closed";

    // Possible states: none, car and boat
    public string occupied_state = "none";

    // List with object who are on the bridge
    public List<GameObject> objects = new List<GameObject>();

    // List with bridge_child objects
    public List<GameObject> bridge_children = new List<GameObject>();

    private void Update() {

        // opens or closes the bridges depending on its state
        if (state == "opening") {
            closeCount -= 1;
            if (closeCount <= 0) {
                state = "open";
            }
        }
        else if (state == "closing") {
            closeCount += 1;
            if (closeCount >= 100) {
                state = "closed";
            }
        }

        // Set the occupied_state to the tag of the first object
        if (objects.Count == 0) {
            occupied_state = "none";
        }
        else {
            occupied_state = objects[0].tag;
        }

        // Detect if the objects are still on/under the bridge or will be there soon. Removes them is they aren't
        List<GameObject> temp = new List<GameObject>(objects);
        foreach (GameObject obj in objects) {

            //TODO instead of using raycast to get the obj, find the obj using a transform
            RaycastHit hit;
            LayerMask bridge_mask = LayerMask.GetMask("Bridge");
            if (!Physics.Raycast(obj.transform.position + new Vector3(0, 1f, 0), obj.transform.TransformDirection(Vector3.down), out hit, 1, bridge_mask) &&
                !Physics.Raycast(obj.transform.position, obj.transform.TransformDirection(Vector3.forward), out hit, 1, bridge_mask)) {

                temp.Remove(obj);
            }
        }

        objects = temp;
    }
}
