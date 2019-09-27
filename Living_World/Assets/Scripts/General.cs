using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour {

    public int xCor = -1;
    public int zCor = -1;
    public bool intersectPossible = true;
    public bool CityPossible = true;
    public GameObject city;
    public string state;

    public void AddPiece(GameObject newObj) {
        // Adds the script component to the gameObject and instantiates the newObj as child of the gameObject

        GameObject temp = (GameObject)Instantiate(newObj);
        
        temp.transform.position = gameObject.transform.position;
        temp.transform.parent = gameObject.transform;

        //Adds the script from the instatiated object
        if (newObj.name.Contains("Asphalt_default")) {
            gameObject.AddComponent<Asphalt>();
        }
        else if (newObj.name == "Bridge") {
            gameObject.AddComponent<Bridge_child>();
            gameObject.GetComponent<Bridge_child>().bridge_visual = temp;
            temp.transform.position += new Vector3(0, 0.01f, 0);
            AddPiece(GameObject.Find("Bridge_collider"));
        }
        else if (newObj.name == "Grass") {
            gameObject.AddComponent<Grass>();
        }
        else if (newObj.name == "House") {
            gameObject.AddComponent<House>();
        }
        else if (newObj.name == "Tree") {
            gameObject.AddComponent<Tree>();
        }
        else if (newObj.name == "Water") {
            gameObject.AddComponent<Water>();
        }
    }

    public void RemovePiece(params string[] args) {
        // Destroys the gameObjects child if the child's name contains any of the specified strings in args.
        // If the first element from args is "all", then all childs will be destroyed.

        foreach (Transform child in gameObject.transform) {
            if (args[0] == "all") {
                Destroy(child.gameObject);
            }
            else {
                foreach (string state in args) {
                    if (child.name.Contains(state)) {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }

    public GameObject GetChild(string name) {
        // Returns the first found child which contains name in its name, 
        // if no child is found, then return null.

        foreach(Transform child in gameObject.transform) {
            if(child.name.Contains(name)) {

                return child.gameObject;
            }
        }
        Debug.Log("No object found with, " + name);
        return null;
    }
}
