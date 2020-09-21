﻿using UnityEngine;
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
            var bgCache = GUI.backgroundColor;
            var removeButtonColor = Color.Lerp(bgCache, Color.red, BACKGROUND_TINT_STRENGTH);
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
                    ObjectFieldRedBackgroundIfNull(wpTransformProp);
                    GUI.backgroundColor = removeButtonColor;
                    if(GUILayout.Button("X", GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT))){
                        removeIndex = i;
                    }
                    GUI.backgroundColor = bgCache;
                });
                Line(string.Empty, () => {
                    var origRadius = wp.Radius;
                    var newRadius = EditorGUILayout.Slider("Radius", origRadius, WaypointBoids.Waypoint.MIN_RADIUS, WaypointBoids.Waypoint.MAX_RADIUS);
                    wpSQRadProp.floatValue = newRadius * newRadius;
                    if(GUILayout.Button("Λ", GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT))){
                        Undo.RecordObject(boidsScript, "Move point index");
                        boidsScript.MovePointIndex(i, -1);
                    }
                    if(GUILayout.Button("V", GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT))){
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
            }

            if(removeIndex != DEFAULT_INDEX){
                Undo.RecordObject(boidsScript, "Delete point");
                boidsScript.RemoveWaypoint(removeIndex);
            }

            if(EditorTools.ButtonCentered("+", EditorGUIUtility.currentViewWidth / 2)){
                Undo.RecordObject(boidsScript, "Add point");
                boidsScript.AddWaypoint();
            }

            GUI.backgroundColor = bgCache;
        }
        
    }

}