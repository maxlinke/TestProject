using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(BSplineMaster))]
    public class BSplineMasterEditor : GenericEditor {

        BSplineMaster bsm;

        protected override void OnEnable () {
            base.OnEnable();
            bsm = target as BSplineMaster;
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            var buttonWidth = Mathf.Max(250f, EditorGUIUtility.currentViewWidth - 100f);
            GUILayout.Space(10);
            if(Button("Toggle Gizmos"))                 bsm.ToggleGizmos();
            if(Button("Update Gizmo Settings"))         bsm.UpdateGizmoSettings();
            if(Button("Update Randomization Settings")) bsm.UpdateRandomizationSettings();
            if(Button("Update Object Settings"))        bsm.UpdateObjectSettings();
            if(Button("Update Placement Settings"))     bsm.UpdatePlacementSettings();
            GUILayout.Space(10);
            if(Button("(Re)Place All")) bsm.ReplaceAll();
            if(Button("Randomize All")) bsm.RandomizeSeeds();
            if(EditorTools.ButtonCenteredWithTint("Clear All", buttonWidth, Color.red)) bsm.ClearAll();

            bool Button (string text) {
                return EditorTools.ButtonCentered(text, buttonWidth);
            }
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "objectPool":
                    EditorTools.DrawObjectFieldWarnIfNull(property);
                    return true;
                case "groundCollider":
                    EditorTools.DrawObjectFieldWarnIfNull(property);
                    return true;
                default:
                    return false;
            }
        }

    }

}