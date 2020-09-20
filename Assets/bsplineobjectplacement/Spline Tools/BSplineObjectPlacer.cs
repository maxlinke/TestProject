using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class BSplineObjectPlacer : MonoBehaviour {

        private const int MAX_PLACE_LOOP_COUNT = 1000;

        [Header("Spline")]
        [SerializeField] BezierSpline spline;

        [Header("Randomness")]
        [SerializeField] int randomSeed;
        [SerializeField] Vector3 placementRandomness;
        [SerializeField] float rotationRandomness;

        [Header("Object Settings")]
        [SerializeField] ObjectPool objectPool;
        [SerializeField] float spaceBetweenObjects;
        [SerializeField] float universalRotationOffset;
        [SerializeField] Vector3 universalPositionOffset;

        [Header("Placement Settings")]
        [SerializeField] DistanceMode distanceMode;
        [SerializeField] GroundMode groundMode;
        [SerializeField] Collider groundCollider;
        [SerializeField] bool noOvershoot;

        public enum DistanceMode {
            EUCLIDIAN,
            BEZIER
        }

        public enum GroundMode {
            DISABLED,
            SNAP,
            SNAP_AND_ALIGN
        }

        public void RandomizeSeed () {
            int hash = Random.value.GetHashCode();
            int loopCount = Mathf.Abs(hash)%64;
            for(int i=0; i<loopCount; i++){
                hash += Random.value.GetHashCode();
            }
            UpdateSeed(hash);
        }

        public void UpdateSeed (int newSeed) {
            BuildSafeUndo.RecordObject(this, "Update random seed");
            this.randomSeed = newSeed;
            ConditionalReplace();
        }

        public void UpdateRandomizationSettings (Vector3 newPlacementRandomness,  float newRotationRandomness) {
            BuildSafeUndo.RecordObject(this, "Update randomization settings");
            this.placementRandomness = newPlacementRandomness;
            this.rotationRandomness = newRotationRandomness;
            ConditionalReplace();
        }

        public void UpdateObjectSettings (ObjectPool newPool, float newSpaceBetweenObjects) {
            BuildSafeUndo.RecordObject(this, "Update object settings");
            this.objectPool = newPool;
            this.spaceBetweenObjects = newSpaceBetweenObjects;
            ConditionalReplace();
        }

        public void UpdatePlacementSettings (DistanceMode newDistanceMode, GroundMode newGroundMode, Collider newGroundCollider, bool newOvershootMode) {
            BuildSafeUndo.RecordObject(this, "Update placement settings");
            this.distanceMode = newDistanceMode;
            this.groundMode = newGroundMode;
            this.groundCollider = newGroundCollider;
            this.noOvershoot = newOvershootMode;
            ConditionalReplace();
        }

        public void ReverseDirection () {
            BuildSafeUndo.RecordObject(this, "Reverse Direction");
            if(spline != null){
                spline.ReverseDirection();
            }
            universalRotationOffset = Mathf.Repeat(universalRotationOffset + 180f, 360f);
            universalPositionOffset = new Vector3(-universalPositionOffset.x, universalPositionOffset.y, -universalPositionOffset.z);
            ConditionalReplace();
        }

        public void Rotate90Deg () {
            BuildSafeUndo.RecordObject(this, "Rotate 90 deg");
            universalRotationOffset = Mathf.Repeat(universalRotationOffset + 90f, 360f);
            ConditionalReplace();
        }

        public void DeletePlacedObjects () {
            for(int i=transform.childCount-1; i>=0; i--){
                #if UNITY_EDITOR
                Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
                #else
                Destroy(transform.GetChild(i).gameObject);
                #endif
            }
        }

        void ConditionalReplace () {
            if(transform.childCount > 0){
                PlaceObjects();
            }
        }

        public void PlaceObjects () {
            DeletePlacedObjects();
            
            if(spline == null){
                spline = GetComponent<BezierSpline>();
                if(spline == null){
                    Debug.LogWarning("No spline assigned!", this.gameObject);
                    return;
                }
            }
            if(spline.Length == 0f){
                Debug.LogWarning("Spline length is zero!", this.gameObject);
                return;
            }
            if(objectPool == null){
                Debug.LogWarning("Pool is null!", this.gameObject);
                return;
            }
            if(objectPool.ObjectCount == 0){
                Debug.LogWarning($"Pool is empty ({objectPool.name})!", this.gameObject);
                return;
            }

            if(spline.gameObject == this.gameObject){
                spline.ApplyScale();
            }
            transform.localScale = Vector3.one;

            System.Random poolRNG = new System.Random(randomSeed);
            System.Random splineRNG = new System.Random(randomSeed);
            bool terminatePool = false;
            if(!objectPool.Initiated){
                objectPool.Initiate();
                terminatePool = true;
            }
            PlacementLoop();
            if(terminatePool){
                objectPool.Terminate();
            }

            void PlacementLoop () {
                float t = 0f;
                int loopCounter = 0;
                
                var tempGO = new GameObject();
                tempGO.transform.localRotation = Quaternion.Euler(0, universalRotationOffset, 0);
                Vector3 sizeMeasureAxis = tempGO.transform.InverseTransformDirection(Vector3.forward);
                #if UNITY_EDITOR
                DestroyImmediate(tempGO);
                #else
                Destroy(tempGO);
                #endif

                while(t < 1f){
                    if(loopCounter > MAX_PLACE_LOOP_COUNT){
                        Debug.LogError("Reached loop limit, aborting!");
                        return;
                    }
                    loopCounter++;
                    // get new gameObject
                    var newSO = objectPool.Next(sizeMeasureAxis, poolRNG);
                    var newGO = newSO.SpawnedObject;
                    newGO.transform.SetParent(this.transform);
                    var newGOSize = newSO.LinearSize;
                    // advance half the object's length if needed
                    if((noOvershoot && t == 0f) || t != 0f){
                        if(!TryAdvanceT(newGOSize / 2f) || t > 1f){
                            #if UNITY_EDITOR
                            DestroyImmediate(newGO);
                            #else
                            Destroy(newGO);
                            #endif
                            return;
                        }
                    }
                    // do the actual placement and advance the other half
                    var rpo = Vector3.Scale(new Vector3(RandomDistribution(), RandomDistribution(), RandomDistribution()), placementRandomness);
                    var rot = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.ProjectOnPlane(spline.BezierDerivative(t), Vector3.up), Vector3.up));
                    var fpo = rot.MultiplyVector(universalPositionOffset);
                    var placeOffset = rpo + fpo;
                    var hPlaceOffset = new Vector3(placeOffset.x, 0f, placeOffset.z);
                    var vPlaceOffset = new Vector3(0f, placeOffset.y, 0f);
                    var bPoint = spline.BezierPoint(t) + hPlaceOffset;
                    var rotationOffset = universalRotationOffset + RandomDistribution() * rotationRandomness;
                    bool successfullyPlaced = TryPlace();
                    if(!successfullyPlaced || !TryAdvanceT(newGOSize / 2f) || (t > 1f && noOvershoot)){
                        if(!successfullyPlaced){
                            Debug.LogWarning("No place point found! Aborting...", this.gameObject);
                        }
                        #if UNITY_EDITOR
                        DestroyImmediate(newGO);
                        #else
                        Destroy(newGO);
                        #endif
                        return;
                    }
                    newGO.transform.position += vPlaceOffset;
                    // save the creation
                    BuildSafeUndo.RegisterCreatedObjectUndo(newGO, "Placed object from spline");
                    // finally do the last advance
                    TryAdvanceT(spaceBetweenObjects, allowBackwards: true);

                    float RandomDistribution () {
                        return 2f * (float)splineRNG.NextDouble() - 1f;
                    }

                    bool TryPlace () {
                        if(TryFindPlacePointAndNormal(out var tempPlacePoint, out var tempPlaceNormal)){
                            var tempSplineFwd = spline.BezierDerivative(t);
                            PlaceAtPointAndNormalWithRotationOffset(tempPlacePoint, tempPlaceNormal, tempSplineFwd, rotationOffset);
                            return true;
                        }
                        return false;
                    }

                    bool TryFindPlacePointAndNormal (out Vector3 outputPlacePoint, out Vector3 outputPlaceNormal) {
                        outputPlacePoint = default;
                        outputPlaceNormal = default;
                        if(groundMode == GroundMode.DISABLED){
                            outputPlacePoint = bPoint;
                            outputPlaceNormal = Vector3.up;
                            return true;
                        }else{
                            if(groundCollider == null){
                                Debug.LogWarning("No ground collider set even though snapping to ground is activated!", this.gameObject);
                            }
                            var gcb = groundCollider.bounds;
                            var ro = new Vector3(bPoint.x, gcb.center.y + gcb.extents.y + 1f, bPoint.z);
                            if(groundCollider.Raycast(new Ray(ro, Vector3.down), out RaycastHit hit, 2f * gcb.extents.y + 2f)){
                                Debug.DrawLine(ro, hit.point, Color.green, 0f, false);
                                Debug.DrawRay(hit.point, hit.normal, Color.blue, 0f, false);
                                outputPlacePoint = hit.point;
                                if(groundMode == GroundMode.SNAP_AND_ALIGN){
                                    outputPlaceNormal = hit.normal;
                                }else{
                                    outputPlaceNormal = Vector3.up;
                                }
                                return true;        
                            }else{
                                Debug.DrawRay(ro, Vector3.down * (gcb.extents.y * 2f + 2f), Color.red, 10f, false);
                                Debug.Log("no hit");
                                return false;
                            }
                        }
                    }

                    void PlaceAtPointAndNormalWithRotationOffset (Vector3 inputPlacePoint, Vector3 inputPlaceNormal, Vector3 inputSplineForward, float inputRotationOffset) {
                        newGO.transform.position = inputPlacePoint;
                        Vector3 lookFwd = Vector3.ProjectOnPlane(inputSplineForward, inputPlaceNormal);
                        newGO.transform.rotation = Quaternion.LookRotation(lookFwd, inputPlaceNormal);
                        newGO.transform.Rotate(Vector3.up, inputRotationOffset);
                    }

                    bool TryAdvanceT (float advanceDist, bool allowBackwards = false) {
                        float newT;
                        switch(distanceMode){
                            case DistanceMode.EUCLIDIAN:
                                newT = spline.NextTFromEuclidianDistance(t, advanceDist);
                                break;
                            case DistanceMode.BEZIER:
                                newT = spline.NextTFromBezierDistance(t, advanceDist);
                                break;
                            default:
                                Debug.LogError($"Unknown {nameof(DistanceMode)} \"{distanceMode}\"!");
                                return false;
                        }
                        if(newT <= t && !allowBackwards){
                            Debug.LogError("No forward advance! Aborting placement...", this.gameObject);
                            return false;
                        }
                        t = newT;
                        return true;
                    }
                }
            }
        }
    
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(BSplineObjectPlacer))]
    public class BSplineObjectPlacerEditor : Editor {

        BSplineObjectPlacer bsop;

        void OnEnable () {
            bsop = target as BSplineObjectPlacer;
        }

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            GUILayout.Space(10);
            if(GUILayout.Button("Delete placed objects")){
                bsop.DeletePlacedObjects();
            }
            if(GUILayout.Button("(Re)Place")){
                bsop.PlaceObjects();
            }
            if(GUILayout.Button("Randomize Seed")){
                bsop.RandomizeSeed();
            }
            GUILayout.Space(10);
            if(GUILayout.Button("Rotate placed objects 90°")){
                bsop.Rotate90Deg();
            }
            if(GUILayout.Button("Reverse direction")){
                bsop.ReverseDirection();
            }
        }

    }

    #endif

}