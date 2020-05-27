using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class ContinuousBezierSpline : BezierSpline {

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

        public void WorldPoints (Point point, out Vector3 wPos, out Vector3 wHFwd, out Vector3 wHBwd) {
            wPos = transform.TransformPoint(point.pos);
            wHFwd = wPos + transform.TransformVector(point.handleFwd);
            wHBwd = wPos + transform.TransformVector(point.handleBwd);
        }

        public Vector3 LocalPointPos (Vector3 wPos) {
            return transform.InverseTransformPoint(wPos);
        }

        public Vector3 PointSpaceHandlePos (Point point, Vector3 wHandlePos) {
            return transform.InverseTransformVector(wHandlePos - transform.TransformPoint(point.pos));
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
            BuildSafeUndo.RecordObject(this, "Insert new point");
            Point prev = points[insertIndex];
            if((insertIndex + 1) < PointCount){
                var next = points[insertIndex+1];
                WorldPoints(prev, out var p0, out var p1, out _);
                WorldPoints(next, out var p3, out _, out var p2);
                var bPoint = CubicBezierSpline.BezierPoint(p0, p1, p2, p3, 0.5f);
                var bFwd = CubicBezierSpline.BezierDerivative(p0, p1, p2, p3, 0.5f);
                var newPoint = new Point(
                    type: Point.Type.SMOOTH, 
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
                    type: Point.Type.SMOOTH,
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
            BuildSafeUndo.RecordObject(this, "Delete point");
            points.RemoveAt(deleteIndex);
            ValidatePointsList();
        }

        public void MovePointIndex (int startIndex, int delta) {
            if(!IndexCheckAndComplain(startIndex)){
                return;
            }
            BuildSafeUndo.RecordObject(this, "Move point index");
            int endIndex = Mathf.Min(PointCount - 1, Mathf.Max(0, startIndex + delta));
            var movePoint = points[startIndex];
            points.RemoveAt(startIndex);
            points.Insert(endIndex, movePoint);
        }

        public override void ReverseDirection () {
            BuildSafeUndo.RecordObject(this, "Reverse spline direction");
            var newPoints = new List<Point>();
            int loopStart, loopEnd;
            if(cyclic){
                var startPoint = points[0];
                newPoints.Add(new Point(
                    type: startPoint.type,
                    pos: startPoint.pos,
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
                    pos: origPoint.pos,
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
                point.pos = LocalPointPos(wPoints[i].Item1);
                point.handleFwd = PointSpaceHandlePos(point, wPoints[i].Item2);
                point.handleBwd = PointSpaceHandlePos(point, wPoints[i].Item3);
            }
        }

        public override void ApplyScale () {
            if(transform.localScale.Equals(Vector3.one)){
                return;
            }
            BuildSafeUndo.RecordObject(this.transform, "Apply spline scale");
            BuildSafeUndo.RecordObject(this, "Apply spline scale");
            ChangeTransformButKeepPoints((ps) => {this.transform.localScale = Vector3.one;});
        }

        public override void MovePositionToAveragePoint () {
            BuildSafeUndo.RecordObject(this.transform, "Move position to average point");
            BuildSafeUndo.RecordObject(this, "Move position to average point");
            ChangeTransformButKeepPoints((ps) => {this.transform.position = ps / PointCount;});
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
                        break;
                    default: 
                        Debug.LogError($"Unknown {typeof(Point.Type)} \"{value}\"");
                        break;
                }
            }}

            public Vector3 pos { get => m_pos; set {
                m_pos = value;
            }}

            public Vector3 handleFwd { get => m_handleFwd; set {
                m_handleFwd = value;
                switch(type){
                    case Type.SMOOTH:
                        var bwdMag = handleBwd.magnitude;
                        m_handleBwd = -bwdMag * value.normalized;
                        break;
                    case Type.BROKEN:
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
                m_handleBwd = handleBwd;
                this.handleFwd = handleFwd;
            }

            public Point (Point other) {
                m_type = other.type;
                m_pos = other.pos;
                m_handleFwd = other.handleFwd;
                m_handleBwd = other.handleBwd;
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

        private const int DESELECTED_INDEX = -1;

        ContinuousBezierSpline cbs;
        GUIStyle textStyle;
        int selectionIndex;

        void OnEnable () {
            cbs = target as ContinuousBezierSpline;
            textStyle = BezierSpline.GetHandlesTextStyle();
            selectionIndex = DESELECTED_INDEX;
        }

        bool SelectionIndexIsValid () {
            return !(selectionIndex < 0 || selectionIndex >= cbs.PointCount);
        }

        void OnSceneGUI () {
            if(!SelectionIndexIsValid() || !(cbs.showHandles || cbs.showLabels)){
                return;
            }
            var point = cbs[selectionIndex];
            cbs.WorldPoints(point, out var wPos, out var wHFwd, out var wHBwd);
            if(cbs.showHandles){
                EditorGUI.BeginChangeCheck();
                Vector3 newWPos = Handles.PositionHandle(wPos, Quaternion.identity);
                Vector3 newWHandleFwd = Handles.PositionHandle(wHFwd, Quaternion.identity);
                Vector3 newWHandleBwd = Handles.PositionHandle(wHBwd, Quaternion.identity);
                if(EditorGUI.EndChangeCheck()){
                    Undo.RecordObject(cbs, "Change Bezier Point");
                    if(newWPos != wPos){
                        point.pos = cbs.LocalPointPos(newWPos);
                    }else if(newWHandleFwd != wHFwd){
                        point.handleFwd = cbs.PointSpaceHandlePos(point, newWHandleFwd);
                    }else if(newWHandleBwd != wHBwd){
                        point.handleBwd = cbs.PointSpaceHandlePos(point, newWHandleBwd);
                    }
                }
            }
            if(cbs.showLabels){
                Handles.Label(wPos, $"P{selectionIndex}", textStyle);
                Handles.Label(wHFwd, $"P{selectionIndex}fwd", textStyle);
                Handles.Label(wHBwd, $"P{selectionIndex}bwd", textStyle);
            }
        }

        public override void OnInspectorGUI () {
            serializedObject.Update();
            BezierSpline.GenericSplineInspector(serializedObject, MonoScript.FromMonoBehaviour(cbs), typeof(ContinuousBezierSpline));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cyclic"));
            var pointListSP = serializedObject.FindProperty("points");

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(pointListSP.displayName);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            int newSelectionIndex = selectionIndex;
            int addIndex = DESELECTED_INDEX;
            int removeIndex = DESELECTED_INDEX;
            for(int i=0; i<pointListSP.arraySize; i++){
                var bgCache = GUI.backgroundColor;
                Header();
                if(i == selectionIndex){
                    Body();
                }
                
                void Header () {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(i.ToString(), GUILayout.Width(24));
                    if(i == selectionIndex){
                        GUI.backgroundColor = (0.25f * Color.green) + (0.75f * bgCache);
                        if(GUILayout.Button("Deselect", GUILayout.Width(70))){
                            newSelectionIndex = DESELECTED_INDEX;
                        }
                        GUI.backgroundColor = bgCache;
                    }else{
                        if(GUILayout.Button("Select", GUILayout.Width(70))){
                            newSelectionIndex = i;
                        }
                    }
                    if(GUILayout.Button("<", GUILayout.Width(20))){
                        cbs.MovePointIndex(i, -1);
                        newSelectionIndex = Mathf.Clamp(newSelectionIndex - 1, 0, cbs.PointCount - 1);
                    }
                    if(GUILayout.Button(">", GUILayout.Width(20))){
                        cbs.MovePointIndex(i, 1);
                        newSelectionIndex = Mathf.Clamp(newSelectionIndex + 1, 0, cbs.PointCount - 1);
                    }
                    if(GUILayout.Button("Insert", GUILayout.Width(50))){
                        addIndex = i;
                        if(selectionIndex != DESELECTED_INDEX){
                            newSelectionIndex = addIndex + 1;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = (0.25f * Color.red) + (0.75f * bgCache);
                    if(GUILayout.Button("Delete", GUILayout.Width(50))){
                        removeIndex = i;
                        if(selectionIndex != DESELECTED_INDEX){
                            newSelectionIndex = Mathf.Max(0, removeIndex - 1);
                        }
                    }
                    GUI.backgroundColor = bgCache;
                    GUILayout.EndHorizontal();
                }

                void Body () {
                    var pointSP = pointListSP.GetArrayElementAtIndex(i);
                    var typeProp = pointSP.FindPropertyRelative("m_type");
                    var posProp = pointSP.FindPropertyRelative("m_pos");
                    var hFwdProp = pointSP.FindPropertyRelative("m_handleFwd");
                    var hBwdProp = pointSP.FindPropertyRelative("m_handleBwd");

                    InsetPropField(typeProp, () => {
                        EditorGUI.BeginChangeCheck();
                        var origType = (ContinuousBezierSpline.Point.Type)(typeProp.enumValueIndex);
                        var newType = (ContinuousBezierSpline.Point.Type)EditorGUILayout.EnumPopup(typeProp.displayName, origType);
                        if(EditorGUI.EndChangeCheck()){
                            Undo.RecordObject(cbs, "Changed point type");
                            cbs[i].type = newType;
                        }
                    });

                    InsetVector3Field(posProp, posProp.vector3Value, (newPos) => {cbs[i].pos = newPos;});
                    InsetVector3Field(hFwdProp, hFwdProp.vector3Value, (newFwd) => {cbs[i].handleFwd = newFwd;});
                    InsetVector3Field(hBwdProp, hBwdProp.vector3Value, (newBwd) => {cbs[i].handleBwd = newBwd;});

                    void InsetPropField (SerializedProperty prop, System.Action drawContent) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Empty, GUILayout.Width(20));
                        drawContent();
                        GUILayout.EndHorizontal();
                    }

                    void InsetVector3Field (SerializedProperty prop, Vector3 origValue, System.Action<Vector3> applyNewValue) {
                        InsetPropField(prop, () => {
                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUILayout.Vector3Field(prop.displayName, origValue);
                            if(EditorGUI.EndChangeCheck()){
                                Undo.RecordObject(cbs, "Changed point vector");
                                applyNewValue(newValue);
                            }
                        });
                    }
                }
            }
            selectionIndex = newSelectionIndex;
            if(addIndex != DESELECTED_INDEX){
                cbs.AddPointAfter(addIndex);
            }
            if(removeIndex != DESELECTED_INDEX){
                cbs.DeletePoint(removeIndex);
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

    #endif

}