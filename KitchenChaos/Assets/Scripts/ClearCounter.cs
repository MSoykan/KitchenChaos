using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter {

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            //There is no kitchen object here.
            if (player.HasKitchenObject()) {
                //Player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }else {
                //Player not carying anything
            }
        }
        else {
            //There is a kitchen object
            if(player.HasKitchenObject()) {
                //Player carying something
            }
            else {
                //Player not carying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }


}
