using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    public class ContinuousBezierSpline : BezierSpline {

        [Header("Spline")]
        [SerializeField] public bool cyclic;
        [SerializeField] List<Point> points;

        public int PointCount => points.Count;
        public int SegmentCount => PointCount - (cyclic ? 0 : 1);
        public Point this[int index] => points[index];
        
        public override int DEFAULT_LENGTH_CALC_ITERATIONS => 64 * SegmentCount;
        public override int DEFAULT_NEXT_T_ITERATIONS => 16;
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => 16;

        List<Point> lastRecalcPoints;

        private Point DefaultPoint () {
            var pos = new Vector3(-7,0,-4);
            var handle = new Vector3(5, 0, 10);
            return new Point(Point.Type.Smooth, pos, handle, -handle);
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
                var pos = p0.position;
                var h1 = p0.handleFwd;
                points.Add(new Point(Point.Type.Smooth, -pos, h1, -h1));
            }
        }

        protected override void Reset () {
            base.Reset();
            points?.Clear();
            ValidatePointsList();
        }

        void OnValidate () {
            ValidatePointsList();
        }

        protected override bool PointsChangedSinceLastRecalculation () {
            ValidatePointsList();
            if(lastRecalcPoints == null){
                lastRecalcPoints = new List<Point>();
            }
            if(lastRecalcPoints.Count != points.Count){
                return true;
            }
            for(int i=0; i<PointCount; i++){
                if(!points[i].Equals(lastRecalcPoints[i])){
                    return true;
                }
            }
            return false;
        }

        protected override void OnLengthRecalculated () {
            base.OnLengthRecalculated();
            ValidatePointsList();
            if(lastRecalcPoints == null){
                lastRecalcPoints = new List<Point>();
            }
            lastRecalcPoints.Clear();
            foreach(var point in points){
                lastRecalcPoints.Add(new Point(point));
            }
        }

        private void GetProperTAndPoints (float inputT, out float outputT, out Point outP1, out Point outP2) {
            var fullT = inputT * SegmentCount;
            var floor = Mathf.FloorToInt(fullT);
            var ceil = floor + 1;
            int p1, p2;
            if(cyclic){
                p1 = floor % SegmentCount;
                if(p1 < 0){
                    p1 += SegmentCount;
                }
                p2 = ceil % SegmentCount;
                if(p2 < 0){
                    p2 += SegmentCount;
                }
            }else{
                if(floor < 0){
                    floor = 0;
                    ceil = 1;
                }else if(ceil > SegmentCount){
                    ceil = SegmentCount;
                    floor = SegmentCount - 1;
                }
                p1 = floor;
                p2 = ceil;
            }
            outputT = fullT - floor;
            outP1 = points[p1];
            outP2 = points[p2];
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

#region Gizmos

        protected override IEnumerable<(Vector3, float)> GetWorldSpaceEndPoints () {
            // foreach(var point in points){
            for(int i=0; i<PointCount; i++){
                var point = points[i];
                WorldPoints(point, out var p, out _, out _);
                yield return (p, ((float)i) / SegmentCount);
            }
        }

        protected override IEnumerable<(Vector3, float)> GetWorldSpaceControlPoints () {
            // foreach(var point in points){
            for(int i=0; i<PointCount; i++){
                var point = points[i];
                WorldPoints(point, out _, out var h1, out var h2);
                yield return (h1, ((float)i) / SegmentCount);
                yield return (h2, ((float)i) / SegmentCount);
            }
        }

        protected override IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines () {
            foreach(var point in points){
                WorldPoints(point, out var p, out var h1, out var h2);
                yield return (p, h1);
                yield return (p, h2);
            }
        }

#endregion

        public Vector3 WorldPoint (Point point) {
            return transform.TransformPoint(point.position);
        }

        public void WorldPoints (Point point, out Vector3 wPos, out Vector3 wHFwd, out Vector3 wHBwd) {
            wPos = transform.TransformPoint(point.position);
            wHFwd = wPos + transform.TransformVector(point.handleFwd);
            wHBwd = wPos + transform.TransformVector(point.handleBwd);
        }

        public Vector3 LocalPointPos (Vector3 wPos) {
            return transform.InverseTransformPoint(wPos);
        }

        public Vector3 PointSpaceHandlePos (Point point, Vector3 wHandlePos) {
            return transform.InverseTransformVector(wHandlePos - transform.TransformPoint(point.position));
        }

        private bool IndexCheckAndComplain (int inputIndex) {
            if(inputIndex < 0){
                Debug.LogError("Negative indices are not allowed!");
                return false;
            }
            if(inputIndex >= PointCount){
                Debug.LogError("Index out of bounds!");
                return false;
            }
            return true;
        }

        public void AddPointAfter (int insertIndex) {
            if(!IndexCheckAndComplain(insertIndex)){
                return;
            }
            Point prev = points[insertIndex];
            if((insertIndex + 1) < PointCount){
                var next = points[insertIndex+1];
                WorldPoints(prev, out var p0, out var p1, out _);
                WorldPoints(next, out var p3, out _, out var p2);
                var bPoint = CubicBezierSpline.BezierPoint(p0, p1, p2, p3, 0.5f);
                var bFwd = CubicBezierSpline.BezierDerivative(p0, p1, p2, p3, 0.5f);
                var newPoint = new Point(
                    type: Point.Type.Smooth, 
                    pos: LocalPointPos(bPoint),
                    handleFwd: Vector3.one,
                    handleBwd: -Vector3.one
                );
                var newHandle = PointSpaceHandlePos(newPoint, bPoint + bFwd);
                newHandle = newHandle.normalized * Mathf.Min(prev.handleFwd.magnitude, next.handleBwd.magnitude) * 0.2f;
                newPoint.handleBwd = -newHandle;
                newPoint.handleFwd = newHandle;
                points.Insert(insertIndex + 1, newPoint);
            }else{
                WorldPoints(prev, out var wPrevPos, out var wPrevHFwd, out _);
                points.Insert(insertIndex + 1, new Point(
                    type: Point.Type.Smooth,
                    pos: LocalPointPos(wPrevPos + 3f * (wPrevHFwd - wPrevPos)),
                    handleFwd: prev.handleFwd,
                    handleBwd: -prev.handleFwd
                ));
            }
        }

        public void DeletePoint (int deleteIndex) {
            if(!IndexCheckAndComplain(deleteIndex)){
                return;
            }
            points.RemoveAt(deleteIndex);
            ValidatePointsList();
        }

        public void MovePointIndex (int startIndex, int delta) {
            if(!IndexCheckAndComplain(startIndex)){
                return;
            }
            int endIndex = Mathf.Min(PointCount - 1, Mathf.Max(0, startIndex + delta));
            var movePoint = points[startIndex];
            points.RemoveAt(startIndex);
            points.Insert(endIndex, movePoint);
        }

        public override void ReverseDirection () {
            var newPoints = new List<Point>();
            int loopStart, loopEnd;
            if(cyclic){
                var startPoint = points[0];
                newPoints.Add(new Point(
                    type: startPoint.type,
                    pos: startPoint.position,
                    handleFwd: startPoint.handleBwd,
                    handleBwd: startPoint.handleFwd
                ));
                loopStart = PointCount-1;
                loopEnd = 1;
            }else{
                loopStart = PointCount-1;
                loopEnd = 0;
            }
            for(int i=loopStart; i>=loopEnd; i--){
                var origPoint = points[i];
                var newPoint = new Point(
                    type: origPoint.type,
                    pos: origPoint.position,
                    handleFwd: origPoint.handleBwd,
                    handleBwd: origPoint.handleFwd
                );
                newPoints.Add(newPoint);
            }
            this.points = newPoints;
        }

        void ChangeTransformButKeepPoints (System.Action<Vector3> changeTransform) {
            var wPoints = new List<(Vector3, Vector3, Vector3)>();
            var pointSum = Vector3.zero;
            foreach(var point in points){
                WorldPoints(point, out var wPos, out var wHFwd, out var wHBwd);
                wPoints.Add((wPos, wHFwd, wHBwd));
                pointSum += wPos;
            }
            changeTransform(pointSum);
            for(int i=0; i<PointCount; i++){
                var point = points[i];
                point.position = LocalPointPos(wPoints[i].Item1);
                point.handleFwd = PointSpaceHandlePos(point, wPoints[i].Item2);
                point.handleBwd = PointSpaceHandlePos(point, wPoints[i].Item3);
            }
        }

        public override void ApplyScale () {
            if(transform.localScale.Equals(Vector3.one)){
                return;
            }
            ChangeTransformButKeepPoints((ps) => {this.transform.localScale = Vector3.one;});
        }

        public override void MovePositionToAveragePoint () {
            ChangeTransformButKeepPoints((ps) => {this.transform.position = ps / PointCount;});
        }

        [System.Serializable]
        public class Point {

            public enum Type {
                Smooth,
                Broken
            }

            [SerializeField] private Type m_type;
            [SerializeField] private Vector3 m_position;
            [SerializeField] private Vector3 m_handleFwd;
            [SerializeField] private Vector3 m_handleBwd;

            public Type type { get => m_type ; set {
                m_type = value;
                switch(value){
                    case Type.Smooth:
                        handleFwd = handleFwd;  // recalc handleback
                        break;
                    case Type.Broken:
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Vector3 position { get => m_position; set {
                m_position = value;
            }}

            public Vector3 handleFwd { get => m_handleFwd; set {
                m_handleFwd = value;
                switch(type){
                    case Type.Smooth:
                        var bwdMag = handleBwd.magnitude;
                        m_handleBwd = -bwdMag * value.normalized;
                        break;
                    case Type.Broken:
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Vector3 handleBwd { get => m_handleBwd; set {
                m_handleBwd = value;
                switch(type){
                    case Type.Smooth:
                        var fwdMag = handleFwd.magnitude;
                        m_handleFwd = -fwdMag * value.normalized;
                        break;
                    case Type.Broken:
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Point () {
                m_type = Type.Smooth;
                m_position = Vector3.zero;
                m_handleFwd = Vector3.one;
                m_handleBwd = -Vector3.one;
            }

            public Point (Type type, Vector3 pos, Vector3 handleFwd, Vector3 handleBwd) {
                m_type = type;
                m_position = pos;
                m_handleBwd = handleBwd;
                this.handleFwd = handleFwd;
            }

            public Point (Point other) {
                m_type = other.type;
                m_position = other.position;
                m_handleFwd = other.handleFwd;
                m_handleBwd = other.handleBwd;
            }

            public override bool Equals (object obj) {
                if(obj is Point other){
                    if(other.position == this.position){
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
                return m_type.GetHashCode() + m_position.GetHashCode() + m_handleFwd.GetHashCode() + m_handleBwd.GetHashCode();
            }

        }

    }

}