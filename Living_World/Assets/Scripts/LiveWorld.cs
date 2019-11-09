using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveWorld : MonoBehaviour {
    // Manages the living aspect of the simulation.

    // TODO add time

    public GameObject Boat;
    public GameObject Car;

    int frame = 0;

    private int spawnCarCount = 10;

    void Start() {

        Debug.Log("Lets live");
        gameObject.GetComponent<BuildWorld>().enabled = false;
    }

    void Update() {

        // For every x frames spawn a boat
        if (frame % 40 == 0) {
            SpawnBoat();
        }

        // Start the simulation with n cars
        if (frame == 0) {
            for (int i = 0; i < spawnCarCount; i++) {
                SpawnCar();
            }
        }

        frame += 1;

    }

    void SpawnBoat() {
        // Spawn a boat

        GameObject boat = (GameObject)Instantiate(Boat);
        boat.SetActive(true);
        boat.GetComponent<Boat>().Initialize();
    }

    public void SpawnCar() {
        // Spawn a car

        GameObject car = (GameObject)Instantiate(Car);
        car.SetActive(true);
        car.GetComponent<Car>().Initialize();
    }
}
