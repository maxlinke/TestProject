using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class ContinuousBezierSpline : BezierSpline {

        public override int DEFAULT_LENGTH_CALC_ITERATIONS => 100;
        public override int DEFAULT_NEXT_T_ITERATIONS => 16;
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => 16;

        [SerializeField] int selectedPoint; // TEMP TEMP TEMP
        [SerializeField] List<Point> points;

        public int PointCount => points.Count;
        public int SegmentCount => PointCount - 1;
        public Point this[int index] => points[index];

        private Point DefaultPoint () {
            var pos = new Vector3(-7,0,-4);
            var handle = new Vector3(5, 0, 10);
            return new Point(Point.Type.SMOOTH, pos, handle, -handle);
        }

        private void ValidatePointsList () {
            if(points == null){
                points = new List<Point>();
            }
            if(points.Count < 1){
                points.Add(DefaultPoint());
            }
            if(points.Count < 2){
                var p0 = points[0];
                var pos = p0.pos;
                var h1 = p0.handleFwd;
                points.Add(new Point(Point.Type.SMOOTH, -pos, h1, -h1));
            }
        }

        protected override void Reset () {
            base.Reset();
            points.Clear();
            ValidatePointsList();
        }

        void OnValidate () {
            ValidatePointsList();
        }

        protected override bool PointsChangedSinceLastRecalculation()
        {
            // throw new System.NotImplementedException();
            return true;
        }

        private void GetProperTAndPoints (float inputT, out float outputT, out Point outP1, out Point outP2) {
            var fullT = inputT * SegmentCount;
            var floor = Mathf.FloorToInt(fullT);
            var ceil = floor + 1;
            if(floor < 0){
                floor = 0;
                ceil = 1;
            }
            if(ceil > SegmentCount){
                ceil = SegmentCount;
                floor = SegmentCount - 1;
            }
            outputT = fullT + floor;
            outP1 = points[floor];
            outP2 = points[ceil];
        }

        private void WorldPoints (Point point, out Vector3 wPos, out Vector3 wHFwd, out Vector3 wHBwd) {
            wPos = transform.TransformPoint(point.pos);
            wHFwd = wPos + transform.TransformVector(point.handleFwd);
            wHBwd = wPos + transform.TransformVector(point.handleBwd);
        }

        public override Vector3 BezierPoint (float t) {
            GetProperTAndPoints(t, out var localT, out var point1, out var point2);
            WorldPoints(point1, out var p0, out var p1, out _);
            WorldPoints(point2, out var p3, out _, out var p2);
            return CubicBezierSpline.BezierPoint(p0, p1, p2, p3, localT);
        }

        public override Vector3 BezierDerivative (float t) {
            GetProperTAndPoints(t, out var localT, out var point1, out var point2);
            WorldPoints(point1, out var p0, out var p1, out _);
            WorldPoints(point2, out var p3, out _, out var p2);
            return CubicBezierSpline.BezierDerivative(p0, p1, p2, p3, localT);
        }

        public override Vector3 SecondDerivative (float t) {
            GetProperTAndPoints(t, out var localT, out var point1, out var point2);
            WorldPoints(point1, out var p0, out var p1, out _);
            WorldPoints(point2, out var p3, out _, out var p2);
            return CubicBezierSpline.SecondDerivative(p0, p1, p2, p3, localT);
        }

        public override void ReverseDirection () {
            // throw new System.NotImplementedException();
        }

        public override void ApplyScale () {
            if(transform.localScale.Equals(Vector3.one)){
                return;
            }
            Undo.RecordObject(this.transform, "Apply spline scale");
            Undo.RecordObject(this, "Apply spline scale");
            var wPoints = new List<(Vector3, Vector3, Vector3)>();
            foreach(var point in points){
                var wPos = transform.TransformPoint(point.pos);
                var wFwd = transform.TransformPoint(point.handleFwd);
                var wBwd = transform.TransformPoint(point.handleBwd);
                wPoints.Add((wPos, wFwd, wBwd));
            }
            this.transform.localScale = Vector3.one;
            for(int i=0; i<PointCount; i++){
                var point = points[i];
                point.pos = transform.InverseTransformPoint(wPoints[i].Item1);
                point.handleFwd = transform.InverseTransformPoint(wPoints[i].Item2);
                point.handleBwd = transform.InverseTransformPoint(wPoints[i].Item3);
            }
        }

        protected override IEnumerable<Vector3> GetWorldSpaceEndPoints () {
            WorldPoints(points[0], out var p0, out _, out _);
            WorldPoints(points[PointCount-1], out var p1, out _, out _);
            yield return p0;
            yield return p1;
        }

        private bool SelectionIndexIsValid () {
            return !(selectedPoint < 0 || selectedPoint >= PointCount);
        }

        protected override IEnumerable<Vector3> GetWorldSpaceControlPoints () {
            if(SelectionIndexIsValid()){
                WorldPoints(points[selectedPoint], out var _, out var h1, out var h2);
                yield return h1;
                yield return h2;
            }
        }

        protected override IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines () {
            if(SelectionIndexIsValid()){
                WorldPoints(points[selectedPoint], out var p, out var h1, out var h2);
                yield return (p, h1);
                yield return (p, h2);
            }
        }

        [System.Serializable]
        public class Point {

            public enum Type {
                SMOOTH,
                BROKEN
            }

            [SerializeField] private Type m_type;
            [SerializeField] private Vector3 m_pos;
            [SerializeField] private Vector3 m_handleFwd;
            [SerializeField] private Vector3 m_handleBwd;

            public Type type { get => m_type ; set {
                m_type = value;
                switch(value){
                    case Type.SMOOTH:
                        handleFwd = handleFwd;  // recalc handleback
                        break;
                    case Type.BROKEN:
                        // nothing to do here
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Vector3 pos { get => m_pos; set {
                pos = value;
            }}

            public Vector3 handleFwd { get => m_handleFwd; set {
                m_handleFwd = value;
                switch(type){
                    case Type.SMOOTH:
                        var bwdMag = handleBwd.magnitude;
                        m_handleBwd = -bwdMag * value.normalized;
                        break;
                    case Type.BROKEN:
                        // nothing to do here
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Vector3 handleBwd { get => m_handleBwd; set {
                m_handleBwd = value;
                switch(type){
                    case Type.SMOOTH:
                        var fwdMag = handleFwd.magnitude;
                        m_handleFwd = -fwdMag * value.normalized;
                        break;
                    case Type.BROKEN:
                        // nothing to do here
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Point () {
                m_type = Type.SMOOTH;
                m_pos = Vector3.zero;
                m_handleFwd = Vector3.one;
                m_handleBwd = -Vector3.one;
            }

            public Point (Type type, Vector3 pos, Vector3 handleFwd, Vector3 handleBwd) {
                m_type = type;
                m_pos = pos;
                // m_handleFwd = handleFwd;
                m_handleBwd = handleBwd;
                this.handleFwd = handleFwd;
            }

            public override bool Equals (object obj) {
                if(obj is Point other){
                    if(other.pos == this.pos){
                        if(other.type == this.type){
                            if(other.handleFwd == this.handleFwd && other.handleBwd == this.handleBwd){
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public override int GetHashCode () {
                return m_type.GetHashCode() + m_pos.GetHashCode() + m_handleFwd.GetHashCode() + m_handleBwd.GetHashCode();
            }

        }

    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(ContinuousBezierSpline))]
    public class ContinuousBezierSplineEditor : Editor {

        ContinuousBezierSpline cbs;

        void OnEnable () {
            cbs = target as ContinuousBezierSpline;
        }

        // void OnSceneGUI () {

        // }

        public override void OnInspectorGUI () {
            // GUI.enabled = false;
            // EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(cbs), typeof(ContinuousBezierSpline), false);
            // GUI.enabled = true;
            DrawDefaultInspector();
        }

    }

    #endif

}