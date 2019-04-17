using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TriangleMeshGenerator : MonoBehaviour {

    [SerializeField] MeshFilter mf;
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

    [ContextMenu(nameof(GenerateMesh))]
    void GenerateMesh () {
        var graph = new TriMeshGraph(this);
        var selectedPoints = GetPointsInsideRadius(selectObject.transform.position, selectRadius);
        foreach(var point in selectedPoints){
            graph.AddVertex(point);
        }
        mf.sharedMesh = graph.ToMesh(TriMeshGraph.UVMode.OBJECTNORMALIZED);
    }

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

    public TriMeshPoint WorldXYZtoNearestABC (Vector3 worldPoint) {
        return LocalXYZtoNearestABC(transform.InverseTransformPoint(worldPoint));
    }

    public TriMeshPoint LocalXYZtoNearestABC (Vector3 localPoint) {
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

    public Vector3 ABCtoWorldXYZ (TriMeshPoint triPoint) {
        return transform.TransformPoint(ABCtoLocalXYZ(triPoint));
    }

    public Vector3 ABCtoLocalXYZ (TriMeshPoint triPoint) {
        float z = ((triPoint.b * aAxis.x) - (bAxis.x * triPoint.a)) / ((bAxis.z * aAxis.x) - (aAxis.z * bAxis.x));
        float x = (triPoint.a - (z * aAxis.z)) / aAxis.x;
        return new Vector3(x, 0, z) * lineLength;
    }

    public List<TriMeshPoint> GetPointsInsideRadius (Vector3 worldCenterPoint, float worldRadius) {
        var output = new List<TriMeshPoint>();
        float sqrWorldRadius = worldRadius * worldRadius;
        var triCenter = WorldXYZtoNearestABC(worldCenterPoint);
        if(TriPointIsInRadius(triCenter) && lineLength > 0f){
            output.Add(triCenter);
            int localA = 0;
            int localB = 0;
            int localC = 0;
            int ringIndex = 0;
            int ringIndexLimit = 101;
            while(ringIndex < ringIndexLimit){
                bool atLeastOnePointInRadius = false;
                localA++;
                localB--;
                ringIndex++;
                int pointsOnRing = ringIndex * 6;
                TraceRing();
                if(!atLeastOnePointInRadius){
                    break;
                }

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
            if(ringIndex == ringIndexLimit){
                Debug.LogError("max rings allowed reached!");
            }
        }
        return output;

        bool TriPointIsInRadius (TriMeshPoint triPoint) {
            return ((ABCtoWorldXYZ(triPoint) - worldCenterPoint).sqrMagnitude <= sqrWorldRadius);
        }
    }

    public struct TriMeshPoint {

        public readonly int a;
        public readonly int b;
        public int c => -(a+b);

        public static bool operator == (TriMeshPoint point1, TriMeshPoint point2) {
            return point1.Equals(point2);
        }

        public static bool operator != (TriMeshPoint point1, TriMeshPoint point2) {
            return !point1.Equals(point2);
        }

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

        public bool Neighbors (TriMeshPoint other) {
            if(other == this){
                return false;
            }
            int aDelta = Mathf.Abs(other.a - this.a);
            int bDelta = Mathf.Abs(other.b - this.b);
            int cDelta = Mathf.Abs(other.c - this.c);
            if(aDelta > 1 || bDelta > 1 || cDelta > 1){
                return false;
            }
            return ((aDelta + bDelta + cDelta) == 2);
        }

        public override bool Equals (object obj) {
            if(obj is TriMeshPoint other){
                return ((other.a == this.a) && (other.b == this.b));
            }
            return false;
        }

        public override int GetHashCode () {
            return 2179 * a + 3853 * b + 4967 * c;  //just some prime numbers
        }

    }

    class TriMeshGraph {

        //TODO use normal mesh graph to implement some sort of "patch holes" thingy...
        //or maybe not. here i pretty much know that any vertex should have 6 neighbors unless it is at an edge...

        public enum UVMode {
            WORLDCOORDS,
            OBJECTNORMALIZED
        }

        public class TriMeshGraphVertex {

            public readonly TriMeshPoint point;
            private readonly List<TriMeshGraphVertex> connected;

            public TriMeshGraphVertex (TriMeshPoint point) {
                this.point = point;
                connected = new List<TriMeshGraphVertex>();
            }

            public void ClearConnections () {
                connected.Clear();
            }

            public void AddConnected (TriMeshGraphVertex other) {
                if(IsConnectedTo(other)){
                    Debug.LogError("duplicate connection, aborting");
                    return;
                }
                connected.Add(other);
            }

            public bool IsConnectedTo (TriMeshGraphVertex other) {
                return connected.Contains(other);
            }

        }

        private readonly TriangleMeshGenerator generator;
        private readonly List<TriMeshGraphVertex> vertices;

        public TriMeshGraph (TriangleMeshGenerator generator) {
            this.generator = generator;
            vertices = new List<TriMeshGraphVertex>();
        }

        public TriMeshGraphVertex AddVertex (TriMeshPoint point) {
            if(HasVertex(point, out _)){
                Debug.LogError("duplicate vertices not allowed!");
                return null;
            }
            var newVert = new TriMeshGraphVertex(point);
            vertices.Add(newVert);
            return newVert;
        }

        public bool HasVertex (TriMeshPoint point, out TriMeshGraphVertex outputVert) {
            foreach(var vert in vertices){
                if(vert.point == point){
                    outputVert = vert;
                    return true;
                }
            }
            outputVert = null;
            return false;
        }

        void RegenerateConnections () {
            foreach(var vert in vertices){
                vert.ClearConnections();
            }
            for(int i=0; i<vertices.Count; i++){
                var vert = vertices[i];
                for(int j=i+1; j<vertices.Count; j++){
                    var otherVert = vertices[j];
                    if(!vert.IsConnectedTo(otherVert) && vert.point.Neighbors(otherVert.point)){
                        vert.AddConnected(otherVert);
                        otherVert.AddConnected(vert);
                    }
                }
            }
        }

        public Mesh ToMesh (UVMode uvMode) {
            RegenerateConnections();
            Vector3[] meshVertices = new Vector3[vertices.Count];
            Vector2[] meshTexcoords = new Vector2[vertices.Count];
            var triangleList = new List<(int, int, int)>();
            for(int i=0; i<vertices.Count; i++){
                var vert = vertices[i];
                meshVertices[i] = generator.ABCtoLocalXYZ(vert.point);      //maybe start off with local (for tex coords) then before the end transform them into world space?
                for(int j=i+1; j<vertices.Count; j++){
                    var otherVert = vertices[j];
                    if(vert.IsConnectedTo(otherVert)){
                        for(int k=i+2; k<vertices.Count; k++){
                            var otherOtherVert = vertices[k];
                            if(vert.IsConnectedTo(otherOtherVert) && otherVert.IsConnectedTo(otherOtherVert)){
                                triangleList.Add((i, j, k));
                            }
                        }
                    }
                }
            }
            int[] meshTriangles = new int[3 * triangleList.Count];
            for(int i=0; i<triangleList.Count; i++){
                int index1 = triangleList[i].Item1;
                int index2 = triangleList[i].Item2;
                int index3 = triangleList[i].Item3;
                Vector3 p1 = meshVertices[index1];
                Vector3 p2 = meshVertices[index2];
                Vector3 p3 = meshVertices[index3];
                float orientation = (p2.z - p1.z) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.z - p2.z);
                int i3 = i * 3;
                if(orientation > 0){
                    meshTriangles[i3 + 0] = index1;
                    meshTriangles[i3 + 1] = index2;
                    meshTriangles[i3 + 2] = index3;
                }else{
                    meshTriangles[i3 + 0] = index1;
                    meshTriangles[i3 + 1] = index3;
                    meshTriangles[i3 + 2] = index2;
                }
            }
            for(int i=0; i<meshVertices.Length; i++){
                meshVertices[i] = generator.transform.TransformPoint(meshVertices[i]);
            }
            Mesh outputMesh = new Mesh();
            outputMesh.vertices = meshVertices;
            outputMesh.uv = meshTexcoords;
            outputMesh.triangles = meshTriangles;
            outputMesh.normals = new Vector3[vertices.Count];
            outputMesh.tangents = new Vector4[vertices.Count];
            outputMesh.RecalculateBounds();
            outputMesh.RecalculateNormals();
            outputMesh.RecalculateTangents();
            return outputMesh;
        }

    }

}