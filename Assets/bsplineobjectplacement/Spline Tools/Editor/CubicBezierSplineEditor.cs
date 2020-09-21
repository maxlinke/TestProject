﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(CubicBezierSpline))]
    public class CubicBezierSplineEditor : Editor {

        CubicBezierSpline cbs;
        GUIStyle textStyle;

        void OnEnable () {
            cbs = target as CubicBezierSpline;
            textStyle = BezierSplineEditor.GetHandlesTextStyle();
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

}