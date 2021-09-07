using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(ContinuousBezierSpline))]
    public class ContinuousBezierSplineEditor : BezierSplineEditor {

        private const int DESELECTED_INDEX = -1;

        ContinuousBezierSpline cbs;
        GUIStyle textStyle;

        static Dictionary<ContinuousBezierSpline, int> selectionIndices = new Dictionary<ContinuousBezierSpline, int>();

        int selectionIndex { 
            get {
                if(!selectionIndices.TryGetValue(cbs, out _)){
                    selectionIndices[cbs] = DESELECTED_INDEX;
                }
                return selectionIndices[cbs];
            } set {
                selectionIndices[cbs] = value;
            }
        }

        protected override void OnEnable () {
            base.OnEnable();
            cbs = target as ContinuousBezierSpline;
            textStyle = BezierSplineEditor.GetHandlesTextStyle();
            selectionIndex = DESELECTED_INDEX;
        }

        bool SelectionIndexIsValid () {
            return !(selectionIndex < 0 || selectionIndex >= cbs.PointCount);
        }

        void OnSceneGUI () {
            if(!(cbs.showHandles || cbs.showLabels)){
                return;
            }
            if(!SelectionIndexIsValid()){
                if(cbs.showLabels){
                    for(int i=0; i<cbs.PointCount; i++){
                        Handles.Label(cbs.WorldPoint(cbs[i]), $"P{i}", textStyle);
                    }
                }
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
                        point.position = cbs.LocalPointPos(newWPos);
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

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            if(property.name.Equals("points")){
                DrawPointsList(property);
                return true;
            }
            return false;
        }

        void DrawPointsList (SerializedProperty pointList) {
            GUILayout.Space(10);
            EditorTools.HeaderLabel(pointList.displayName);

            int newSelectionIndex = selectionIndex;
            int addIndex = DESELECTED_INDEX;
            int removeIndex = DESELECTED_INDEX;
            for(int i=0; i<pointList.arraySize; i++){
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
                        Undo.RecordObject(cbs, "Move point index");
                        cbs.MovePointIndex(i, -1);
                        newSelectionIndex = Mathf.Clamp(newSelectionIndex - 1, 0, cbs.PointCount - 1);
                    }
                    if(GUILayout.Button(">", GUILayout.Width(20))){
                        Undo.RecordObject(cbs, "Move point index");
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
                    var pointSP = pointList.GetArrayElementAtIndex(i);
                    var typeProp = pointSP.FindPropertyRelative("m_type");
                    var posProp = pointSP.FindPropertyRelative("m_position");
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

                    InsetVector3Field(posProp, posProp.vector3Value, (newPos) => {cbs[i].position = newPos;});
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
                Undo.RecordObject(cbs, "Insert new point");
                cbs.AddPointAfter(addIndex);
            }
            if(removeIndex != DESELECTED_INDEX){
                Undo.RecordObject(cbs, "Delete point");
                cbs.DeletePoint(removeIndex);
            }
        }

    }

}