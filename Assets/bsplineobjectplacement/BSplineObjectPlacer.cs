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
    [SerializeField] float rotationOffset;

    [Header("Placement Settings")]
    [SerializeField] bool flipSides;    // TODO rather concave/convex?
    [SerializeField] DistanceMode distanceMode;
    [SerializeField] GroundMode groundMode;
    [SerializeField] Collider groundCollider;
    
    enum DistanceMode {
        EUCLIDIAN,
        BEZIER
    }

    enum GroundMode {
        DISABLED,
        SNAP,
        SNAP_AND_ALIGN
    }

    protected override void Reset () {
        base.Reset();
        randomSeed = 0;
    }

    public void UpdateSeed (int newSeed) {
        this.randomSeed = newSeed;
    }

    public void UpdatePool (BSplineObjectPool newPool) {
        this.objectPool = newPool;
    }

    public void DeletePlacedObjects () {
        for(int i=transform.childCount-1; i>=0; i--){
            DestroyImmediate(transform.GetChild(i).gameObject);
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
        while(t < 1f){
            var xRand = 2f * (float)rng.NextDouble() - 1f;
            var yRand = 2f * (float)rng.NextDouble() - 1f;
            var zRand = 2f * (float)rng.NextDouble() - 1f;
            var bPoint = BezierPoint(t) + Vector3.Scale(new Vector3(xRand, yRand, zRand), placementRandomness);

            bool canPlace = false;
            Vector3 placePoint = default;
            Vector3 placeNormal = default;
            if(groundMode == GroundMode.DISABLED){
                canPlace = true;
                placePoint = bPoint;
                placeNormal = Vector3.up;
            }else{
                if(groundCollider == null){
                    Debug.LogWarning("No ground collider set even though snapping to ground is activated!", this.gameObject);
                    return;
                }
                var gcb = groundCollider.bounds;
                var ro = new Vector3(bPoint.x, gcb.center.y + gcb.extents.y + 1f, bPoint.z);
                groundCollider.Raycast(new Ray(ro, Vector3.down), out RaycastHit hit, 2f * gcb.extents.y + 2f);
                if(hit.collider != null){
                    Debug.DrawLine(ro, hit.point, Color.green, 0f, false);
                    Debug.DrawRay(hit.point, hit.normal, Color.blue, 0f, false);
                    canPlace = true;
                    placePoint = hit.point;
                    if(groundMode == GroundMode.SNAP_AND_ALIGN){
                        placeNormal = hit.normal;
                    }else{
                        placeNormal = Vector3.up;
                    }                    
                }else{
                    Debug.DrawRay(ro, Vector3.down * (gcb.extents.y * 2f + 2f), Color.red, 0f, false);
                }
            }
            if(!canPlace){
                Debug.LogWarning("Placement error, aborting!", this.gameObject);
                return;
            }

            var newTemplate = objectPool.WeightedRandomObject(rng);
            if(newTemplate.Item1 == null){
                Debug.LogError($"There was a null object in {objectPool.name}! Aborting placement!", this.gameObject);
                return;
            }
            var newGO = Instantiate(newTemplate.Item1, this.transform);
            newGO.transform.position = placePoint;
            var splineFwd = BezierDerivative(t);
            Vector3 lookFwd = Vector3.ProjectOnPlane(splineFwd, placeNormal);
            newGO.transform.rotation = Quaternion.LookRotation(lookFwd, placeNormal);
            newGO.transform.Rotate(Vector3.up, rotationOffset + (rotationRandomness * (2f * (float)rng.NextDouble() - 1f)));

            if(newTemplate.Item2 != null){
                var newGOMR = newGO.GetComponent<MeshRenderer>();
                newGOMR.material = newTemplate.Item2;
            }
            var newGOMF = newGO.GetComponent<MeshFilter>();
            var newGOBounds = newGOMF.sharedMesh.bounds;
            float advanceDist = spaceBetweenObjects;
            // var boundsRayOrigin = newGOBounds.center + (splineFwd * newGOBounds.extents.magnitude * 2f);
            // var boundsRayDirection = -splineFwd;
            // if(newGOBounds.IntersectRay(new Ray(boundsRayOrigin, boundsRayDirection), out var hitDist)){
            //     var boundsRayHit = boundsRayOrigin + hitDist * boundsRayDirection.normalized;
            //     advanceDist += (boundsRayOrigin - boundsRayHit).magnitude;
            // }
            advanceDist += 2 * newGOBounds.extents.magnitude;

            float deltaT;
            switch(distanceMode){
                case DistanceMode.EUCLIDIAN:
                    deltaT = NextTFromEuclidianDistance(t, advanceDist);
                    break;
                case DistanceMode.BEZIER:
                    deltaT = NextTFromBezierDistance(t, advanceDist);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(DistanceMode)} \"{distanceMode}\"!");
                    return;
            }
            if(deltaT <= 0f){
                Debug.LogError("Delta T was negative! Aborting placement...", this.gameObject);
            }
            Debug.Log(advanceDist + ", " + deltaT);
            t = deltaT;
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
        if(GUILayout.Button("(Re)Place")){
            bsop.PlaceObjects();
        }
        if(GUILayout.Button("(Re)Place with new random seed")){
            int hash = Random.value.GetHashCode();
            int loopCount = Mathf.Abs(hash)%64;
            for(int i=0; i<loopCount; i++){
                hash += Random.value.GetHashCode();
            }
            bsop.UpdateSeed(hash);
            bsop.PlaceObjects();
        }
        if(GUILayout.Button("Delete placed objects")){
            bsop.DeletePlacedObjects();
        }
    }

}

#endif