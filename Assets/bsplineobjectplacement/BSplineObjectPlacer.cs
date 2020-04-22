using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BSplineObjectPlacer : QuadraticBezierSpline {

    [Header("Randomness")]
    [SerializeField] int randomSeed;
    [SerializeField] Vector3 placementRandomness;
    [SerializeField] float rotationRandomness;

    [Header("Object Settings")]
    [SerializeField] BSplineObjectPool objectPool;
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

    public void UpdateObjectSettings (BSplineObjectPool newPool, float newSpaceBetweenObjects) {
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

    public void Rotate90Deg () {
        universalRotationOffset = Mathf.Repeat(universalRotationOffset + 90f, 360f);
        ConditionalReplace();
    }

    public void DeletePlacedObjects () {
        for(int i=transform.childCount-1; i>=0; i--){
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    void ConditionalReplace () {
        if(transform.childCount > 0){
            PlaceObjects();
        }
    }

    public void PlaceObjects () {
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

        System.Random rng = new System.Random(randomSeed);
        float t = 0f;
        int loopCounter = 0;
        while(t < 1f){
            if(loopCounter > 1000){
                Debug.LogError("Reached loop limit, aborting!");
                return;
            }
            loopCounter++;
            // instantiate new model
            var newTemplate = objectPool.WeightedRandomObject(rng);
            if(newTemplate == null){
                Debug.LogError($"There was a null object in {objectPool.name}! Aborting placement!", this.gameObject);
                return;
            }
            var newGO = Instantiate(newTemplate.Prefab, this.transform);
            var newGOMR = newGO.GetComponent<MeshRenderer>();
            var newGOMF = newGO.GetComponent<MeshFilter>();

            Vector3 bPoint;
            float rotationOffset;
            // advance half the object's length
            if((noOvershoot && t == 0f) || t != 0f){
                bPoint = BezierPoint(t);
                rotationOffset = universalRotationOffset;
                bool placePointFound = TryPlaceAndAdvance();
                if(!placePointFound || t > 1f){
                    if(!placePointFound){
                        Debug.LogWarning("No place point found! Aborting...", this.gameObject);
                    }
                    DestroyImmediate(newGO);
                    return;
                }
            }
            // do the actual placement and advance the other half
            bPoint = BezierPoint(t) + Vector3.Scale(new Vector3(RandDist(), RandDist(), RandDist()), placementRandomness);
            rotationOffset = universalRotationOffset + RandDist() * rotationRandomness;
            bool successfullyPlaced = TryPlaceAndAdvance();
            if(!successfullyPlaced || (t > 1f && noOvershoot)){
                if(!successfullyPlaced){
                    Debug.LogWarning("No place point found! Aborting...", this.gameObject);
                }
                DestroyImmediate(newGO);
                return;
            }
            // assign the material if there is one
            var newGOMat = newTemplate.RandomMaterial(rng);
            if(newGOMat != null){
                newGOMR.sharedMaterial = newGOMat;
            }
            // finally do the last advance
            TryAdvanceT(spaceBetweenObjects, allowBackwards: true);

            float RandDist () {
                return 2f * (float)rng.NextDouble() - 1f;
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
                        Debug.DrawRay(ro, Vector3.down * (gcb.extents.y * 2f + 2f), Color.red, 0f, false);
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

            bool TryFindAdvanceDistance (Vector3 inputAdvanceDirection, out float outputAdvanceDist) {
                if(newTemplate.UseBoxColliderForSpacing){
                    var boxColliders = newGO.GetComponents<BoxCollider>();
                    if(boxColliders == null || boxColliders.Length <= 0){
                        Debug.LogError("No BoxColliders present but they are marked to be used!");
                        outputAdvanceDist = 0f;
                        return false;
                    }
                    if(boxColliders.Length > 1){
                        Debug.LogError("Multiple BoxColliders present!");
                        outputAdvanceDist = 0f;
                        return false;
                    }
                    var col = boxColliders[0];
                    var worldColCenter = newGO.transform.TransformPoint(col.center);
                    var worldColSize = newGO.transform.TransformVector(col.size);
                    var worldRD = -inputAdvanceDirection.normalized;
                    var worldRO = worldColCenter - worldRD * worldColSize.magnitude * 2f;
                    if(col.Raycast(new Ray(worldRO, worldRD), out var colRayHit, 2f * worldColSize.magnitude)){
                        outputAdvanceDist = (worldColCenter - colRayHit.point).magnitude;
                        return true;
                    }else{
                        Debug.LogError("No Hit!");
                        outputAdvanceDist = 0f;
                        return false;
                    }
                }else{
                    var bounds = newGOMF.sharedMesh.bounds;
                    var localDir = newGO.transform.InverseTransformDirection(inputAdvanceDirection).normalized;
                    var boundsRO = bounds.center + (localDir * bounds.extents.magnitude * 2f);
                    if(bounds.IntersectRay(new Ray(boundsRO, -localDir), out float boundsHitDist)){
                        var boundsHit = boundsRO - localDir * boundsHitDist;
                        var worldBoundsCenter = newGO.transform.TransformPoint(bounds.center);
                        var worldBoundsHit = newGO.transform.TransformPoint(boundsHit);
                        outputAdvanceDist = (worldBoundsCenter - worldBoundsHit).magnitude;
                        return true;
                    }else{
                        Debug.LogError("No Hit!");
                        outputAdvanceDist = 0f;
                        return false;
                    }
                }
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

            bool TryPlaceAndAdvance () {
                if(TryFindPlacePointAndNormal(out var tempPlacePoint, out var tempPlaceNormal)){
                    var tempSplineFwd = BezierDerivative(t);
                    PlaceAtPointAndNormalWithRotationOffset(tempPlacePoint, tempPlaceNormal, tempSplineFwd, rotationOffset);
                    if(TryFindAdvanceDistance(Vector3.ProjectOnPlane(tempSplineFwd, tempPlaceNormal), out var tempAdvanceDist)){
                        if(TryAdvanceT(tempAdvanceDist)){
                            return true;
                        }
                    }
                }
                return false;
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
        if(GUILayout.Button("Delete placed objects")){
            bsop.DeletePlacedObjects();
        }
        if(GUILayout.Button("(Re)Place")){
            bsop.PlaceObjects();
        }
        if(GUILayout.Button("Randomize Seed")){
            bsop.RandomizeSeed();
        }
        if(GUILayout.Button("Rotate placed objects 90°")){
            bsop.Rotate90Deg();
        }
    }

}

#endif