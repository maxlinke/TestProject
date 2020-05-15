using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools{

    [CreateAssetMenu(menuName = "Spline Tools/Mesh Pool", fileName = "New MeshPool")]
    public class MeshPool : ObjectPool {

        public const string NO_BOXCOLLS_WARNING = "No BoxCollider on object, even though it is marked to be used!";
        public const string TOO_MANY_BOXCOLLS_WARNING = "Multiple BoxColliders on object, this isn't supported!";

        private const int MAX_DUPLICATE_AVOIDANCE_ITERATIONS = 32;

        [SerializeField] OutputType outputType;
        [SerializeField] RandomRepetitionType randomRepetitionType;
        [SerializeField] MeshPoolObject[] objects;

        public override int ObjectCount => objects.Length;

        List<MeshPoolObject> occurenceList;
        int nextIndex;
        MeshPoolObject lastPO;
        Material lastMat;

        public enum OutputType {
            ORDERED,
            RANDOM
        }

        public enum RandomRepetitionType {
            ALLOW,
            AVOID_DUPLICATE_OBJECT,
            AVOID_DUPLICATE_APPEARANCE
        }

        protected override void Init () {
            base.Init();
            nextIndex = 0;
            lastPO = null;
            lastMat = null;
            FillOccurenceList();
        }

        protected override void DeInit () {
            base.DeInit();
            occurenceList.Clear();
        }

        protected override SplineObject GetNext (Vector3 measureAxis, System.Random rng) {
            if(objects == null || objects.Length <= 0){
                return null;
            }
            MeshPoolObject po;
            Material mat;
            switch(outputType){
                case OutputType.ORDERED:
                    po = objects[nextIndex];
                    mat = po.RandomMaterial(rng);
                    break;
                case OutputType.RANDOM:
                    po = null;
                    mat = null;
                    for(int i=0; i<MAX_DUPLICATE_AVOIDANCE_ITERATIONS; i++){
                        var selectIndex = (rng != null) ? rng.Next(0, occurenceList.Count) : Random.Range(0, occurenceList.Count);
                        var validSelection = false;
                        po = occurenceList[selectIndex];
                        mat = po.RandomMaterial(rng);
                        var samePO = po == lastPO;
                        var sameMat = mat == lastMat;
                        switch(randomRepetitionType){
                            case RandomRepetitionType.ALLOW:
                                validSelection = true;
                                break;
                            case RandomRepetitionType.AVOID_DUPLICATE_APPEARANCE:
                                validSelection = !(samePO && sameMat);
                                break;
                            case RandomRepetitionType.AVOID_DUPLICATE_OBJECT:
                                validSelection = !samePO;
                                break;
                            default:
                                Debug.LogError($"Unknown {nameof(RandomRepetitionType)} \"{randomRepetitionType}\"!");
                                validSelection = true;
                                break;
                        }
                        if(validSelection){
                            break;
                        }
                    }
                    break;
                default:
                    Debug.LogError($"Unknown {typeof(OutputType)} \"{outputType}\"!");
                    return null;
            }
            var output = InstantiatePrefabAndMeasureSize(po.Prefab, mat, measureAxis, po.UseBoxColliderForSpacing);
            nextIndex = (nextIndex + 1) % ObjectCount;
            lastPO = po;
            lastMat = mat;
            return output;
        }

        void FillOccurenceList () {
            if(occurenceList != null && occurenceList.Count > 0){
                Debug.LogWarning("List was called to be filled, even though it wasn't empty. Code will run fine but this shouldn't happen!");
                return;
            }
            if(occurenceList == null){
                occurenceList = new List<MeshPoolObject>();
            }
            List<string> nullPrefabs = new List<string>();
            for(int i=0; i<objects.Length; i++){
                if(objects[i].Prefab == null){
                    nullPrefabs.Add($"Entry #{i}");
                }else{
                    for(int j=0; j<objects[i].Occurences; j++){
                        occurenceList.Add(objects[i]);
                    }
                }
            }
            if(nullPrefabs.Count > 0){
                string problemString = $"{nameof(ObjectPool)} \"{this.name}\" contains {nullPrefabs.Count} Prefab(s) that are null";
                for(int i=0; i<nullPrefabs.Count; i++){
                    problemString += $"\n   - {nullPrefabs[i]}";
                }
                Debug.LogError(problemString);
            }
        }

        public bool CanUseBoxCollForSpacing (GameObject inputGO, out BoxCollider col, out string message) {
            var boxColliders = inputGO.GetComponents<BoxCollider>();
            message = string.Empty;
            col = null;
            if(boxColliders == null || boxColliders.Length <= 0){
                message = NO_BOXCOLLS_WARNING;
            }else if(boxColliders.Length > 1){
                message = TOO_MANY_BOXCOLLS_WARNING;
            }else{
                col = boxColliders[0];
            }
            return col != null;
        }

        protected SplineObject InstantiatePrefabAndMeasureSize (GameObject prefab, Material material, Vector3 measureAxis, bool useBoxColliderForSpacing) {
            var newGO = Instantiate(prefab, Vector3.zero, Quaternion.identity, null);   // do i really want the zero pos and identity rotation?
            float linearSize;
            if(material != null){
                var newGOMR = newGO.GetComponent<MeshRenderer>();
                newGOMR.sharedMaterial = material;
            }
            var canUseBoxColliderForSpacing = CanUseBoxCollForSpacing(newGO, out var col, out var msg);
            if(useBoxColliderForSpacing && !canUseBoxColliderForSpacing){
                Debug.LogError(msg);
            }
            if(useBoxColliderForSpacing && canUseBoxColliderForSpacing){
                var worldColCenter = newGO.transform.TransformPoint(col.center);
                var worldColSize = newGO.transform.TransformVector(col.size);
                var worldRD = -measureAxis;
                var worldRO = worldColCenter - worldRD * worldColSize.magnitude * 2f;
                if(col.Raycast(new Ray(worldRO, worldRD), out var colRayHit, 2f * worldColSize.magnitude)){
                    linearSize = 2f * (worldColCenter - colRayHit.point).magnitude;
                }else{
                    Debug.LogError("No Hit! WHAT?!!?!?");
                    linearSize = float.NaN;
                }
            }else{
                var newGOMF = newGO.GetComponent<MeshFilter>();
                if(newGOMF == null){
                    newGOMF = newGO.GetComponentInChildren<MeshFilter>();
                }
                if(newGOMF == null){
                    Debug.LogError("No MeshFilter to measure the size on!");
                    linearSize = float.NaN;
                }else{
                    var bounds = newGOMF.sharedMesh.bounds;
                    var localDir = newGOMF.transform.InverseTransformVector(measureAxis).normalized;
                    var boundsRO = bounds.center + (localDir * bounds.extents.magnitude * 2f);
                    if(bounds.IntersectRay(new Ray(boundsRO, -localDir), out float boundsHitDist)){
                        var boundsHit = boundsRO - localDir * boundsHitDist;
                        var worldBoundsCenter = newGOMF.transform.TransformPoint(bounds.center);
                        var worldBoundsHit = newGOMF.transform.TransformPoint(boundsHit);
                        linearSize = 2f * (worldBoundsCenter - worldBoundsHit).magnitude;
                    }else{
                        Debug.LogError("No Hit! WHAT?!!?!?");
                        linearSize = float.NaN;
                    }
                }
            }
            return new SplineObject(newGO, linearSize);
        }

        [System.Serializable] 
        public class MeshPoolObject {

            [SerializeField] GameObject prefab;
            [SerializeField, Range(1, 10)] int occurences;
            [SerializeField] Material[] materials;
            [SerializeField] bool useBoxColliderForSpacing;

            public GameObject Prefab => prefab;
            public int MaterialCount => materials.Length;
            public Material Material (int i) => materials[i];
            public int Occurences => occurences;
            public bool UseBoxColliderForSpacing => useBoxColliderForSpacing;

            public Material RandomMaterial (System.Random rng) {
                if(MaterialCount > 0){
                    return materials[(rng != null) ? rng.Next(0, MaterialCount) : Random.Range(0, MaterialCount)];
                }
                rng?.Next();
                return null;
            }

        }
        
    }

    // TODO in editor: make sure occurences is never < 1
    // TODO in editor: when boxcolls not possible, set flag to false, gui.disable the field and add a tooltip

}