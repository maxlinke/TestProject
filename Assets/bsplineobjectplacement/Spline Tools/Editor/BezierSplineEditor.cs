using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SplineTools {

    // public abstract class BezierSplineEditor : GenericEditor {
    public abstract class BezierSplineEditor : Editor {

        private const int GUI_HANDLES_FONT_SIZE = 12;
        private static Color GUI_HANDLES_TEXT_BACKGROUND => new Color(1, 1, 1, 0.5f);
        private static Color GUI_HANDLES_TEXT => Color.black;
        private static Texture2D GUI_HANDLES_BACKGROUND_TEX;

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

        // protected override bool DrawPropertyCustom (SerializedProperty property) {
        //     switch(property.name){

        //         default:
        //             return false;
        //     }
        // }

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
        
    }

}