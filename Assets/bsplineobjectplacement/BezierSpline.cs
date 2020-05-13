using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public abstract class BezierSpline : MonoBehaviour {

        public const float MIN_GIZMO_SIZE = 0.05f;
        public const float MAX_GIZMO_SIZE = 5f;

        [SerializeField] public bool showHandles;
        [SerializeField] public bool alwaysDrawGizmos;
        [SerializeField] public float gizmoSize;

        public abstract int DEFAULT_LENGTH_CALC_ITERATIONS { get; }
        public abstract int DEFAULT_NEXT_T_ITERATIONS { get; }
        public abstract int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION { get; }

        private float m_length;
        private Vector3 lastLossyScale;

        protected virtual void Reset () {
            showHandles = true;
            alwaysDrawGizmos = true;
            gizmoSize = 0.5f;
        }

        public abstract Vector3 BezierPoint (float t);

        public abstract Vector3 BezierDerivative (float t);

        public abstract Vector3 SecondDerivative (float t);

        public abstract void ReverseDirection ();

        public abstract void ApplyScale ();

        public float Length { get {
            if(NeedToRecalculateLength()){
                m_length = CalculateLength();
                OnLengthRecalculated();
            }
            return m_length;
        }}

        private bool NeedToRecalculateLength () {
            return (transform.localScale != lastLossyScale) || PointsChangedSinceLastRecalculation();
        }

        protected abstract bool PointsChangedSinceLastRecalculation ();

        protected virtual void OnLengthRecalculated () {
            lastLossyScale = transform.lossyScale;
        }

#region Gizmos

        protected abstract IEnumerable<Vector3> GetWorldSpaceEndPoints ();

        protected abstract IEnumerable<Vector3> GetWorldSpaceControlPoints ();

        protected abstract IEnumerable<(Vector3, Vector3)> GetWorldSpaceHandleLines ();

        protected Color GetGizmoColor () {
            int hash = 0;
            foreach(var ch in gameObject.name){
                hash += 17 * ch;
            }
            hash = Mathf.Abs(hash);
            float hue = (float)(hash % 100) / 100;
            float saturation = (float)(hash % 90) / 90;
            saturation = 0.25f + 0.5f * saturation;
            float value = (float)(hash % 70) / 70;
            value = 0.667f + 0.333f * value;
            return Color.HSVToRGB(hue, saturation, value);
        }

        void GizmoLine (float stepSize, System.Action<Vector3> onStep) {
            float l = Length;
            if(l > 0){
                float t = 0f;
                onStep(BezierPoint(t));
                while(t < 1f){
                    t = Mathf.Clamp01(NextTFromEuclidianDistance(t, stepSize, 10, l));
                    onStep(BezierPoint(t));
                }
            }
        }

        void OnDrawGizmos () {
            gizmoSize = Mathf.Clamp(gizmoSize, MIN_GIZMO_SIZE, MAX_GIZMO_SIZE);
            if(!alwaysDrawGizmos){
                return;
            }
            var colorChache = Gizmos.color;
            Gizmos.color = GetGizmoColor();
            foreach(var p in GetWorldSpaceEndPoints()){
                Gizmos.DrawSphere(p, gizmoSize);
            }
            Vector3 lastPoint = BezierPoint(0);
            GizmoLine(2f * gizmoSize, (newPoint) => {
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            });
            Gizmos.color = colorChache;
        }

        void OnDrawGizmosSelected () {
            if(UnityEditor.Selection.activeGameObject != this.gameObject){
                return;
            }
            var gizmoCache = alwaysDrawGizmos;
            alwaysDrawGizmos = true;
            OnDrawGizmos();
            alwaysDrawGizmos = gizmoCache;
            var colorChache = Gizmos.color;
            Gizmos.color = GetGizmoColor();
            foreach(var p in GetWorldSpaceControlPoints()){
                Gizmos.DrawSphere(p, gizmoSize);
            }
            foreach(var l in GetWorldSpaceHandleLines()){
                Gizmos.DrawLine(l.Item1, l.Item2);
            }
            float stepSize = 3f * gizmoSize;
            float objectSize = 0.33f * gizmoSize;
            GizmoLine(stepSize, (newPoint) => {
                Gizmos.DrawSphere(newPoint, objectSize);
            });
            Gizmos.color = colorChache;
        }

#endregion

        public float BezierDistanceEstimate (float startT, float endT) {
            return BezierDistanceEstimate(startT, endT, DEFAULT_LENGTH_CALC_ITERATIONS);
        }

        public float BezierDistanceEstimate (float startT, float endT, int steps) {
            float total = 0f;
            Vector3 last = BezierPoint(startT);
            for(int i=0; i<steps; i++){
                float t = Mathf.Lerp(startT, endT, (float)(i+1) / steps);
                Vector3 current = BezierPoint(t);
                total += (current - last).magnitude;
                last = current;
            }
            return total;
        }

        public float CalculateLength () {
            return CalculateLength(DEFAULT_LENGTH_CALC_ITERATIONS);
        }

        public float CalculateLength (int steps) {
            return BezierDistanceEstimate(0f, 1f, steps);
        }

        public float NextTFromEuclidianDistance (float startT, float desiredDistance) {
            return NextTFromEuclidianDistance(startT, desiredDistance, DEFAULT_NEXT_T_ITERATIONS);
        }

        public float NextTFromEuclidianDistance (float startT, float desiredDistance, int iterations) {
            return NextTFromEuclidianDistance(startT, desiredDistance, iterations, Length);
        }

        public float NextTFromEuclidianDistance (float startT, float desiredDistance, int iterations, float inputLength) {
            float desiredSqrDistance = desiredDistance * desiredDistance;
            Vector3 startPoint = BezierPoint(startT);
            return NextTFromDistance(
                getDistDelta: (testT) => (desiredSqrDistance - (BezierPoint(testT) - startPoint).sqrMagnitude), 
                startT: startT, 
                desiredDistance: desiredDistance, 
                iterations: iterations, 
                length: inputLength
            );
        }

        public float NextTFromBezierDistance (float startT, float desiredDistance) {
            return NextTFromBezierDistance(startT, desiredDistance);
        }

        public float NextTFromBezierDistance (float startT, float desiredDistance, int precision, int iterations) {
            return NextTFromBezierDistance(startT, desiredDistance, precision, iterations, Length);
        }

        public float NextTFromBezierDistance (float startT, float desiredDistance, int precision, int iterations, float inputLength) {
            float absDist = Mathf.Abs(desiredDistance);
            Vector3 startPoint = BezierPoint(startT);
            return NextTFromDistance(
                getDistDelta: (testT) => (absDist - BezierDistanceEstimate(startT, testT, precision)),
                startT: startT, 
                desiredDistance: desiredDistance, 
                iterations: iterations, 
                length: inputLength
            );
        }

        float NextTFromDistance (System.Func<float, float> getDistDelta, float startT, float desiredDistance, int iterations, float length) {
            if(desiredDistance == 0 || float.IsNaN(desiredDistance) || float.IsInfinity(desiredDistance)){
                return startT;
            }
            if(length == 0 || float.IsInfinity(length) || float.IsNaN(length)){
                return startT;
            }
            float lastDistDelta = getDistDelta(startT);
            float closestT = startT;
            float closestTAbsDistDelta = Mathf.Abs(lastDistDelta);
            float delta = desiredDistance / length;     // just a solid(ish) guess. this is where the direction comes from as desiredDistance can be positive or negative...
            float deltaMultiplier = 2f;

            for(int i=0; i<iterations; i++){            // binary search
                startT += delta;
                float currentDistDelta = getDistDelta(startT);
                if(currentDistDelta == 0){
                    closestT = startT;
                    break;
                }
                if(Mathf.Sign(currentDistDelta) != Mathf.Sign(lastDistDelta)){   // over-/undershoot
                    delta *= -1f;               // flip direction
                    deltaMultiplier = 0.5f;     // from now on we're inching closer every iteration
                }
                float absDistDelta = Mathf.Abs(currentDistDelta);
                if(absDistDelta < closestTAbsDistDelta){
                    closestT = startT;
                    closestTAbsDistDelta = absDistDelta;
                }
                delta *= deltaMultiplier;
                lastDistDelta = currentDistDelta;
            }

            return closestT;
        }
    
    }

}