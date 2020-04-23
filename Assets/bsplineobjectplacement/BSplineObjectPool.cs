using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BSplineObjectPlacer/BSplineObjectPool", fileName = "New Object Pool")]
public class BSplineObjectPool : ScriptableObject {

    [SerializeField] BSplineObjectPoolObject[] objects;

    public BSplineObjectPoolObject this[int index] => objects[index];
    public int ObjectCount => objects.Length;

    public bool gotCachedList => weightedProbabilityList != null && weightedProbabilityList.Count > 0;
    List<BSplineObject> weightedProbabilityList;

    public IEnumerator<BSplineObjectPoolObject> GetEnumerator () {
        foreach(var obj in objects){
            yield return obj;
        }
    }

    // prevents caching and never releasing...
    public void CacheWeightedArrayAndDoAction (System.Action actionToDo) {
        FillWeightedProbabiltyList();
        actionToDo();
        weightedProbabilityList.Clear();
    }

    private void FillWeightedProbabiltyList () {
        if(weightedProbabilityList != null && weightedProbabilityList.Count > 0){
            Debug.LogWarning("List was called to be filled, even though it wasn't empty. Code will run fine but this shouldn't happen!");
            return;
        }
        if(weightedProbabilityList == null){
            weightedProbabilityList = new List<BSplineObject>();
        }
        List<string> nullObjects = new List<string>();
        List<string> nullPrefabs = new List<string>();
        for(int i=0; i<objects.Length; i++){
            if(objects[i].SourceObject == null){
                nullObjects.Add($"Entry #{i}");
                continue;
            }
            if(objects[i].SourceObject.Prefab == null){
                nullPrefabs.Add(objects[i].SourceObject.name);
                continue;
            }
            for(int j=0; j<objects[i].Probability; j++){
                weightedProbabilityList.Add(objects[i].SourceObject);
            }
        }
        if(nullObjects.Count > 0 || nullPrefabs.Count > 0){
            string problemString = string.Empty;
            if(nullObjects.Count > 0){
                problemString += $"{nullObjects.Count} {nameof(BSplineObject)}(s) that were null";
                for(int i=0; i<nullObjects.Count; i++){
                    problemString += $"\n   - {nullObjects[i]}";
                }
            }
            if(nullPrefabs.Count > 0){
                problemString += (nullObjects.Count > 0 ? "\nand " : string.Empty) + $"{nullPrefabs.Count} Prefab(s) that were null";
                for(int i=0; i<nullPrefabs.Count; i++){
                    problemString += $"\n   - {nullPrefabs[i]}";
                }
            }
            Debug.LogError($"{nameof(BSplineObjectPool)} \"{this.name}\" has problems:\n{problemString}.");
        }
        Debug.Log("filled list");
    }

    public BSplineObject WeightedRandomObject (System.Random rng = null) {
        if(objects == null || objects.Length <= 0){
            return null;
        }
        bool clearListAtEnd = false;
        if(!gotCachedList){
            FillWeightedProbabiltyList();
            clearListAtEnd = true;
        }
        BSplineObject output;
        if(rng == null){
            output = weightedProbabilityList[Random.Range(0, weightedProbabilityList.Count)];
        }else{
            output = weightedProbabilityList[rng.Next(0, weightedProbabilityList.Count)];
        }
        if(clearListAtEnd){
            weightedProbabilityList.Clear();
        }
        return output;
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
