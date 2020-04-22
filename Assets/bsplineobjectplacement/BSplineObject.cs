using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BSplineObjectPlacer/BSplineObject", fileName = "New Object")]
public class BSplineObject : ScriptableObject {

    [SerializeField] GameObject prefab;
    [SerializeField] Material[] materials;

    public GameObject Prefab => prefab;
    public Material this[int index] => materials[index];
    public int MaterialCount => materials.Length;

    public Material RandomMaterial (System.Random rng = null) {
        if(materials == null || materials.Length <= 0){
            return null;
        }
        if(rng == null){
            return materials[Random.Range(0, MaterialCount)];
        }else{
            return materials[rng.Next(0, MaterialCount)];
        }
    }
	
}
