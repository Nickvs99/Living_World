using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_child : MonoBehaviour{

    public GameObject bridge_visual, bridge_main;
    
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        int x = bridge_main.GetComponent<Bridge>().closeCount;
        bridge_visual.transform.localScale = new Vector3(x / 100f, x / 100f, x / 100f);

    }
}
