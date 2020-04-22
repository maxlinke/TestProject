using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuadraticBezierSpline : MonoBehaviour {

    public const float MIN_GIZMO_SIZE = 0.05f;
    public const float MAX_GIZMO_SIZE = 5f;

    [SerializeField] public bool drawGizmos;
    [SerializeField] public float gizmoSize;

    [Header("Handles")]
    [SerializeField] public Vector3 handle1;
    [SerializeField] public Vector3 handle2;
    [SerializeField] public Vector3 controlHandle;

    public Vector3 p1 => transform.TransformPoint(handle1);
    public Vector3 p2 => transform.TransformPoint(handle2);
    public Vector3 pC => transform.TransformPoint(controlHandle);

    protected virtual void Reset () {
        drawGizmos = true;
        gizmoSize = 0.5f;
        handle1 = new Vector3(-5f, 0f, -3f);
        handle2 = new Vector3(5f, 0f, -3f);
        controlHandle = new Vector3(0f, 0f, 6f);
    }

    protected Color GetGizmoColor () {
        int hash = 0;
        foreach(var ch in gameObject.name){
            hash += 17 * ch;
        }
        hash = Mathf.Abs(hash);
        float hue = (float)(hash % 100) / 100;
        float saturation = (float)(hash % 90) / 90;
        saturation = 0.25f + 0.5f * saturation;
        float value = (float)(hash % 70) / 70;
        value = 0.667f + 0.333f * value;
        return Color.HSVToRGB(hue, saturation, value);
    }

    protected virtual void GizmoLine (float stepSize, System.Action<Vector3> onStep) {
        float l = BezierLengthEstimate();
        if(l > 0){
            float t = 0f;
            onStep(BezierPoint(t));
            while(t < 1f){
                t = Mathf.Clamp01(NextTFromEuclidianDistance(t, stepSize, 10, true, l));
                onStep(BezierPoint(t));
            }
        }
    }

    protected virtual void OnDrawGizmos () {
        gizmoSize = Mathf.Clamp(gizmoSize, MIN_GIZMO_SIZE, MAX_GIZMO_SIZE);
        if(!drawGizmos){
            return;
        }
        var colorChache = Gizmos.color;
        Gizmos.color = GetGizmoColor();
        Gizmos.DrawSphere(p1, gizmoSize);
        Gizmos.DrawSphere(p2, gizmoSize);
        Gizmos.DrawSphere(pC, gizmoSize);
        Vector3 lastPoint = p1;
        GizmoLine(2f * gizmoSize, (newPoint) => {
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        });
        Gizmos.color = colorChache;
    }

    void OnDrawGizmosSelected () {
        if(!drawGizmos || Selection.activeGameObject != this.gameObject){
            return;
        }
        var colorChache = Gizmos.color;
        Gizmos.color = GetGizmoColor();
        float stepSize = 3f * gizmoSize;
        float objectSize = 0.33f * gizmoSize;
        GizmoLine(stepSize, (newPoint) => {
            Gizmos.DrawSphere(newPoint, objectSize);
        });
        Gizmos.color = colorChache;
    }

    public Vector3 BezierPoint (float t) {
        float iT = 1f - t;
        return pC + (iT * iT * (p1 - pC)) + (t * t * (p2 - pC));
    }

    public Vector3 BezierDerivative (float t) {
        float iT = 1f - t;
        return (2f * iT * (pC - p1)) + (2f * t * (p2 - pC));
    }

    public float BezierDistanceEstimate (float startT, float endT, int steps = 100) {
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

    public float BezierLengthEstimate (int steps = 100) {
        return BezierDistanceEstimate(0f, 1f, steps);
    }

    public float NextTFromEuclidianDistance (float startT, float desiredDistance, int iterations = 16, bool dontCalculateLength = false, float inputLength = 0f) {
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

    public float NextTFromBezierDistance (float startT, float desiredDistance, int precision = 16, int iterations = 16, bool dontCalculateLength = false, float inputLength = 0f) {
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

    protected float NextTFromDistance (System.Func<float, float> getDistDelta, float startT, float desiredDistance, int iterations, bool dontCalculateLength, float inputLength) {
        if(desiredDistance == 0 || float.IsNaN(desiredDistance) || float.IsInfinity(desiredDistance)){
            return startT;
        }
        float length = (dontCalculateLength ? inputLength : BezierLengthEstimate());
        if(length == 0 || float.IsInfinity(length) || float.IsNaN(length)){
            return startT;
        }
        float lastDistDelta = getDistDelta(startT);
        float closestT = startT;
        float closestTAbsDistDelta = Mathf.Abs(lastDistDelta);
        float delta = desiredDistance / length;     // just a solid(ish) guess. this is where the direction comes from as desiredDistance can be positive or negative...
        float deltaMultiplier = 2f;
        
        for(int i=0; i<iterations; i++){            // binary search
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

    #if UNITY_EDITOR

    public virtual void EditorHandles () {
        EditorGUI.BeginChangeCheck();
        Vector3 newH1 = Handles.PositionHandle(p1, Quaternion.identity);
        Vector3 newH2 = Handles.PositionHandle(p2, Quaternion.identity);
        Vector3 newCH = Handles.PositionHandle(pC, Quaternion.identity);
        if(EditorGUI.EndChangeCheck()){
            Undo.RecordObject(this, "Change Handle Position");
            handle1 = transform.InverseTransformPoint(newH1);
            handle2 = transform.InverseTransformPoint(newH2);
            controlHandle = transform.InverseTransformPoint(newCH);
        }
    }

    #endif

}

#if UNITY_EDITOR

[CustomEditor(typeof(QuadraticBezierSpline))]
public class QuadraticBezierSplineEditor : Editor {

    QuadraticBezierSpline qbs;

    protected virtual void OnEnable () {
        qbs = target as QuadraticBezierSpline;
    }

    protected virtual void OnSceneGUI () {
        qbs.EditorHandles();
    }

}

#endif