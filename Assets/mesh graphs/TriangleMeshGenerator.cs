using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TriangleMeshGenerator : MonoBehaviour {

    [SerializeField] SphereCollider refCollider;
    [SerializeField] float lineLength;
    [SerializeField] int debugResolution;
    [SerializeField] float debugSize;

    Vector3 selectionCenter;
    List<TriMeshPoint> selectionPoints; //local space
    List<TriMeshPoint> referencePoints; //also local space

    Vector3 aAxis;
    Vector3 bAxis;

    void Update () {
        EnsureAxesAndMatrixExist();
        if(referencePoints == null){
            referencePoints = new List<TriMeshPoint>();
            for(int i=-10; i<11; i++){
                for(int j=-10; j<11; j++){
                    referencePoints.Add(new TriMeshPoint(i, j));
                }
            }
        }
        if((refCollider != null) && (Mathf.Abs(lineLength) > 0)){
            selectionCenter = refCollider.transform.transform.position + (refCollider.transform.rotation * refCollider.center);
            Vector3 localSelectionCenter = transform.InverseTransformPoint(selectionCenter);
            if(selectionPoints == null){
                selectionPoints = new List<TriMeshPoint>();
            }
            selectionPoints.Clear();
            selectionPoints.Add(LocalXYZtoNearestABC(localSelectionCenter));
        }
    }

    void EnsureAxesAndMatrixExist () {
        aAxis = new Vector3(1f, 0f, 0f);
        bAxis = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * 30), 0f, -Mathf.Cos(Mathf.Deg2Rad * 30));
    }

    void OnDrawGizmos () {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(selectionCenter, 1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1f);
        if(selectionPoints != null){
            Gizmos.color = Color.black;
            foreach(var point in selectionPoints){
                Gizmos.DrawSphere(transform.TransformPoint(ABCtoLocalXYZ(point)), 0.5f);
            }
        }
        if(referencePoints != null){
            Gizmos.color = Color.white;
            Handles.color = Color.white;
            foreach(var point in referencePoints){
                Vector3 worldPoint = transform.TransformPoint(ABCtoLocalXYZ(point));
                Gizmos.DrawSphere(worldPoint, 0.25f);
                Handles.Label(worldPoint, $"(A: {point.a}, B: {point.b})");
            }
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(aAxis * lineLength));
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(bAxis * lineLength));
        for(int i=-debugResolution; i<=debugResolution; i++){
            for(int j=-debugResolution; j<=debugResolution; j++){
                Vector3 debugPoint = selectionCenter + (debugSize * new Vector3(i, 0, j) / debugResolution);
                var approx = LocalXYZtoNearestABC(transform.InverseTransformPoint(debugPoint));
                byte r = (byte)((71 * approx.a) % 255);
                byte g = (byte)((137 * approx.b) % 255);
                byte b = (byte)((231 * approx.a + 197 * approx.b) % 255);
                Gizmos.color = new Color32(r, g, b, (byte)(255));
                // Gizmos.DrawSphere(debugPoint, 0.05f);
                Gizmos.DrawCube(debugPoint, 0.05f * Vector3.one);
            }
        }
    }

    TriMeshPoint LocalXYZtoNearestABC (Vector3 localPoint) {
        var normedLocalPoint =  localPoint /  lineLength;
        float fA = Vector3.Dot(normedLocalPoint, aAxis);
        float fB = Vector3.Dot(normedLocalPoint, bAxis);
        // bool aSafe = IsInSafeZone(fA, out float aSafe01, out float aNonSafe01);
        // bool bSafe = IsInSafeZone(fB, out float bSafe01, out float bNonSafe01);
        // if(aSafe && bSafe){
        //     //nothing, a and b are fine
        // }else{
        //     if(aSafe){
                
        //     }else if(bSafe){

        //     }else{

        //     }
        //     fA = 0;
        //     fB = 0;
        // }
        int a = Mathf.FloorToInt(fA + 0.5f);
        int b = Mathf.FloorToInt(fB + 0.5f);
        // return new TriMeshPoint(a, b);
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

        bool IsInSafeZone (float floatVal, out float safeValue01, out float nonSafeValue01) {
            // float tan30 = Mathf.Tan(Mathf.Deg2Rad * 30);
            // float rMin = tan30 * lineLength;
            // float halfSafe = tan30 * rMin;
            float halfSafe = lineLength / 3f;   //tan(30) squared = 1/3
            floatVal = Mathf.Repeat(floatVal, 1f);
            safeValue01 = Mathf.Clamp01((floatVal + halfSafe) / (2f * halfSafe));
            nonSafeValue01 = Mathf.Clamp01((floatVal - halfSafe) / (1f - (2f * halfSafe)));
            return (nonSafeValue01 <= 0 || nonSafeValue01 >= 1);
        }
    }

    Vector3 ABCtoLocalXYZ (TriMeshPoint triPoint) {
        // float x = triPoint.a;
        // float z = (triPoint.b - (triPoint.a * bAxis.x)) / bAxis.z;
        // return new Vector3(x, 0, z) * lineLength;
        float z = ((triPoint.b * aAxis.x) - (bAxis.x * triPoint.a)) / ((bAxis.z * aAxis.x) - (aAxis.z * bAxis.x));
        float x = (triPoint.a - (z * aAxis.z)) / aAxis.x;
        return new Vector3(x, 0, z) * lineLength;
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