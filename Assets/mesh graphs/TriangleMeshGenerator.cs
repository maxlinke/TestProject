using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TriangleMeshGenerator : MonoBehaviour {

    [SerializeField] GameObject selectObject;
    [SerializeField] float selectRadius;
    [SerializeField] float lineLength;
    [SerializeField] float debugGizmoSize;

    Vector3 aAxis => new Vector3(1f, 0f, 0f);
    Vector3 bAxis => new Vector3(-Mathf.Sin(Mathf.Deg2Rad * 30), 0f, -Mathf.Cos(Mathf.Deg2Rad * 30));

    float bigGizmoSize => debugGizmoSize;
    float mediumGizmosSize => 0.5f * debugGizmoSize;
    float smallGizmoSize => 0.25f * debugGizmoSize;
    float miniGizmoSize => 0.05f * debugGizmoSize;

    void OnDrawGizmos () {
        // DrawDebugGridAndAxes();
        // if(selectObject != null){
        //     DrawSelectDebugInfo(selectObject.transform.position);
        // }
        if(selectObject != null){
            var points = GetPointsInsideRadius(selectObject.transform.position, selectRadius);
            Gizmos.color = Color.white;
            foreach(var point in points){
                Gizmos.DrawCube(ABCtoWorldXYZ(point), smallGizmoSize * Vector3.one);
            }
        }
    }

    void DrawDebugGridAndAxes (int referenceGridRes = 10) {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, mediumGizmosSize);
        var referencePoints = new List<TriMeshPoint>();
        for(int i=-referenceGridRes; i<=referenceGridRes; i++){
            for(int j=-referenceGridRes; j<=referenceGridRes; j++){
                referencePoints.Add(new TriMeshPoint(i, j));
            }
        }
        Gizmos.color = Color.white;
        Handles.color = Color.white;
        foreach(var point in referencePoints){
            Vector3 worldPoint = transform.TransformPoint(ABCtoLocalXYZ(point));
            Gizmos.DrawSphere(worldPoint, smallGizmoSize);
            Handles.Label(worldPoint, $"(A: {point.a}, B: {point.b}, C: {point.c})");
        }
        Gizmos.color = Color.yellow;
        Handles.color = Gizmos.color;
        var aRay = transform.TransformDirection(aAxis * lineLength);
        Gizmos.DrawRay(transform.position, aRay);
        Handles.Label(transform.position + aRay, "a");
        Gizmos.color = Color.magenta;
        Handles.color = Gizmos.color;
        var bRay = transform.TransformDirection(bAxis * lineLength);
        Gizmos.DrawRay(transform.position, bRay);
        Handles.Label(transform.position + bRay, "b");
    }

    void DrawSelectDebugInfo (Vector3 worldMidPoint, int gridRes = 32, float gridSize = 4) {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldMidPoint, smallGizmoSize);
        var localMidPoint = transform.InverseTransformPoint(worldMidPoint);
        var localTriGridPoint = ABCtoLocalXYZ(LocalXYZtoNearestABC(localMidPoint));
        var worldTriGridPoint = transform.TransformPoint(localTriGridPoint);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(worldTriGridPoint, mediumGizmosSize);
        for(int i=-gridRes; i<=gridRes; i++){
            for(int j=-gridRes; j<=gridRes; j++){
                Vector3 debugPoint = worldMidPoint + (gridSize * new Vector3(i, 0, j) / gridRes);
                var approx = LocalXYZtoNearestABC(transform.InverseTransformPoint(debugPoint));
                byte r = (byte)((71 * approx.a) % 255);
                byte g = (byte)((137 * approx.b) % 255);
                byte b = (byte)((231 * approx.a + 197 * approx.b) % 255);
                Gizmos.color = new Color32(r, g, b, (byte)(255));
                Gizmos.DrawCube(debugPoint, miniGizmoSize * Vector3.one);
            }
        }
    }

    TriMeshPoint WorldXYZtoNearestABC (Vector3 worldPoint) {
        return LocalXYZtoNearestABC(transform.InverseTransformPoint(worldPoint));
    }

    TriMeshPoint LocalXYZtoNearestABC (Vector3 localPoint) {
        var normedLocalPoint =  localPoint /  lineLength;
        int a = Mathf.FloorToInt(Vector3.Dot(normedLocalPoint, aAxis) + 0.5f);
        int b = Mathf.FloorToInt(Vector3.Dot(normedLocalPoint, bAxis) + 0.5f);
        TriMeshPoint closest = default;
        float closestSqrDist = Mathf.Infinity;
        for(int i=-1; i<=1; i++){
            for(int j=-1; j<=1; j++){
                var temp = new TriMeshPoint(a + i, b + j);
                float tempSqrDist = (localPoint - ABCtoLocalXYZ(temp)).sqrMagnitude;
                if(tempSqrDist < closestSqrDist){
                    closest = temp;
                    closestSqrDist = tempSqrDist;
                }
            }
        }
        return closest;
    }

    Vector3 ABCtoWorldXYZ (TriMeshPoint triPoint) {
        return transform.TransformPoint(ABCtoLocalXYZ(triPoint));
    }

    Vector3 ABCtoLocalXYZ (TriMeshPoint triPoint) {
        float z = ((triPoint.b * aAxis.x) - (bAxis.x * triPoint.a)) / ((bAxis.z * aAxis.x) - (aAxis.z * bAxis.x));
        float x = (triPoint.a - (z * aAxis.z)) / aAxis.x;
        return new Vector3(x, 0, z) * lineLength;
    }

    List<TriMeshPoint> GetPointsInsideRadius (Vector3 worldCenterPoint, float worldRadius) {
        var output = new List<TriMeshPoint>();
        float sqrWorldRadius = worldRadius * worldRadius;
        var triCenter = WorldXYZtoNearestABC(worldCenterPoint);
        if(TriPointIsInRadius(triCenter) && lineLength > 0f){
            output.Add(triCenter);
            int localA = 0;
            int localB = 0;
            int localC = 0;
            int ringIndex = 0;
            bool atLeastOnePointInRadius = true;

            while(atLeastOnePointInRadius){
                atLeastOnePointInRadius = false;
                localA++;
                localB--;
                ringIndex++;
                int pointsOnRing = ringIndex * 6;
                TraceRing();

                void TraceRing () {
                    TraceSegment(() => {localB--; localC++;}, () => (localC >= 0)); //will be skipped on the first ring because c is already 0
                    TraceSegment(() => {localA--; localC++;}, () => (localA <= 0));
                    TraceSegment(() => {localA--; localB++;}, () => (localB >= 0));
                    TraceSegment(() => {localC--; localB++;}, () => (localC <= 0));
                    TraceSegment(() => {localC--; localA++;}, () => (localA >= 0));
                    TraceSegment(() => {localB--; localA++;}, () => (localB <= 0));
                    CreateCheckAndPossiblyInsertPointAndCurrentLocalPos();

                    void TraceSegment (System.Action step, System.Func<bool> stopCheck) {
                        int segmentLoopCounter = 0;
                        while(segmentLoopCounter < 100){
                            if(stopCheck()){
                                return;
                            }
                            CreateCheckAndPossiblyInsertPointAndCurrentLocalPos();
                            step();
                            segmentLoopCounter++;
                        }
                        Debug.LogError("breaking loop because loop counter reached limit. if this is intentional, adjust the hardcoded limit...");
                    }

                    void CreateCheckAndPossiblyInsertPointAndCurrentLocalPos () {
                        var newPoint = new TriMeshPoint(localA + triCenter.a, localB + triCenter.b, localC + triCenter.c);
                        if(TriPointIsInRadius(newPoint)){
                            output.Add(newPoint);
                            atLeastOnePointInRadius = true;
                        }
                    }
                }

                
            }
        }
        return output;

        bool TriPointIsInRadius (TriMeshPoint triPoint) {
            return ((ABCtoWorldXYZ(triPoint) - worldCenterPoint).sqrMagnitude <= sqrWorldRadius);
        }
    }

    struct TriMeshPoint {

        public readonly int a;
        public readonly int b;
        public int c => -(a+b);

        public TriMeshPoint (int a, int b) {
            this.a = a;
            this.b = b;
        }

        public TriMeshPoint (int a, int b, int c) {
            this.a = a;
            this.b = b;
            int sum = a + b + c;
            if(sum != 0){
                throw new System.Exception($"Error, {nameof(TriMeshPoint)} indices must have a sum of 0. Indices: {a}, {b}, {c}, Sum: {sum}.");
            }
        }

    }

}