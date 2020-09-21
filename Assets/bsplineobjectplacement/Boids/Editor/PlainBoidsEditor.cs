using UnityEngine;
using UnityEditor;

namespace Boids {

    [CustomEditor(typeof(PlainBoids))]
    public class PlainBoidsEditor : GenericEditor {

        private const string SIZE_PROP = "size";
        private const string PREFAB_PROP = "boidPrefab";
        private const string ANIM_NAME_PROP = "initialAnimationName";

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            if(EditorApplication.isPlaying){
                GUILayout.Space(10f);
                var bgColCache = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(bgColCache, Color.green, BACKGROUND_TINT_STRENGTH);
                if(EditorTools.ButtonCentered("Spawn Boids", 250f)){
                    (target as PlainBoids).SpawnBoids();
                }
                GUI.backgroundColor = bgColCache;
                GUILayout.Space(10f);
            }
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(property.name.Equals(SIZE_PROP)){
                EditorTools.DrawHorizontal(() => {
                    EditorGUILayout.PropertyField(property, true);
                    EditorTools.DrawDisabled(() => GUILayout.Label("(Transform scale also works)"));
                });
                return true;
            }
            if(property.name.Equals(PREFAB_PROP)){
                ObjectFieldRedBackgroundIfNull(property);
                return true;
            }
            if(property.name.Equals(ANIM_NAME_PROP)){
                var prefabProp = serializedObject.FindProperty(PREFAB_PROP);
                var guiOn = GUI.enabled;
                if(prefabProp.objectReferenceValue == null){
                    GUI.enabled = false;
                    property.stringValue = string.Empty;
                }else if((prefabProp.objectReferenceValue as GameObject).GetComponent<Animator>() == null){
                    GUI.enabled = false;
                    property.stringValue = string.Empty;
                }
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property);
                EditorGUI.indentLevel--;
                GUI.enabled = guiOn;
                return true;
            }
            return false;
        }

    }

}