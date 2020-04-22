using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BSplineObjectPlacer/BSplineObjectPool", fileName = "New Object Pool")]
public class BSplineObjectPool : ScriptableObject {

    [SerializeField] BSplineObjectPoolObject[] objects;

    public BSplineObjectPoolObject this[int index] => objects[index];
    public int ObjectCount => objects.Length;

    public IEnumerator<BSplineObjectPoolObject> GetEnumerator () {
        foreach(var obj in objects){
            yield return obj;
        }
    }

    // TODO nullcheck?
    public BSplineObject WeightedRandomObject (System.Random rng = null) {
        if(objects == null || objects.Length <= 0){
            return null;
        }
        var temp = new List<BSplineObject>();
        for(int i=0; i<objects.Length; i++){
            for(int j=0; j<objects[i].Probability; j++){
                temp.Add(objects[i].SourceObject);
            }
        }
        if(rng == null){
            return temp[Random.Range(0, temp.Count)];
        }else{
            return temp[rng.Next(0, temp.Count)];
        }
    }

    [System.Serializable] 
    public class BSplineObjectPoolObject {

        public const int MIN_PROB = 1;
        public const int MAX_PROB = 10;

        [SerializeField] BSplineObject sourceObject;
        [SerializeField, Range(MIN_PROB, MAX_PROB)] int probability = MIN_PROB;

        public BSplineObject SourceObject => sourceObject;
        public int Probability => probability;

    }
	
}
