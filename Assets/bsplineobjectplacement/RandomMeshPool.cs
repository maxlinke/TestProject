using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    [CreateAssetMenu(menuName = "BSplineObjectPlacer/Random Mesh Pool", fileName = "New RandomMeshPool")]
    public class RandomMeshPool : ObjectPool {

        private const int MAX_DUPLICATE_AVOIDANCE_ITERATIONS = 10;

        [SerializeField] DuplicateMode duplicateMode;
        [SerializeField] RandomPoolObject[] objects;

        public RandomPoolObject this[int index] => objects[index];
        public bool gotCachedList => weightedProbabilityList != null && weightedProbabilityList.Count > 0;
        List<RandomMeshPoolObject> weightedProbabilityList;

        public override int ObjectCount => objects.Length;

        private RandomMeshPoolObject lastPO;
        private Material lastMaterial;

        public enum DuplicateMode {
            ALLOW,
            AVOID_DUPLICATE_OBJECT,
            AVOID_DUPLICATE_APPEARANCE
        }

        protected override void Init () {
            FillWeightedProbabiltyList();
            lastPO = null;
            lastMaterial = null;
        }

        protected override void DeInit () {
            weightedProbabilityList.Clear();
            lastPO = null;
            lastMaterial = null;

        }

        protected override SplineObject GetNext (Vector3 measureAxis, System.Random rng) {
            if(objects == null || objects.Length <= 0){
                return null;
            }
            bool clearListAtEnd = false;
            if(!gotCachedList){
                FillWeightedProbabiltyList();
                clearListAtEnd = true;
            }
            RandomMeshPoolObject selectedPO = null;
            Material selectedMat = null;
            for(int i=0; i<MAX_DUPLICATE_AVOIDANCE_ITERATIONS; i++){
                var selectIndex = (rng == null) ? Random.Range(0, weightedProbabilityList.Count) : rng.Next(0, weightedProbabilityList.Count);
                
                selectedPO = weightedProbabilityList[selectIndex];
                selectedMat = selectedPO.RandomMaterial(rng);

                bool samePO = selectedPO == lastPO;
                bool sameMat = selectedMat == lastMaterial;
                bool validSelection = false;
                switch(duplicateMode){
                    case DuplicateMode.ALLOW:
                        validSelection = true;
                        break;
                    case DuplicateMode.AVOID_DUPLICATE_APPEARANCE:
                        validSelection = !(samePO && sameMat);
                        break;
                    case DuplicateMode.AVOID_DUPLICATE_OBJECT:
                        validSelection = !samePO;
                        break;
                    default:
                        Debug.LogError($"Unknown {nameof(DuplicateMode)} \"{duplicateMode}\"! Code will run fine(ish) but FIX IT!!!");
                        validSelection = true;
                        break;
                }
                if(validSelection){
                    break;
                }
            }
            if(clearListAtEnd){
                weightedProbabilityList.Clear();
            }
            lastPO = selectedPO;
            lastMaterial = selectedMat;
            var newGO = selectedPO.CreateSelfAndMeasureSize(selectedMat, measureAxis, out var linearSize);
            return new SplineObject(newGO, linearSize);
        }

        public IEnumerator<RandomPoolObject> GetEnumerator () {
            foreach(var obj in objects){
                yield return obj;
            }
        }

        private void FillWeightedProbabiltyList () {
            if(weightedProbabilityList != null && weightedProbabilityList.Count > 0){
                Debug.LogWarning("List was called to be filled, even though it wasn't empty. Code will run fine but this shouldn't happen!");
                return;
            }
            if(weightedProbabilityList == null){
                weightedProbabilityList = new List<RandomMeshPoolObject>();
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
                    problemString += $"{nullObjects.Count} {nameof(RandomMeshPoolObject)}(s) that were null";
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
                Debug.LogError($"{nameof(ObjectPool)} \"{this.name}\" has problems:\n{problemString}.");
            }
        }

        [System.Serializable] 
        public class RandomPoolObject {

            public const int MIN_PROB = 1;
            public const int MAX_PROB = 10;

            [SerializeField] RandomMeshPoolObject sourceObject;
            [SerializeField, Range(MIN_PROB, MAX_PROB)] int probability = MIN_PROB;

            public RandomMeshPoolObject SourceObject => sourceObject;
            public int Probability => probability;

        }
    
    }

}