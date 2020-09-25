using System.Collections.Generic;
using UnityEngine;

namespace SplineTools{

    [CreateAssetMenu(menuName = "Spline Tools/Mesh Pool", fileName = "New MeshPool")]
    public class MeshPool : ObjectPool {

        private const int MAX_DUPLICATE_AVOIDANCE_ITERATIONS = 32;

        [SerializeField] PlacementOrder placementOrder = PlacementOrder.FixedOrder;
        [SerializeField] RandomRepetition randomRepetition = RandomRepetition.Allow;
        [SerializeField] MeshPoolObject[] objects = default;

        public override int ObjectCount => (objects == null) ? 0 : objects.Length;
        public MeshPoolObject this[int index] => objects[index];

        List<MeshPoolObject> occurenceList;
        int nextIndex;
        MeshPoolObject lastPO;
        Material lastMat;

        public enum PlacementOrder {
            FixedOrder,
            RandomOrder
        }

        public enum RandomRepetition {
            Allow,
            AvoidDuplicateObjects,
            AvoidDuplicateAppearance
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
            switch(placementOrder){
                case PlacementOrder.FixedOrder:
                    po = occurenceList[nextIndex];
                    mat = po.RandomMaterial(rng);
                    break;
                case PlacementOrder.RandomOrder:
                    po = null;
                    mat = null;
                    for(int i=0; i<MAX_DUPLICATE_AVOIDANCE_ITERATIONS; i++){
                        var selectIndex = (rng != null) ? rng.Next(0, occurenceList.Count) : Random.Range(0, occurenceList.Count);
                        var validSelection = false;
                        po = occurenceList[selectIndex];
                        mat = po.RandomMaterial(rng);
                        var samePO = po == lastPO;
                        var sameMat = mat == lastMat;
                        switch(randomRepetition){
                            case RandomRepetition.Allow:
                                validSelection = true;
                                break;
                            case RandomRepetition.AvoidDuplicateAppearance:
                                validSelection = !(samePO && sameMat);
                                break;
                            case RandomRepetition.AvoidDuplicateObjects:
                                validSelection = !samePO;
                                break;
                            default:
                                Debug.LogError($"Unknown {nameof(RandomRepetition)} \"{randomRepetition}\"!");
                                validSelection = true;
                                break;
                        }
                        if(validSelection){
                            break;
                        }
                    }
                    break;
                default:
                    Debug.LogError($"Unknown {typeof(PlacementOrder)} \"{placementOrder}\"!");
                    return null;
            }
            var output = InstantiatePrefabAndMeasureSize(po, mat, measureAxis);
            nextIndex = (nextIndex + 1) % occurenceList.Count;
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

        protected SplineObject InstantiatePrefabAndMeasureSize (MeshPoolObject mpo, Material material, Vector3 measureAxis) {
            var newGO = Instantiate(mpo.Prefab, Vector3.zero, Quaternion.identity, null);   // do i really want the zero pos and identity rotation?
            float linearSize;
            if(material != null){
                var newGOMR = newGO.GetComponent<MeshRenderer>();
                newGOMR.sharedMaterial = material;
            }
            var canUseBoxColliderForSpacing = mpo.CanUseBoxCollForSpacing(out var col, out var msg);
            if(mpo.UseBoxColliderForSpacing && !canUseBoxColliderForSpacing){
                Debug.LogError(msg);
            }
            if(mpo.UseBoxColliderForSpacing && canUseBoxColliderForSpacing){
                var worldColCenter = newGO.transform.TransformPoint(col.center);
                var worldColSize = newGO.transform.TransformVector(col.size);
                var worldRD = -measureAxis;
                var worldRO = worldColCenter - worldRD * worldColSize.magnitude * 2f;
                if(col.Raycast(new Ray(worldRO, worldRD), out var colRayHit, 2f * worldColSize.magnitude)){
                    linearSize = 2f * (worldColCenter - colRayHit.point).magnitude;
                }else{
                    Debug.LogError("No Hit! WHAT?!!?!?");
                    linearSize = 0f;
                }
            }else{
                Bounds bounds;
                Transform boundsTransform;
                if(mpo.UseCustomBounds){
                    bounds = mpo.CustomBounds;
                    boundsTransform = newGO.transform;
                }else{
                    var newGOMF = GetNewGOMeshFilter();
                    if(newGOMF == null){
                        Debug.LogError($"No {nameof(MeshFilter)} on \"{newGO.name}\" to measure the size of!");
                        bounds = new Bounds(Vector3.zero, Vector3.one * 0.01f);
                        boundsTransform = newGO.transform;
                    }else{
                        bounds = newGOMF.sharedMesh.bounds;
                        boundsTransform = newGOMF.transform;
                    }
                }
                var localDir = boundsTransform.transform.InverseTransformVector(measureAxis).normalized;
                var boundsRO = bounds.center + (localDir * bounds.extents.magnitude * 2f);
                if(bounds.IntersectRay(new Ray(boundsRO, -localDir), out float boundsHitDist)){
                    var boundsHit = boundsRO - localDir * boundsHitDist;
                    var worldBoundsCenter = boundsTransform.transform.TransformPoint(bounds.center);
                    var worldBoundsHit = boundsTransform.transform.TransformPoint(boundsHit);
                    linearSize = 2f * (worldBoundsCenter - worldBoundsHit).magnitude;
                }else{
                    Debug.LogError("No Hit! WHAT?!!?!?");
                    linearSize = 0f;
                }
            }
            return new SplineObject(newGO, linearSize);

            MeshFilter GetNewGOMeshFilter () {
                var newGOMF = newGO.GetComponent<MeshFilter>();
                if(newGOMF == null){
                    newGOMF = newGO.GetComponentInChildren<MeshFilter>();
                }
                return newGOMF;
            }
        }

        public void DeleteObject (int deleteIndex) {
            var tempList = new List<MeshPoolObject>();
            for(int i=0; i<ObjectCount; i++){
                if(i == deleteIndex){
                    continue;
                }
                tempList.Add(objects[i]);
            }
            objects = tempList.ToArray();
        }

        public void AddObject () {
            var tempList = new List<MeshPoolObject>();
            for(int i=0; i<ObjectCount; i++){
                tempList.Add(objects[i]);
            }
            tempList.Add(new MeshPoolObject());
            objects = tempList.ToArray();
        }
        
    }

}