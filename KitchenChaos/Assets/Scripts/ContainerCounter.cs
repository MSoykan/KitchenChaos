using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter {

    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

 
    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            // Has no object on it.
            Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.GetPrefab());
            kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(player);
            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);   
        }
        
    }


}
