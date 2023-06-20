using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : MonoBehaviour {


    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private ClearCounter secondCounter;
    [SerializeField] private bool testing;


    private KitchenObject kitchenObject;

    private void Update() {
        if (testing  && Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("How many times did keycode have been called?");
            if (kitchenObject != null) {
                if (kitchenObject.GetClearCounter().Equals(this)) {
                    Debug.Log("Seeting parent to :" + secondCounter);
                    kitchenObject.SetClearCounter(secondCounter);

                }

            }

        }
    }


    public void Interact() {
        if (kitchenObject == null) {
            // Has no object on it.
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.GetPrefab(), counterTopPoint);
            kitchenObjectTransform.GetComponent<KitchenObject>().SetClearCounter(this);
        }
        else {
            Debug.Log(kitchenObject.GetClearCounter());
        }
    }

    public Transform GetKitchenObjectFollowTransform() {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        return kitchenObject !=null;
    }
}
