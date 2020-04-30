using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class BSplineObjectPlacer : QuadraticBezierSpline {

        public const string DUPLICATE_TOOLTIP = "This will mess with probabilities and might result in failure to finish!";
        private const int MAX_PLACE_LOOP_COUNT = 1000;

        [Header("Randomness")]
        [SerializeField] int randomSeed;
        [SerializeField] Vector3 placementRandomness;
        [SerializeField] float rotationRandomness;

        [Header("Object Settings")]
        [SerializeField] ObjectPool objectPool;
        [SerializeField] float spaceBetweenObjects;
        [SerializeField] float universalRotationOffset;

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

        protected override void Reset () {
            base.Reset();
            randomSeed = 0;
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
            this.randomSeed = newSeed;
            ConditionalReplace();
        }

        public void UpdateRandomizationSettings (Vector3 newPlacementRandomness,  float newRotationRandomness) {
            this.placementRandomness = newPlacementRandomness;
            this.rotationRandomness = newRotationRandomness;
            ConditionalReplace();
        }

        public void UpdateObjectSettings (ObjectPool newPool, float newSpaceBetweenObjects) {
            this.objectPool = newPool;
            this.spaceBetweenObjects = newSpaceBetweenObjects;
            ConditionalReplace();
        }

        public void UpdatePlacementSettings (DistanceMode newDistanceMode, GroundMode newGroundMode, Collider newGroundCollider, bool newOvershootMode) {
            this.distanceMode = newDistanceMode;
            this.groundMode = newGroundMode;
            this.groundCollider = newGroundCollider;
            this.noOvershoot = newOvershootMode;
            ConditionalReplace();
        }

        public void ReverseDirection () {
            var temp = handle2;
            handle2 = handle1;
            handle1 = temp;
            universalRotationOffset = Mathf.Repeat(universalRotationOffset + 180f, 360f);
            ConditionalReplace();
        }

        public void Rotate90Deg () {
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
            // TODO if localsize != 1
            // apply world transform to handles, set localsize 1
            // don't forget to record own scale change in undo
            // and do a debug warning reminding people not to scale the splines
            // maybe make the scale applying its own method with a button...
            DeletePlacedObjects();
            if(BezierLengthEstimate() == 0f){
                Debug.LogWarning("Curve Length is zero!", this.gameObject);
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

            // this is one big mess. i know. i don't care.
            void PlacementLoop () {
                float t = 0f;
                int loopCounter = 0;
                Quaternion initialLocalRotation = Quaternion.Euler(0, universalRotationOffset, 0);
                while(t < 1f){
                    if(loopCounter > MAX_PLACE_LOOP_COUNT){
                        Debug.LogError("Reached loop limit, aborting!");
                        return;
                    }
                    loopCounter++;
                    // get new gameObject
                    var newSO = objectPool.Next(initialLocalRotation, poolRNG);
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
                    var placeOffset = Vector3.Scale(new Vector3(RandomDistribution(), RandomDistribution(), RandomDistribution()), placementRandomness);
                    var hPlaceOffset = new Vector3(placeOffset.x, 0f, placeOffset.z);
                    var vPlaceOffset = new Vector3(0f, placeOffset.y, 0f);
                    var bPoint = BezierPoint(t) + hPlaceOffset;
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
                    Undo.RegisterCreatedObjectUndo(newGO, "Placed object from spline");
                    // finally do the last advance
                    TryAdvanceT(spaceBetweenObjects, allowBackwards: true);

                    float RandomDistribution () {
                        return 2f * (float)splineRNG.NextDouble() - 1f;
                    }

                    bool TryPlace () {
                        if(TryFindPlacePointAndNormal(out var tempPlacePoint, out var tempPlaceNormal)){
                            var tempSplineFwd = BezierDerivative(t);
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
                            groundCollider.Raycast(new Ray(ro, Vector3.down), out RaycastHit hit, 2f * gcb.extents.y + 2f);

                            // var ASDF = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            // ASDF.transform.localScale = new Vector3(0.1f, 2f * gcb.extents.y + 2f, 0.1f);
                            // ASDF.transform.position = ro - 0.5f * ASDF.transform.localScale;
                            // ASDF.transform.SetParent(this.transform, true);

                            if(hit.collider != null){
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
                                newT = NextTFromEuclidianDistance(t, advanceDist);
                                break;
                            case DistanceMode.BEZIER:
                                newT = NextTFromBezierDistance(t, advanceDist);
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

        protected virtual void OnSceneGUI () {
            bsop.EditorHandles();
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