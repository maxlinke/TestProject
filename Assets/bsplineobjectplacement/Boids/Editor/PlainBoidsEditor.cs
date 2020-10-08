using UnityEngine;
using UnityEditor;

namespace Boids {

    [CustomEditor(typeof(PlainBoids))]
    public class PlainBoidsEditor : GenericEditor {

        SerializedProperty prefabProp;

        protected virtual void OnEnable () {
            prefabProp = serializedObject.FindProperty("boidPrefab");
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            if(EditorApplication.isPlaying){
                GUILayout.Space(10f);
                var bgColCache = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(bgColCache, Color.green, EditorTools.BACKGROUND_TINT_STRENGTH);
                if(EditorTools.ButtonCentered("Spawn Boids", 250f)){
                    (target as PlainBoids).SpawnBoids();
                }
                GUI.backgroundColor = bgColCache;
                GUILayout.Space(10f);
            }
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "size":
                    EditorTools.DrawHorizontal(() => {
                        EditorGUILayout.PropertyField(property, true);
                        EditorTools.DrawDisabled(() => GUILayout.Label("(Transform scale works too)"));
                    });
                    return true;
                case "boidPrefab":
                    EditorTools.DrawObjectFieldWarnIfNull(property);
                    return true;
                case "initialAnimationName":
                    var guiOn = GUI.enabled;
                    if(prefabProp.objectReferenceValue == null){
                        GUI.enabled = false;
                        property.stringValue = string.Empty;
                    }else if((prefabProp.objectReferenceValue as GameObject).GetComponent<Animator>() == null){
                        GUI.enabled = false;
                        property.stringValue = string.Empty;
                    }
                    EditorTools.DrawIndented(property);
                    GUI.enabled = guiOn;
                    return true;
                default:
                    return false;
            }

        }

    }

}