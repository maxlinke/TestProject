using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSplineObjectPlacer : MonoBehaviour {

    [SerializeField] bool drawGizmos;
    [SerializeField] float gizmoSize;

    [Header("Handles")]
    [SerializeField] Transform handle1;
    [SerializeField] Transform handle2;
    [SerializeField] Transform controlHandle;

    [Header("Settings")]
    [SerializeField] GroundMode groundMode;
    [SerializeField] Collider groundCollider;
    [SerializeField] PlacementMode placementMode;
    [SerializeField] bool flipSides;
    [SerializeField] Vector2 placementRandomness;

    [Header("Debug")]
    [SerializeField] float testStartT;
    [SerializeField] float testDistance;
    [SerializeField] bool testEuclidian;
    [SerializeField] float testDistanceOutput;
    [SerializeField] float testDistanceOuptutDeviation;
    [SerializeField, Range(0, 20)] int testIterations;
    [SerializeField, Range(0, 20)] int testPrecision;
    [SerializeField] bool ASDF;

    public bool setUp => (handle1 != null && handle2 != null && controlHandle != null);

    enum GroundMode {
        DISABLED,
        SNAP,
        SNAP_AND_ALIGN
    }

    enum PlacementMode {
        CENTER,
        SIDE,
        CORNER
    }

    void OnDrawGizmos () {
        if(ASDF){
            Debug.Log(BezierDistanceEstimate(0.5642f, 0.3123f));
            ASDF = false;
        }
        if(!setUp || !drawGizmos){
            return;
        }
        var colorChache = Gizmos.color;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(handle1.transform.position, 2f * gizmoSize);
        Gizmos.DrawSphere(handle2.transform.position, 2f * gizmoSize);
        Gizmos.DrawSphere(controlHandle.transform.position, 2f * gizmoSize);
        Gizmos.color = Color.magenta;
        for(int i=0; i<100; i++){
            Gizmos.DrawCube(BezierPoint((float)i / 99), gizmoSize * 0.2f * Vector3.one);
        }
        Gizmos.color = Color.white;
        Vector3 s = BezierPoint(testStartT);
        Vector3 t;
        if(testEuclidian){
            float nT = NextTFromEuclidianDistance(testStartT, testDistance, testIterations);
            t = BezierPoint(nT);
            testDistanceOutput = (s - t).magnitude;
        }else{
            float nT = NextTFromBezierDistance(testStartT, testDistance, testPrecision, testIterations);
            t = BezierPoint(nT);
            testDistanceOutput = BezierDistanceEstimate(testStartT, nT);
        }
        testDistanceOuptutDeviation = testDistance - testDistanceOutput;
        Gizmos.DrawSphere(s, gizmoSize);
        Gizmos.DrawSphere(t, gizmoSize);

        // float l = BezierLengthEstimate();

        Gizmos.color = colorChache;
    }

    Vector3 BezierPoint (float t) {
        Vector3 p1 = handle1.transform.position;
        Vector3 p2 = handle2.transform.position;
        Vector3 pC = controlHandle.transform.position;
        float iT = 1f - t;
        return pC + (iT * iT * (p1 - pC)) + (t * t * (p2 - pC));
    }

    Vector3 BezierDerivative (float t) {
        Vector3 p1 = handle1.transform.position;
        Vector3 p2 = handle2.transform.position;
        Vector3 pC = controlHandle.transform.position;
        float iT = 1f - t;
        return (2f * iT * (pC - p1)) + (2f * t * (p2 - pC));
    }

    float BezierDistanceEstimate (float startT, float endT, int steps = 100) {
        float total = 0f;
        Vector3 last = BezierPoint(startT);
        for(int i=0; i<steps; i++){
            float t = Mathf.Lerp(startT, endT, (float)(i+1) / steps);
            Vector3 current = BezierPoint(t);
            total += (current - last).magnitude;
            last = current;
        }
        return total;
    }

    float NextTFromEuclidianDistance (float startT, float desiredDistance, int iterations = 16, bool dontCalculateLength = false, float inputLength = 0f) {
        float desiredSqrDistance = desiredDistance * desiredDistance;
        Vector3 startPoint = BezierPoint(startT);
        return NextTFromDistance(
            getDistDelta: (testT) => (desiredSqrDistance - (BezierPoint(testT) - startPoint).sqrMagnitude), 
            startT: startT, 
            desiredDistance: desiredDistance, 
            iterations: iterations, 
            dontCalculateLength: dontCalculateLength, 
            inputLength: inputLength
        );
    }

    float NextTFromBezierDistance (float startT, float desiredDistance, int precision, int iterations = 16, bool dontCalculateLength = false, float inputLength = 0f) {
        float absDist = Mathf.Abs(desiredDistance);
        Vector3 startPoint = BezierPoint(startT);
        return NextTFromDistance(
            getDistDelta: (testT) => (absDist - BezierDistanceEstimate(startT, testT, precision)),
            startT: startT, 
            desiredDistance: desiredDistance, 
            iterations: iterations, 
            dontCalculateLength: dontCalculateLength, 
            inputLength: inputLength
        );
    }

    float NextTFromDistance (System.Func<float, float> getDistDelta, float startT, float desiredDistance, int iterations, bool dontCalculateLength, float inputLength) {
        if(desiredDistance == 0 || float.IsNaN(desiredDistance) || float.IsInfinity(desiredDistance)){
            return startT;
        }
        float length = (dontCalculateLength ? inputLength : BezierDistanceEstimate(0f, 1f));
        if(length == 0 || float.IsInfinity(length) || float.IsNaN(length)){
            return startT;
        }
        float lastDistDelta = getDistDelta(startT);
        float closestT = startT;
        float closestTAbsDistDelta = Mathf.Abs(lastDistDelta);
        float delta = desiredDistance / length;     // just a solid(ish) guess. this is where the direction comes from as desiredDistance can be positive or negative...
        float deltaMultiplier = 2f;

        for(int i=0; i<iterations; i++){
            startT += delta;
            float currentDistDelta = getDistDelta(startT);
            if(currentDistDelta == 0){
                closestT = startT;
                break;
            }
            if(Mathf.Sign(currentDistDelta) != Mathf.Sign(lastDistDelta)){   // over-/undershoot
                delta *= -1f;               // flip direction
                deltaMultiplier = 0.5f;     // from now on we're inching closer every iteration
            }
            float absDistDelta = Mathf.Abs(currentDistDelta);
            if(absDistDelta < closestTAbsDistDelta){
                closestT = startT;
                closestTAbsDistDelta = absDistDelta;
            }
            delta *= deltaMultiplier;
            lastDistDelta = currentDistDelta;
        }

        return closestT;

        
    }

}
