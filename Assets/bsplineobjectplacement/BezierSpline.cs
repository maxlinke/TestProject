using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public abstract class BezierSpline : MonoBehaviour {

        public const float MIN_GIZMO_SIZE = 0.1f;
        public const float MAX_GIZMO_SIZE = 10f;

        private const int GUI_HANDLES_FONT_SIZE = 12;
        private static Color GUI_HANDLES_TEXT_BACKGROUND => new Color(1, 1, 1, 0.5f);
        private static Color GUI_HANDLES_TEXT => Color.black;
        private static Texture2D GUI_HANDLES_BACKGROUND_TEX;

        [SerializeField] public bool showLabels = true;
        [SerializeField] public bool showHandles = true;
        [SerializeField] public bool showDirection = true;
        [SerializeField] public bool alwaysDrawGizmos = true;
        [SerializeField, Range(MIN_GIZMO_SIZE, MAX_GIZMO_SIZE)] public float gizmoSize;

        public abstract int DEFAULT_LENGTH_CALC_ITERATIONS { get; }
        public abstract int DEFAULT_NEXT_T_ITERATIONS { get; }
        public abstract int DEFAULT_NEXT_T_BEZIER_DIST_PRECISION { get; }

        private float m_length;
        private Vector3 lastLossyScale;
        private Color m_gizmoColor;
        private Color m_inverseGizmoColor;
        private string lastName;

        private bool m_drawingGizmosSelected = false;

        protected virtual void Reset () {
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

        public Color GizmoColor { get {
            if(gameObject.name != lastName){
                m_gizmoColor = GetGizmoColor();
                Color.RGBToHSV(m_gizmoColor, out var h, out var s, out var v);
                m_inverseGizmoColor = Color.HSVToRGB(
                    H: Mathf.Repeat(h + 0.5f, 1), 
                    S: 0, 
                    V: Mathf.Repeat(v + 0.5f, 1)
                );
                lastName = gameObject.name;
            }
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
            float lrp = 1f - Mathf.Clamp01(Mathf.Abs((inputT - inputTime) / 0.2f));
            lrp *= (1f - inputTime);
            return Color.Lerp(GizmoColor, InverseGizmoColor, lrp);
        }

        float MovingThingyInputTime () {
            return ((float)(System.DateTime.Now.Millisecond)) / 1000f;
        }

        public static GUIStyle GetHandlesTextStyle () {
            var output = new GUIStyle();
            output.fontSize = GUI_HANDLES_FONT_SIZE;
            if(GUI_HANDLES_BACKGROUND_TEX == null){ 
                var bgTex = new Texture2D(4, 4);
                var cols = new Color[bgTex.width * bgTex.height];
                for(int j=0; j<bgTex.height; j++){
                    for(int i=0; i<bgTex.width; i++){
                        int pos = j * bgTex.height + i;
                        cols[pos] = GUI_HANDLES_TEXT_BACKGROUND;
                        if(i == 0 || j == 0 || i == bgTex.width-1 || j == bgTex.height-1){
                            cols[pos].a = 0;
                        }
                    }
                }
                bgTex.SetPixels(cols);
                bgTex.Apply();
                GUI_HANDLES_BACKGROUND_TEX = bgTex;
            }
            output.normal.background = GUI_HANDLES_BACKGROUND_TEX;
            output.normal.textColor = GUI_HANDLES_TEXT;
            var off = new RectOffset();
            off.bottom = off.left = off.right = off.top = 5;
            output.padding = off;
            output.alignment = TextAnchor.MiddleCenter;
            return output;
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
            if(UnityEditor.Selection.activeGameObject != this.gameObject){
                return;
            }
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

        #if UNITY_EDITOR

        public static void GenericSplineInspector (SerializedObject serializedObject, MonoScript callingScript, System.Type callingType) {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", callingScript, callingType, false);
            GUI.enabled = true;

            // GUILayout.BeginHorizontal();
            // GUILayout.Label("Labels:", GUILayout.Width(44));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("showLabels"), GUIContent.none, GUILayout.Width(10));
            // GUILayout.Space(20);
            // GUILayout.Label("Handles:", GUILayout.Width(52));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("showHandles"), GUIContent.none, GUILayout.Width(10));
            // GUILayout.Space(20);
            // GUILayout.Label("Direction:", GUILayout.Width(58));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("showDirection"), GUIContent.none, GUILayout.Width(10));
            // GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("showLabels"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showHandles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDirection"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("alwaysDrawGizmos"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoSize"));
        }

        #endif
    
    }

}