using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(BSplineObjectPlacer))]
    public class BSplineObjectPlacerEditor : GenericEditor {

        BSplineObjectPlacer bsop;
        SerializedProperty placementMode;

        void OnEnable () {
            bsop = target as BSplineObjectPlacer;
            placementMode = serializedObject.FindProperty("placementMode");
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "spline":
                    EditorTools.DrawObjectFieldWarnIfNull(property);
                    return true;
                case "objectPool":
                    EditorTools.DrawObjectFieldWarnIfNull(property);
                    return true;
                case "groundCollider":
                    if(placementMode.enumValueIndex != (int)BSplineObjectPlacer.PlacementMode.OnSpline){
                        EditorTools.DrawObjectFieldWarnIfNull(property);
                    }
                    return true;
                default:
                    return false;
            }
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            var buttonWidth = Mathf.Max(250f, EditorGUIUtility.currentViewWidth - 100f);
            GUILayout.Space(10);
            if(EditorTools.ButtonCenteredWithTint("Clear placed objects", buttonWidth, Color.red)) bsop.DeletePlacedObjects();
            if(Button("(Re)Place")) bsop.PlaceObjects();
            if(Button("Randomize Seed")) bsop.RandomizeSeed();
            GUILayout.Space(10);
            if(Button("Rotate placed objects 90°")) bsop.Rotate90Deg();
            if(Button("Reverse direction")) bsop.ReverseDirection();

            bool Button (string text) {
                return EditorTools.ButtonCentered(text, buttonWidth);
            }
        }

    }

}