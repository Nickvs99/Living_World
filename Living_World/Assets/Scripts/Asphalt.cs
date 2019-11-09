using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asphalt : MonoBehaviour {
    // This class stores information about the asphalt. It also gives it the right visual
    // depending on its surroundings.

    public bool highway = false;
    public float speed_limit = 0.1f;

    public void Start() {

        //Adds the colliders used for car navitagion.
        gameObject.GetComponent<General>().AddPiece(GameObject.Find("/States/Asphalt_objects/Asphalt_colliders"));
    }

    public void Specialize() {
        // Give the asphalt object the right look, depending on its surrounding objects.

        //Remove default asphalt
        gameObject.GetComponent<General>().RemovePiece("Asphalt_default");

        List<GameObject> surround = Utility.SurroundingObjectsPlus(gameObject.GetComponent<General>().xCor, gameObject.GetComponent<General>().zCor, 1);

        // Count the number of neighbouring drive-able objects
        int asphalt_neighbours_count = 0;
        foreach (GameObject obj in surround) {
            if (Check_drive_able(obj)) {
                asphalt_neighbours_count += 1;
            }
        }

        if (asphalt_neighbours_count == 1) {
            // End piece

            gameObject.GetComponent<General>().AddPiece(GameObject.Find("/States/Asphalt_objects/Asphalt_end"));

            // Rotate piece
            if (Check_drive_able(Utility.GetNeighbour(gameObject, "up"))) {
                gameObject.GetComponent<General>().GetChild("Asphalt_end").transform.Rotate(0, 0, 90);

            }
            else if (Check_drive_able(Utility.GetNeighbour(gameObject, "left"))) {
                gameObject.GetComponent<General>().GetChild("Asphalt_end").transform.Rotate(0, 0, 180);

            }
            else if (Check_drive_able(Utility.GetNeighbour(gameObject, "down"))) {
                gameObject.GetComponent<General>().GetChild("Asphalt_end").transform.Rotate(0, 0, 270);

            }
        }

        else if (asphalt_neighbours_count == 2) {

            // Straight
            if (Check_drive_able(Utility.GetNeighbour(gameObject, "left")) && Check_drive_able(Utility.GetNeighbour(gameObject, "right")) ||
                Check_drive_able(Utility.GetNeighbour(gameObject, "down")) && Check_drive_able(Utility.GetNeighbour(gameObject, "up"))) {

                gameObject.GetComponent<General>().AddPiece(GameObject.Find("/States/Asphalt_objects/Asphalt_straight"));

                if (Check_drive_able(Utility.GetNeighbour(gameObject, "down"))) {

                    gameObject.GetComponent<General>().GetChild("Asphalt_straight").transform.Rotate(0, 0, 90);
                }
            }

            // Corner
            else {

                gameObject.GetComponent<General>().AddPiece(GameObject.Find("/States/Asphalt_objects/Asphalt_corner"));

                if (Check_drive_able(Utility.GetNeighbour(gameObject, "up"))) {
                    if (Check_drive_able(Utility.GetNeighbour(gameObject, "left"))) {
                        gameObject.GetComponent<General>().GetChild("Asphalt_corner").transform.Rotate(0, 0, 90);
                    }
                }

                else if (Check_drive_able(Utility.GetNeighbour(gameObject, "down"))) {
                    if (Check_drive_able(Utility.GetNeighbour(gameObject, "left"))) {
                        gameObject.GetComponent<General>().GetChild("Asphalt_corner").transform.Rotate(0, 0, 180);
                    }
                    else {
                        gameObject.GetComponent<General>().GetChild("Asphalt_corner").transform.Rotate(0, 0, 270);
                    }
                }
            }
        }

        else if (asphalt_neighbours_count == 3) {
            // T-junction
            // Searches for the neighbour which isn't drive-able and rotates accordingly.

            gameObject.GetComponent<General>().AddPiece(GameObject.Find("/States/Asphalt_objects/Asphalt_t-junction"));


            if (!Check_drive_able(Utility.GetNeighbour(gameObject, "down"))) {

                gameObject.GetComponent<General>().GetChild("Asphalt_t-junction").transform.Rotate(0, 0, 90);
            }
            else if (!Check_drive_able(Utility.GetNeighbour(gameObject, "right"))) {

                gameObject.GetComponent<General>().GetChild("Asphalt_t-junction").transform.Rotate(0, 0, 180);
            }
            else if (!Check_drive_able(Utility.GetNeighbour(gameObject, "up"))) {

                gameObject.GetComponent<General>().GetChild("Asphalt_t-junction").transform.Rotate(0, 0, 270);
            }
        }

        else if (asphalt_neighbours_count == 4) {
            // Cross-intersection

            gameObject.GetComponent<General>().AddPiece(GameObject.Find("/States/Asphalt_objects/Asphalt_cross-intersection"));
        }
    }

    private bool Check_drive_able(GameObject obj) {
        // Checks if the obj is drive-able (i.e. Asphalt or Bridge).

        if (obj == null) {
            return false;
        }
        else {
            return (obj.tag == "Asphalt" || obj.tag == "Bridge");
        }
    }
}
