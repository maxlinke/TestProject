﻿using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    public abstract class BezierSpline : MonoBehaviour {

        public const float MIN_GIZMO_SIZE = 0.1f;
        public const float MAX_GIZMO_SIZE = 10f;
        public const float DEFAULT_GIZMO_SIZE = 0.5f;

        [Header("Display")]
        [SerializeField] public bool showLabels = true;
        [SerializeField] public bool showHandles = true;
        [SerializeField] public bool showDirection = true;

        [Header("Gizmos")]
        [SerializeField] public bool alwaysDrawGizmos = true;
        [SerializeField] public bool overrideGizmoColor = false;
        [SerializeField] public Color customGizmoColor = Color.white;
        [SerializeField, Range(MIN_GIZMO_SIZE, MAX_GIZMO_SIZE)] public float gizmoSize = DEFAULT_GIZMO_SIZE;

        public abstract int DEFAULT_LENGTH_CALC_ITERATIONS { get; }
        public abstract int DEFAULT_NEXT_T_ITERATIONS { get; }
        public abstract int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION { get; }

        private float m_length;
        private Vector3 lastLossyScale;
        private Color m_gizmoColor;
        private Color m_inverseGizmoColor;

        private bool lastOverrideColor;
        private string lastName;

        private bool m_drawingGizmosSelected = false;

        protected virtual void Reset () { }

        public abstract Vector3 BezierPoint (float t);

        public abstract Vector3 BezierDerivative (float t);

        public abstract Vector3 SecondDerivative (float t);

        public abstract void ReverseDirection ();

        public abstract void ApplyScale ();

        public abstract void MovePositionToAveragePoint ();

        public float Length { get {
            if(NeedToRecalculateLength()){
                m_length = CalculateLength();
                OnLengthRecalculated();
            }
            return m_length;
        }}

        public Color GizmoColor { get {
            bool recalcInverse = false;
            if(overrideGizmoColor){
                m_gizmoColor = customGizmoColor;
                recalcInverse = true;
            }else if(gameObject.name != lastName || lastOverrideColor){
                m_gizmoColor = GetGizmoColorFromNameHash();
                recalcInverse = true;
            }
            if(recalcInverse){
                var lum = 0.299f * m_gizmoColor.r + 0.587f * m_gizmoColor.g + 0.115f * m_gizmoColor.b;
                m_inverseGizmoColor = lum < 0.5f ? Color.white : Color.black;
            }
            lastOverrideColor = overrideGizmoColor;
            lastName = gameObject.name;
            return m_gizmoColor;
        }}

        public Color InverseGizmoColor { get {
            var temp = GizmoColor;  // for the recalc
            return m_inverseGizmoColor;
        }}

        private bool NeedToRecalculateLength () {
            return (transform.localScale != lastLossyScale) || PointsChangedSinceLastRecalculation();
        }

        protected abstract bool PointsChangedSinceLastRecalculation ();

        protected virtual void OnLengthRecalculated () {
            lastLossyScale = transform.lossyScale;
        }

#region Gizmos

        protected abstract IEnumerable<(Vector3 pos, float t)> GetWorldSpaceEndPoints ();

        protected abstract IEnumerable<(Vector3 pos, float t)> GetWorldSpaceControlPoints ();

        protected abstract IEnumerable<(Vector3 start, Vector3 end)> GetWorldSpaceHandleLines ();

        public Color GetGizmoColor () {
            if(overrideGizmoColor){
                return customGizmoColor;
            }
            return GetGizmoColorFromNameHash();
        }

        public Color GetGizmoColorFromNameHash () {
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

        Color MovingThingyGizmoColor (float inputTime, float inputT) {
            if(!showDirection){
                return GizmoColor;
            }
            float delta = Mathf.Abs(inputT - inputTime);
            float lrp = Mathf.Clamp01(Mathf.Min(delta, 1f - delta) / 0.1f);
            return Color.Lerp(InverseGizmoColor, GizmoColor, lrp);
        }

        float MovingThingyInputTime () {
            return ((float)(System.DateTime.Now.Millisecond)) / 1000f;
        }

        void GizmoLine (float stepSize, System.Action<Vector3, float> onStep) {
            float l = Length;
            if(l > 0){
                float t = 0f;
                onStep(BezierPoint(t), t);
                int i = 0;                      // crash prevention.
                while(t < 1f){
                    if(i > 9000){
                        Debug.LogWarning("Spline draw loop limit reached, what's going on?");
                        break;
                    }
                    t = Mathf.Clamp01(NextTFromEuclidianDistance(t, stepSize, 10, l));
                    onStep(BezierPoint(t), t);
                    i++;
                }
            }
        }

        void OnDrawGizmos () {
            gizmoSize = Mathf.Clamp(gizmoSize, MIN_GIZMO_SIZE, MAX_GIZMO_SIZE);
            if(!alwaysDrawGizmos){
                return;
            }
            var highlightT = MovingThingyInputTime();
            var colorChache = Gizmos.color;
            Gizmos.color = GizmoColor;
            foreach(var (p, t) in GetWorldSpaceEndPoints()){
                if(m_drawingGizmosSelected){
                    Gizmos.color = MovingThingyGizmoColor(highlightT, t);
                }
                Gizmos.DrawSphere(p, gizmoSize);
            }
            Vector3 lastPoint = BezierPoint(0);
            GizmoLine(gizmoSize, (newPoint, newT) => {
                if(m_drawingGizmosSelected){
                    Gizmos.color = MovingThingyGizmoColor(highlightT, newT);
                }
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            });
            Gizmos.color = colorChache;
        }

        void OnDrawGizmosSelected () {
            #if UNITY_EDITOR
            if(UnityEditor.Selection.activeGameObject != this.gameObject){
                return;
            }
            #endif
            var gizmoCache = alwaysDrawGizmos;
            alwaysDrawGizmos = true;
            m_drawingGizmosSelected = true;
            OnDrawGizmos();
            alwaysDrawGizmos = gizmoCache;
            m_drawingGizmosSelected = false;

            float highlightT = MovingThingyInputTime();
            var colorChache = Gizmos.color;
            var gizCol = GizmoColor;
            Gizmos.color = gizCol;
            foreach(var (p, t) in GetWorldSpaceControlPoints()){
                Gizmos.DrawSphere(p, 0.5f * gizmoSize);
            }
            foreach(var (start, end) in GetWorldSpaceHandleLines()){
                Gizmos.DrawLine(start, end);
            }
            float stepSize = 2f * gizmoSize;
            float objectSize = 0.25f * gizmoSize;
            GizmoLine(stepSize, (newPoint, newT) => {
                Gizmos.color = MovingThingyGizmoColor(highlightT, newT);
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
            return NextTFromBezierDistance(startT, desiredDistance, DEFAULT_NEXT_T_BEZIER_DIST_PRECISION, DEFAULT_NEXT_T_ITERATIONS);
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