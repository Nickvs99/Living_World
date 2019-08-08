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
    
    public List<object> CreateCopyList() {

        List<object> copyData = new List<object>();
        copyData.Add(xCor);
        copyData.Add(zCor);
        copyData.Add(intersectPossible);
        copyData.Add(CityPossible);
        copyData.Add(city);
        

        return copyData;

    }
    public void CopyData(List<object> list, string tag) {

        gameObject.tag = tag;
        state = tag;

        xCor = (int)list[0];
        zCor = (int)list[1];
        intersectPossible = (bool)list[2];
        CityPossible = (bool)list[3];
        city = (GameObject)list[4];

        gameObject.transform.position = new Vector3(xCor + 0.5f, 0, zCor + 0.5f);
    }
}
