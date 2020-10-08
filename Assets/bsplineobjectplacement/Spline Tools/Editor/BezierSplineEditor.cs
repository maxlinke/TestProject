using UnityEngine;
using UnityEditor;

namespace SplineTools {

    public abstract class BezierSplineEditor : GenericEditor {

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

        private BezierSpline spline;
        private SerializedProperty overrideColorProp;
        private SerializedProperty customColorProp;

        protected virtual void OnEnable () {
            spline = target as BezierSpline;
            overrideColorProp = serializedObject.FindProperty("overrideGizmoColor");
            customColorProp = serializedObject.FindProperty("customGizmoColor");
        }

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            GUILayout.Space(10f);
            DrawButtons();
        }

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            switch(property.name){
                case "overrideGizmoColor":
                    EditorTools.DrawHorizontal(() => {
                        EditorGUILayout.PropertyField(overrideColorProp);
                        if(overrideColorProp.boolValue){
                            EditorGUILayout.PropertyField(customColorProp, GUIContent.none);
                        }
                    });
                    return true;
                case "customGizmoColor":
                    return true;
                default:
                    return false;
            }
        }

        void DrawButtons () {
            var buttonWidth = Mathf.Max(250f, EditorGUIUtility.currentViewWidth - 100f);
            if(Button("Reverse Direction")){
                Undo.RecordObject(spline, "Reverse spline direction");
                spline.ReverseDirection();
            }
            if(Button("Apply Transform Scale")){
                Undo.RecordObject(spline.transform, "Apply spline scale");
                Undo.RecordObject(spline, "Apply spline scale");
                spline.ApplyScale();
            }
            if(Button("Move Origin To Center")){
                Undo.RecordObject(spline.transform, "Move position to average point");
                Undo.RecordObject(spline, "Move position to average point");
                spline.MovePositionToAveragePoint();
            }

            bool Button (string text) {
                return EditorTools.ButtonCentered(text, buttonWidth);
            }
        }
        
    }

}