using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject {


    [SerializeField] private Transform prefab;
    public Sprite sprite;
    public string objectName;

    public Transform GetPrefab() {
        return prefab;
    }
}
