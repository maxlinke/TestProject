using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class CubicBezierSpline : BezierSpline {

        [Header("Handles")]
        [SerializeField] public Vector3 localP0;
        [SerializeField] public Vector3 localP1;
        [SerializeField] public Vector3 localP2;
        [SerializeField] public Vector3 localP3;

        public Vector3 p0 => transform.TransformPoint(localP0);
        public Vector3 p1 => transform.TransformPoint(localP1);
        public Vector3 p2 => transform.TransformPoint(localP2);
        public Vector3 p3 => transform.TransformPoint(localP3);

        public override int DEFAULT_LENGTH_CALC_ITERATIONS => 100;
        public override int DEFAULT_NEXT_T_ITERATIONS => 16;
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => 16;

        private Vector3 lastLP0;
        private Vector3 lastLP1;
        private Vector3 lastLP2;
        private Vector3 lastLP3;

        protected override bool PointsChangedSinceLastRecalculation () {
            return lastLP0 != localP0
                || lastLP1 != localP1
                || lastLP2 != localP2
                || lastLP3 != localP3;
        }

        protected override void OnLengthRecalculated () {
            base.OnLengthRecalculated();
            lastLP0 = localP0;
            lastLP1 = localP1;
            lastLP2 = localP2;
            lastLP3 = localP3;
        }

        protected override void Reset () {
            base.Reset();
            localP0 = new Vector3(-7, 0, -4);
            localP1 = new Vector3(-2, 0, 6);
            localP2 = new Vector3(2, 0, -6);
            localP3 = new Vector3(7, 0, 4);
        }

        public static Vector3 BezierPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            float iT = 1f - t;
            return (iT * iT * iT) * p0
                + (3f * iT * iT * t) * p1
                + (3f * iT * t * t) * p2
                + (t * t * t) * p3;
        }

        public override Vector3 BezierPoint (float t) {
            return BezierPoint(this.p0, this.p1, this.p2, this.p3, t);
        }

        public static Vector3 BezierDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            float iT = 1f - t;
            return (3f * iT * iT) * (p1 - p0)
                + (6f * iT * t) * (p2 - p1)
                + (3f * t * t) * (p3 - p2);
        }

        public override Vector3 BezierDerivative (float t) {
            return BezierDerivative(this.p0, this.p1, this.p2, this.p3, t);
        }

        public static Vector3 SecondDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            float iT = 1f - t;
            return (6f * iT) * (p2 - (2f * p1) + p0)
                + (6f * t) * (p3 - (2f * p2) + p1);
        }

        public override Vector3 SecondDerivative (float t) {
            return SecondDerivative(this.p0, this.p1, this.p2, this.p3, t);
        }

        public override void ReverseDirection () {
            Undo.RecordObject(this, "Reverse spline direction");
            var p0Cache = localP0;
            var p1Cache = localP1;
            localP0 = localP3;
            localP1 = localP2;
            localP2 = p1Cache;
            localP3 = p0Cache;
        }

        public override void ApplyScale () {
            if(transform.localScale.Equals(Vector3.one)){
                return;
            }
            Undo.RecordObject(this.transform, "Apply spline scale");
            Undo.RecordObject(this, "Apply spline scale");
            var wp0 = p0;
            var wp1 = p1;
            var wp2 = p2;
            var wp3 = p3;
            this.transform.localScale = Vector3.one;
            localP0 = transform.InverseTransformPoint(wp0);
            localP1 = transform.InverseTransformPoint(wp1);
            localP2 = transform.InverseTransformPoint(wp2);
            localP3 = transform.InverseTransformPoint(wp3);
        }

        protected override IEnumerable<(Vector3, float)> GetWorldSpaceEndPoints () {
            yield return (p0, 0);
            yield return (p3, 1);
        }

        protected override IEnumerable<(Vector3, float)> GetWorldSpaceControlPoints () {
            yield return (p1, 1f / 3f);
            yield return (p2, 2f / 3f);
        }

        protected override IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines  () {
            yield return (p0, p1);
            yield return (p3, p2);
        }

    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(CubicBezierSpline))]
    public class CubicBezierSplineEditor : Editor {

        CubicBezierSpline cbs;
        GUIStyle textStyle;

        void OnEnable () {
            cbs = target as CubicBezierSpline;
            textStyle = BezierSpline.GetHandlesTextStyle();
        }

        void OnSceneGUI () {
            if(!(cbs.showHandles || cbs.showLabels)){
                return;
            }
            var p0 = cbs.p0;
            var p1 = cbs.p1;
            var p2 = cbs.p2;
            var p3 = cbs.p3;
            if(cbs.showHandles){
                EditorGUI.BeginChangeCheck();
                Vector3 newp0 = Handles.PositionHandle(cbs.p0, Quaternion.identity);
                Vector3 newp1 = Handles.PositionHandle(cbs.p1, Quaternion.identity);
                Vector3 newp2 = Handles.PositionHandle(cbs.p2, Quaternion.identity);
                Vector3 newp3 = Handles.PositionHandle(cbs.p3, Quaternion.identity);
                if(EditorGUI.EndChangeCheck()){
                    Undo.RecordObject(cbs, "Change Handle Position");
                    cbs.localP0 = cbs.transform.InverseTransformPoint(newp0);
                    cbs.localP1 = cbs.transform.InverseTransformPoint(newp1);
                    cbs.localP2 = cbs.transform.InverseTransformPoint(newp2);
                    cbs.localP3 = cbs.transform.InverseTransformPoint(newp3);
                }
            }
            if(cbs.showLabels){
                Handles.Label(p0, nameof(cbs.p0).ToUpper(), textStyle);
                Handles.Label(p1, nameof(cbs.p1).ToUpper(), textStyle);
                Handles.Label(p2, nameof(cbs.p2).ToUpper(), textStyle);
                Handles.Label(p3, nameof(cbs.p3).ToUpper(), textStyle);

            }
        }

    }

    #endif

}