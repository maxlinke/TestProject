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

        public override int DEFAULT_LENGTH_CALC_ITERATIONS => 100;
        public override int DEFAULT_NEXT_T_ITERATIONS => 16;
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => 16;

        protected override void Reset () {
            base.Reset();
            localP0 = new Vector3(-5f, 0f, -3f);
            localP1 = new Vector3(0f, 0f, 6f);
            localP2 = new Vector3(5f, 0f, -3f);
        }

        public override Vector3 BezierPoint (float t) {
            float iT = 1f - t;
            return p1 + (iT * iT * (p0 - p1)) + (t * t * (p2 - p1));
        }

        public override Vector3 BezierDerivative (float t) {
            float iT = 1f - t;
            return (2f * iT * (p1 - p0)) + (2f * t * (p2 - p1));
        }

        public override Vector3 SecondDerivative (float t) {
            float iT = 1f - t;
            return 2f * (p2 - (2f * p1) + p0);
        }

        public override void ReverseDirection () {
            Undo.RecordObject(this, "Reverse spline direction");
            var h1Cache = localP0;
            localP0 = localP2;
            localP2 = h1Cache;
        }

        public override void ApplyScale () {
            if(transform.localScale == Vector3.one){
                return;
            }
            Undo.RecordObject(this.transform, "Apply spline scale");
            Undo.RecordObject(this, "Apply spline scale");
            var h1wPos = this.p0;
            var h2wPos = this.p2;
            var chwPos = this.p1;
            this.transform.localScale = Vector3.one;
            this.localP0 = transform.InverseTransformPoint(h1wPos);
            this.localP2 = transform.InverseTransformPoint(h2wPos);
            this.localP1 = transform.InverseTransformPoint(chwPos);
        }

        protected override IEnumerable<Vector3> GetWorldSpaceEndPoints () {
            yield return p0;
            yield return p2;
        }

        protected override IEnumerable<Vector3> GetWorldSpaceControlPoints () {
            yield return p1;
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

        void OnEnable () {
            qbs = target as QuadraticBezierSpline;
        }

        void OnSceneGUI () {
            if(!qbs.showHandles){
                return;
            }
            EditorGUI.BeginChangeCheck();
            Vector3 newH1 = Handles.PositionHandle(qbs.p0, Quaternion.identity);
            Vector3 newH2 = Handles.PositionHandle(qbs.p2, Quaternion.identity);
            Vector3 newCH = Handles.PositionHandle(qbs.p1, Quaternion.identity);
            if(EditorGUI.EndChangeCheck()){
                Undo.RecordObject(qbs, "Change Handle Position");
                qbs.localP0 = qbs.transform.InverseTransformPoint(newH1);
                qbs.localP2 = qbs.transform.InverseTransformPoint(newH2);
                qbs.localP1 = qbs.transform.InverseTransformPoint(newCH);
            }
        }

    }

    #endif

}