using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(MeshPool))]
    public class MeshPoolEditor : Editor {

        MeshPool mp;

        void OnEnable () {
            mp = target as MeshPool;
        }

        public override void OnInspectorGUI () {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(mp), typeof(MeshPool), false);
            GUI.enabled = true;

            var otProp = serializedObject.FindProperty("placementOrder");
            EditorGUILayout.PropertyField(otProp);
            if(((MeshPool.PlacementOrder)(otProp.enumValueIndex)) == MeshPool.PlacementOrder.RandomOrder){
                var rrtProp = serializedObject.FindProperty("randomRepetition");
                EditorGUILayout.PropertyField(rrtProp);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Objects");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            var listProp = serializedObject.FindProperty("objects");
            int deleteIndex = -1;

            for(int i=0; i<mp.ObjectCount; i++){
                var poProp = listProp.GetArrayElementAtIndex(i);

                var prefabProp = poProp.FindPropertyRelative("prefab");
                InsetLine(1, () => {
                    EditorGUILayout.PropertyField(prefabProp);
                    var bgCache = GUI.backgroundColor;
                    GUI.backgroundColor = (0.25f * Color.red) + (0.75f * bgCache);
                    if(GUILayout.Button("X", GUILayout.Width(20))){
                        deleteIndex = i;
                    }
                    GUI.backgroundColor = bgCache;
                }, i.ToString());

                var occProp = poProp.FindPropertyRelative("occurences");
                InsetLine(1, () => {
                    var occVal = occProp.intValue;
                    if(occVal < 1){
                        occProp.intValue = 1;
                    }
                    EditorGUILayout.PropertyField(occProp);
                });

                var bcProp = poProp.FindPropertyRelative("useBoxColliderForSpacing");
                if(!MeshPoolObject.CanUseBoxCollForSpacing(prefabProp.objectReferenceValue as GameObject, out _, out var boxColMsg)){
                    GUI.enabled = false;
                    bcProp.boolValue = false;
                }
                InsetLine(1, () => {
                    EditorGUILayout.PropertyField(bcProp);
                    GUILayout.Label(boxColMsg);
                });
                GUI.enabled = true;

                var cbProp = poProp.FindPropertyRelative("useCustomBounds");
                if(bcProp.boolValue){
                    GUI.enabled = false;
                }
                InsetLine(1, () => EditorGUILayout.PropertyField(cbProp));
                if(cbProp.boolValue){
                    InsetLine(2, () => EditorGUILayout.PropertyField(poProp.FindPropertyRelative("customBoundsCenter")));
                    InsetLine(2, () => EditorGUILayout.PropertyField(poProp.FindPropertyRelative("customBoundsSize")));
                }
                GUI.enabled = true;

                var matProp = poProp.FindPropertyRelative("materials");
                InsetLine(1, () => EditorGUILayout.PropertyField(matProp, true));

                Separator();

                void InsetLine (int insetLevel, System.Action drawLine, string label = "") {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label, GUILayout.Width(insetLevel * 20));
                    drawLine();
                    GUILayout.EndHorizontal();
                }
            }

            void Separator () {
                var guiCache = GUI.enabled;
                GUI.enabled = false;
                GUILayout.Box(string.Empty, GUILayout.Height(2), GUILayout.Width(EditorGUIUtility.currentViewWidth-30));
                GUI.enabled = guiCache;
            }

            if(deleteIndex != -1){
                Undo.RecordObject(mp, "Delete array element");
                mp.DeleteObject(deleteIndex);
            }

            if(GUILayout.Button("+")){
                Undo.RecordObject(mp, "Add array element");
                mp.AddObject();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

}