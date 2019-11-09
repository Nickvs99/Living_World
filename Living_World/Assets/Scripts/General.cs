using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class General : MonoBehaviour {
    // This class stores data, which all different states need. 

    public int xCor = -1;
    public int zCor = -1;
    public bool intersectPossible = true;
    public bool CityPossible = true;
    public GameObject city = null;
    public string state;

    public void AddPiece(GameObject newObj) {
        // Adds the script component to the gameObject and instantiates the newObj as child of the gameObject

        GameObject temp = (GameObject)Instantiate(newObj);

        // Set it in hierarchy
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

        foreach (Transform child in gameObject.transform) {
            if (child.name.Contains(name)) {

                return child.gameObject;
            }
        }
        Debug.Log("No object found with, " + name);
        return null;
    }

    public List<GameObject> GetChildren(string name) {
        // Returns all children which contain name in their name

        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in gameObject.transform) {
            if (child.name.Contains(name)) {
                children.Add(child.gameObject);
            }
        }
        if (children.Count == 0) {
            Debug.LogWarning($"No object found with: {name}", gameObject);
        }

        return children;
    }

    public List<GameObject> GetAllChildren() {
        // Returns all children

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in gameObject.transform) {
            children.Add(child.gameObject);
        }

        if (children.Count == 0) {
            Debug.LogWarning("This gameObject does not have any child objects.", gameObject);
        }
        return children;
    }

    public bool ComponentCheck() {
        // Checks for unexpected script components attached to the gameObject. Returns true if there is a duplicate component, else false.

        List<System.Type> componentsTypes = new List<System.Type>();
        bool duplicateFound = false;
        foreach (Component component in gameObject.GetComponents<Component>()) {
            System.Type componentType = component.GetType();
            if (componentsTypes.Contains(componentType)) {
                duplicateFound = true;
                Debug.LogWarning($"{gameObject.name} has a duplicate {componentType} script.", gameObject);
            }
            else {
                componentsTypes.Add(componentType);
            }
        }
        return duplicateFound;

    }

    public bool ChildCheck() {
        // Checks for duplicate childs. Returns ture if a duplicate is found, else false.

        List<string> childNames = new List<string>();
        bool duplicateFound = false;
        foreach (GameObject child in GetAllChildren()) {
            if (childNames.Contains(child.name)) {
                duplicateFound = true;
                Debug.LogWarning($"{gameObject.name} has a duplicate {child.name} child object.");
            }
            else {
                childNames.Add(child.name);
            }
        }
        return duplicateFound;
    }
}
