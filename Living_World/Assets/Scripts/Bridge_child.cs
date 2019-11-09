﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_child : MonoBehaviour {
    // This class manages the visual look of the bridge.

    public GameObject bridge_visual, bridge_main;

    void Update() {

        int x = bridge_main.GetComponent<Bridge>().closeCount;
        bridge_visual.transform.localScale = new Vector3(x / 100f, x / 100f, x / 100f);

    }
}
