using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class QuadraticBezierSpline : BezierSpline {

        [Header("Handles")]
        [SerializeField] public Vector3 handle1;
        [SerializeField] public Vector3 handle2;
        [SerializeField] public Vector3 controlHandle;

        public Vector3 p1 => transform.TransformPoint(handle1);
        public Vector3 p2 => transform.TransformPoint(handle2);
        public Vector3 pC => transform.TransformPoint(controlHandle);

        public override int DEFAULT_LENGTH_CALC_ITERATIONS => 100;
        public override int DEFAULT_NEXT_T_ITERATIONS => 16;
        public override int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION => 16;

        protected override void Reset () {
            base.Reset();
            handle1 = new Vector3(-5f, 0f, -3f);
            handle2 = new Vector3(5f, 0f, -3f);
            controlHandle = new Vector3(0f, 0f, 6f);
        }

        public override Vector3 BezierPoint (float t) {
            float iT = 1f - t;
            return pC + (iT * iT * (p1 - pC)) + (t * t * (p2 - pC));
        }

        public override Vector3 BezierDerivative (float t) {
            float iT = 1f - t;
            return (2f * iT * (pC - p1)) + (2f * t * (p2 - pC));
        }

        public override Vector3 SecondDerivative (float t) {
            float iT = 1f - t;
            return 2f * (p2 - (2f * pC) + p1);
        }

        public override void ReverseDirection () {      // TODO is all this editor stuff really necessary?
            // #if UNITY_EDITOR
            //     if(!(EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPaused)){
            //         Undo.RecordObject(this, "Reversing Direction");
            //     }
            // #endif
            var h1Cache = handle1;
            handle1 = handle2;
            handle2 = h1Cache;
        }

        public override void ApplyScale () {
            if(transform.localScale == Vector3.one){
                return;
            }
            #if UNITY_EDITOR
                Undo.RecordObject(this.transform, "Applying spline localscale");
                Undo.RecordObject(this, "Applying spline localscale");
            #endif
            var h1wPos = this.p1;
            var h2wPos = this.p2;
            var chwPos = this.pC;
            this.transform.localScale = Vector3.one;
            this.handle1 = transform.InverseTransformPoint(h1wPos);
            this.handle2 = transform.InverseTransformPoint(h2wPos);
            this.controlHandle = transform.InverseTransformPoint(chwPos);
        }

        protected override IEnumerable<Vector3> GetWorldSpaceEndPoints () {
            yield return p1;
            yield return p2;
        }

        protected override IEnumerable<Vector3> GetWorldSpaceControlPoints () {
            yield return pC;
        }

        protected override IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines  () {
            yield return (p1, pC);
            yield return (p2, pC);
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
            Vector3 newH1 = Handles.PositionHandle(qbs.p1, Quaternion.identity);
            Vector3 newH2 = Handles.PositionHandle(qbs.p2, Quaternion.identity);
            Vector3 newCH = Handles.PositionHandle(qbs.pC, Quaternion.identity);
            if(EditorGUI.EndChangeCheck()){
                Undo.RecordObject(qbs, "Change Handle Position");
                qbs.handle1 = qbs.transform.InverseTransformPoint(newH1);
                qbs.handle2 = qbs.transform.InverseTransformPoint(newH2);
                qbs.controlHandle = qbs.transform.InverseTransformPoint(newCH);
            }
        }

    }

    #endif

}