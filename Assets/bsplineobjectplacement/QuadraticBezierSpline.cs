using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class QuadraticBezierSpline : BezierSpline {

        [Header("Handles")]
        [SerializeField] public Vector3 localP0;
        [SerializeField] public Vector3 localP1;
        [SerializeField] public Vector3 localP2;

        public Vector3 p0 => transform.TransformPoint(localP0);
        public Vector3 p1 => transform.TransformPoint(localP1);
        public Vector3 p2 => transform.TransformPoint(localP2);

        public override int DEFAULT_LENGTH_CALC_ITERATIONS => 64;
        public override int DEFAULT_NEXT_T_ITERATIONS => 16;
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => 16;

        private Vector3 lastLP0;
        private Vector3 lastLP1;
        private Vector3 lastLP2;

        protected override bool PointsChangedSinceLastRecalculation () {
            return lastLP0 != localP0
                || lastLP1 != localP1
                || lastLP2 != localP2;
        }

        protected override void OnLengthRecalculated () {
            base.OnLengthRecalculated();
            lastLP0 = localP0;
            lastLP1 = localP1;
            lastLP2 = localP2;
        }

        protected override void Reset () {
            base.Reset();
            localP0 = new Vector3(-5f, 0f, -3f);
            localP1 = new Vector3(0f, 0f, 6f);
            localP2 = new Vector3(5f, 0f, -3f);
        }

        public static Vector3 BezierPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            float iT = 1f - t;
            return p1 + (iT * iT * (p0 - p1)) + (t * t * (p2 - p1));
        }

        public override Vector3 BezierPoint (float t) {
            return BezierPoint(this.p0, this.p1, this.p2, t);
        }

        public static Vector3 BezierDerivative (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            float iT = 1f - t;
            return (2f * iT * (p1 - p0)) + (2f * t * (p2 - p1));
        }

        public override Vector3 BezierDerivative (float t) {
            return BezierDerivative(this.p0, this.p1, this.p2, t);
        }

        public static Vector3 SecondDerivative (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            float iT = 1f - t;
            return 2f * (p2 - (2f * p1) + p0);
        }

        public override Vector3 SecondDerivative (float t) {
            return SecondDerivative(this.p0, this.p1, this.p2, t);
        }

        public override void ReverseDirection () {
            Undo.RecordObject(this, "Reverse spline direction");
            var h1Cache = localP0;
            localP0 = localP2;
            localP2 = h1Cache;
        }

        void ChangeTransformButKeepPoints (System.Action<Vector3> changeTransform) {
            var wp0 = p0;
            var wp1 = p1;
            var wp2 = p2;
            changeTransform(wp0 + wp1 + wp2);
            localP0 = transform.InverseTransformPoint(wp0);
            localP1 = transform.InverseTransformPoint(wp1);
            localP2 = transform.InverseTransformPoint(wp2);
        }

        public override void ApplyScale () {
            if(transform.localScale == Vector3.one){
                return;
            }
            Undo.RecordObject(this.transform, "Apply spline scale");
            Undo.RecordObject(this, "Apply spline scale");
            ChangeTransformButKeepPoints((ps) => {this.transform.localScale = Vector3.one;});
        }

        public override void MovePositionToAveragePoint () {
            Undo.RecordObject(this.transform, "Move position to average point");
            Undo.RecordObject(this, "Move position to average point");
            ChangeTransformButKeepPoints((ps) => {this.transform.position = ps / 3f;});
        }

        protected override IEnumerable<(Vector3, float)> GetWorldSpaceEndPoints () {
            yield return (p0, 0);
            yield return (p2, 1);
        }

        protected override IEnumerable<(Vector3, float)> GetWorldSpaceControlPoints () {
            yield return (p1, 0.5f);
        }

        protected override IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines  () {
            yield return (p0, p1);
            yield return (p2, p1);
        }
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(QuadraticBezierSpline))]
    public class QuadraticBezierSplineEditor : Editor {

        QuadraticBezierSpline qbs;
        GUIStyle textStyle;

        void OnEnable () {
            qbs = target as QuadraticBezierSpline;
            textStyle = BezierSpline.GetHandlesTextStyle();
        }

        void OnSceneGUI () {
            if(!(qbs.showHandles || qbs.showLabels)){
                return;
            }
            var p0 = qbs.p0;
            var p1 = qbs.p1;
            var p2 = qbs.p2;
            if(qbs.showHandles){
                EditorGUI.BeginChangeCheck();
                Vector3 newp0 = Handles.PositionHandle(qbs.p0, Quaternion.identity);
                Vector3 newp1 = Handles.PositionHandle(qbs.p1, Quaternion.identity);
                Vector3 newp2 = Handles.PositionHandle(qbs.p2, Quaternion.identity);
                if(EditorGUI.EndChangeCheck()){
                    Undo.RecordObject(qbs, "Change Handle Position");
                    qbs.localP0 = qbs.transform.InverseTransformPoint(newp0);
                    qbs.localP1 = qbs.transform.InverseTransformPoint(newp1);
                    qbs.localP2 = qbs.transform.InverseTransformPoint(newp2);
                }
            }
            if(qbs.showLabels){
                Handles.Label(p0, nameof(qbs.p0).ToUpper(), textStyle);
                Handles.Label(p1, nameof(qbs.p1).ToUpper(), textStyle);
                Handles.Label(p2, nameof(qbs.p2).ToUpper(), textStyle);

            }
        }

    }

    #endif

}
