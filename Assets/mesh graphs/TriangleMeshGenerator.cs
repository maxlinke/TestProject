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
            foreach(var point in referencePoints){
                Gizmos.DrawSphere(transform.TransformPoint(ABCtoLocalXYZ(point)), 0.25f);
            }
        }
        for(int i=-debugResolution; i<=debugResolution; i++){
            for(int j=-debugResolution; j<=debugResolution; j++){
                Vector3 debugPoint = selectionCenter + (debugSize * new Vector3(i, 0, j) / debugResolution);
                var approx = LocalXYZtoNearestABC(transform.InverseTransformPoint(debugPoint));
                byte r = (byte)((71 * approx.a) % 255);
                byte g = (byte)((137 * approx.b) % 255);
                byte b = (byte)((231 * approx.a + 197 * approx.b) % 255);
                Gizmos.color = new Color32(r, g, b, (byte)(255));
                Gizmos.DrawSphere(debugPoint, 0.05f);
            }
        }
    }

    TriMeshPoint LocalXYZtoNearestABC (Vector3 localPoint) {
        localPoint /= lineLength;
        int a = Mathf.FloorToInt(Vector3.Dot(localPoint, aAxis) + 0.5f);
        int b = Mathf.FloorToInt(Vector3.Dot(localPoint, bAxis) + 0.5f);
        return new TriMeshPoint(a, b);
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