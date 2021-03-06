﻿using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(QuadraticBezierSpline))]
    public class QuadraticBezierSplineEditor : BezierSplineEditor {

        QuadraticBezierSpline qbs;
        GUIStyle textStyle;

        protected override void OnEnable () {
            base.OnEnable();
            qbs = target as QuadraticBezierSpline;
            textStyle = BezierSplineEditor.GetHandlesTextStyle();
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            return base.DrawPropertyCustom(property);
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
                Vector3 newp0 = Handles.PositionHandle(p0, Quaternion.identity);
                Vector3 newp1 = Handles.PositionHandle(p1, Quaternion.identity);
                Vector3 newp2 = Handles.PositionHandle(p2, Quaternion.identity);
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

}
