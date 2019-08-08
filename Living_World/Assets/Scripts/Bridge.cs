using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour {

    public bool open = false;
    public bool closing = false;
    private int closeCount = 10;         // The time it takes to close a bridge
    public GameObject brug;

    
    
    private List<GameObject> connectedBridges;

    private void Update() {

        if (closing) {
            closeCount -= 1;
            brug.transform.localScale = new Vector3(closeCount / 100f, closeCount / 100f, closeCount / 100f);
        }

        // If the bridge has closed reset variables and set open to true
        if(closeCount == 0) {
            open = true;
            closing = false;
        }
    }


    public void OpenBridge() {
        List<GameObject> bridges = Utility.FindConnectedPieces(gameObject.GetComponent<General>().xCor, gameObject.GetComponent<General>().zCor, "Bridge");
        foreach(GameObject bridge in bridges) {
            bridge.GetComponent<Bridge>().closing = true;
            bridge.GetComponent<Bridge>().closeCount = 100;
        }
        closing = true;
        closeCount = 100;
    }

    public void CloseBridge() {
        gameObject.transform.localScale = new Vector3(1, 1, 1);

    } 
}
