using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCallBack : MonoBehaviour {

    private bool isFirstUpdate = true;


    private void Update() {
        if(isFirstUpdate) {
            isFirstUpdate = false;

            Loader.LoaderCallBack();
        }
        Debug.Log("Kaç kez run etti?");
    }

}
