using UnityEngine;
using UnityEditor;

namespace Boids {

    [CustomEditor(typeof(PlainBoids))]
    public class PlainBoidsEditor : Editor {

        public const float COLOR_STRENGTH = 0.3f;

        private const string SIZE_PROP = "size";
        private const string PREFAB_PROP = "boidPrefab";
        private const string ANIM_NAME_PROP = "initialAnimationName";

        public override void OnInspectorGUI () {
            EditorTools.DrawScriptReference(target);
            serializedObject.Update();
            var it = serializedObject.GetIterator();
            it.NextVisible(true);           // must be done, otherwise unity complains
            while(it.NextVisible(false)){   // skips the first element, which is the script field, which we're already drawing
                if(IsSpecialProperty(it)){
                    DrawSpecialProperty(it);
                }else{
                    EditorGUILayout.PropertyField(it, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
            if(EditorApplication.isPlaying){
                GUILayout.Space(10f);
                var bgColCache = GUI.backgroundColor;
                GUI.backgroundColor = COLOR_STRENGTH * Color.green + (1f - COLOR_STRENGTH) * bgColCache;
                if(EditorTools.ButtonCentered("Spawn Boids", 250f)){
                    (target as PlainBoids).SpawnBoids();
                }
                GUI.backgroundColor = bgColCache;
                GUILayout.Space(10f);
            }
        }

        protected virtual bool IsSpecialProperty (SerializedProperty property) {
            return property.name.Equals(SIZE_PROP) || property.name.Equals(PREFAB_PROP) || property.name.Equals(ANIM_NAME_PROP);
        }

        protected virtual void DrawSpecialProperty (SerializedProperty property) {
            if(property.name.Equals(SIZE_PROP)){
                EditorTools.DrawHorizontal(() => {
                    EditorGUILayout.PropertyField(property, true);
                    EditorTools.DrawDisabled(() => GUILayout.Label("(Transform scale also works)"));
                });
            }
            if(property.name.Equals(PREFAB_PROP)){
                ObjectFieldRedBackgroundIfNull(property);
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
            }
        }

        protected virtual void ObjectFieldRedBackgroundIfNull (SerializedProperty property) {
            var bgCol = GUI.backgroundColor;
            if(property.objectReferenceValue == null){
                GUI.backgroundColor = COLOR_STRENGTH * Color.red + (1f - COLOR_STRENGTH) * bgCol;
            }
            EditorGUILayout.PropertyField(property, true);
            GUI.backgroundColor = bgCol;
        }

    }

}