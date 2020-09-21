using UnityEngine;
using UnityEditor;

namespace Boids {

    [CustomEditor(typeof(WaypointBoids))]
    public class WaypointBoidsEditor : PlainBoidsEditor {

        const int DEFAULT_INDEX = -1;
        float INLINE_BUTTON_WIDTH => 20f;
        float INLINE_BUTTON_HEIGHT => EditorGUIUtility.singleLineHeight - 2;

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            if(property.name.Equals("waypoints")){
                DrawWaypointsProperty(property);
                return true;
            }
            return false;
        }

        void DrawWaypointsProperty (SerializedProperty wpArrayProp) {
            GUILayout.Space(10f);
            EditorTools.HeaderLabel(wpArrayProp.displayName);
            var boidsScript = target as WaypointBoids;

            if(wpArrayProp.arraySize < 1){
                EditorTools.DrawCentered(() => GUILayout.Label("No waypoints assigned!"));
            }

            int removeIndex = DEFAULT_INDEX;
            for(int i=0; i<wpArrayProp.arraySize; i++){
                var wpProp = wpArrayProp.GetArrayElementAtIndex(i);
                var wpTransformProp = wpProp.FindPropertyRelative("point");
                var wpSQRadProp = wpProp.FindPropertyRelative("sqRadius");
                var wp = boidsScript[i];
                Line(i.ToString(), () => {
                    EditorTools.DrawObjectFieldWarnIfNull(wpTransformProp);
                    if(InlineButton("X", true, Color.red)){
                        removeIndex = i;
                    }
                });
                Line(string.Empty, () => {
                    var origRadius = wp.Radius;
                    var newRadius = EditorGUILayout.Slider("Radius", origRadius, WaypointBoids.Waypoint.MIN_RADIUS, WaypointBoids.Waypoint.MAX_RADIUS);
                    wpSQRadProp.floatValue = newRadius * newRadius;
                    if(InlineButton("Λ")){
                        Undo.RecordObject(boidsScript, "Move point index");
                        boidsScript.MovePointIndex(i, -1);
                    }
                    if(InlineButton("V")){
                        Undo.RecordObject(boidsScript, "Move point index");
                        boidsScript.MovePointIndex(i, +1);
                    }
                });

                void Line (string labelText, System.Action drawLine) {
                    EditorTools.DrawHorizontal(() => {
                        EditorGUILayout.LabelField(labelText, GUILayout.Width(24));
                        drawLine();
                    });
                }

                bool InlineButton (string text, bool tint = false, Color tintColor = default) {
                    var bgCol = GUI.backgroundColor;
                    if(tint){
                        GUI.backgroundColor = Color.Lerp(bgCol, tintColor, EditorTools.BACKGROUND_TINT_STRENGTH);
                    }
                    var output = GUILayout.Button(text, GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT));
                    GUI.backgroundColor = bgCol;
                    return output;
                }
            }

            if(removeIndex != DEFAULT_INDEX){
                Undo.RecordObject(boidsScript, "Delete point");
                boidsScript.RemoveWaypoint(removeIndex);
            }

            if(EditorTools.ButtonCentered("+", EditorGUIUtility.currentViewWidth / 2)){
                Undo.RecordObject(boidsScript, "Add point");
                boidsScript.AddWaypoint();
            }
        }
        
    }

}