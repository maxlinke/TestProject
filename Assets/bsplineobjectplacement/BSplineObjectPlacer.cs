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

    [Header("Debug")]
    [SerializeField] float testStartT;
    [SerializeField] float testDistance;
    [SerializeField] float testDistanceOutput;
    [SerializeField, Range(0, 20)] int testIterations;

    public bool setUp => (handle1 != null && handle2 != null && controlHandle != null);

    enum GroundMode {
        DISABLED,
        SNAP,
        SNAP_AND_ALIGN
    }

    void Start () {
        
    }

    void Update () {
        
    }

    void OnDrawGizmos () {
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
        float nT = NextTFromDistance(testStartT, testDistance, testIterations);
        Vector3 t = BezierPoint(nT);
        Gizmos.DrawSphere(s, gizmoSize);
        Gizmos.DrawSphere(t, gizmoSize);
        testDistanceOutput = (s - t).magnitude;

        // float l = BezierLengthEstimate();
        // float t = 0f;
        // // while(t<1f){
        // //     Gizmos.DrawCube(BezierPoint(t), gizmoSize * Vector3.one);
        // //     float nextT = t;
        // //     float lastDelta = 0;
        // //     for(int i=0; i<4; i++){

        // //     }
        // // }
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

    float BezierLengthEstimate (int steps = 100) {
        float total = 0f;
        Vector3 last = BezierPoint(0f);
        for(int i=0; i<steps; i++){
            float t = (float)(i+1) / steps;
            Vector3 current = BezierPoint(t);
            total += (current - last).magnitude;
            last = current;
        }
        return total;
    }

    float NextTFromDistance (float t, float desiredDistance, int iterations = 16, bool dontCalculateLength = false, float inputLength = 0f) {
        if(desiredDistance == 0 || float.IsNaN(desiredDistance) || float.IsInfinity(desiredDistance)){
            return t;
        }
        float length = (dontCalculateLength ? inputLength : BezierLengthEstimate());
        if(length == 0 || float.IsInfinity(length) || float.IsNaN(length)){
            return t;
        }
        float desiredSqrDistance = desiredDistance * desiredDistance;
        Vector3 startPoint = BezierPoint(t);
        float lastSqrDistanceDelta = SqrDistanceDelta(t);
        float closestT = t;
        float closestTAbsSqrDistanceDelta = Mathf.Abs(lastSqrDistanceDelta);
        float delta = desiredDistance / length;     // just a solid(ish) guess. this is where the direction comes from as desiredDistance can be positive or negative...
        float deltaMultiplier = 2f;

        for(int i=0; i<iterations; i++){
            t += delta;
            float sqrDistanceDelta = SqrDistanceDelta(t);
            if(sqrDistanceDelta == 0){
                closestT = t;
                break;
            }
            if(Mathf.Sign(sqrDistanceDelta) != Mathf.Sign(lastSqrDistanceDelta)){   // over-/undershoot
                delta *= -1f;               // flip direction
                deltaMultiplier = 0.5f;     // from now on we're inching closer every iteration
            }
            float absSqrDistanceDelta = Mathf.Abs(sqrDistanceDelta);
            if(absSqrDistanceDelta < closestTAbsSqrDistanceDelta){
                closestT = t;
                closestTAbsSqrDistanceDelta = absSqrDistanceDelta;
            }
            delta *= deltaMultiplier;
            lastSqrDistanceDelta = sqrDistanceDelta;
        }

        return closestT;

        // positive: not far enough, negative: too far (kinda)
        float SqrDistanceDelta (float testT) {
            return desiredSqrDistance - (BezierPoint(testT) - startPoint).sqrMagnitude;
        }
    }

}
