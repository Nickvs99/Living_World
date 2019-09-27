using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveWorld : MonoBehaviour {

    public GameObject Boat;
    public GameObject Car;
    int frame = 0;

    void Start () {
        Debug.Log("Lets live");
        gameObject.GetComponent<BuildWorld>().enabled = false;

    }
    
    void Update () {

        // For every x frames spawn a boat
        if(frame % 40 == 0) {
            SpawnBoat();
        }

        
        if(frame == 0) {
            for(int i = 0; i < 10; i++) {
                SpawnCar();
            }
        }

        frame += 1;
		
	}

    void SpawnBoat() {

        GameObject boat = (GameObject)Instantiate(Boat);
        boat.SetActive(true);
        boat.GetComponent<Boat>().Initialize();
        Debug.Log("Spawn Boat");

    }

    public void SpawnCar() {

        GameObject car = (GameObject)Instantiate(Car);
        car.SetActive(true);
        car.GetComponent<Car>().Initialize();

    }
}
