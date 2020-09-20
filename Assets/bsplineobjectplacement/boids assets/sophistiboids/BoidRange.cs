using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SophistiBoids {

    [System.Serializable]
    public struct BoidRange {

        public static BoidRange Default => new BoidRange(InfluenceMode.DISTANCE, AttractionMode.ATTRACT, 0, 1, 1);

        [SerializeField] InfluenceMode m_influenceMode;
        [SerializeField] AttractionMode m_attractionMode;
        [SerializeField] float m_a;
        [SerializeField] float m_b;
        [SerializeField] float m_pow;

        public InfluenceMode influenceMode => m_influenceMode;
        public AttractionMode attractionMode => m_attractionMode;
        public float min => Mathf.Min(m_a, m_b);
        public float max => Mathf.Max(m_a, m_b);
        public float pow => m_pow;
        public float sign { get {
            switch(attractionMode){
                case AttractionMode.ATTRACT:
                    return 1f;
                case AttractionMode.REPULSE:
                    return -1f;
                default:
                    Debug.LogError($"Unknown {nameof(AttractionMode)} \"{attractionMode}\"!");
                    return float.NaN;
            }
        }}

        public BoidRange (InfluenceMode iMode, AttractionMode aMode, float a, float b, float pow) {
            this.m_influenceMode = iMode;
            this.m_attractionMode = aMode;
            this.m_a = a;
            this.m_b = b;
            this.m_pow = pow;
        }

        public float Evaluate (float input) {
            var x = (input - min) / (max - min);
            switch(influenceMode){
                case InfluenceMode.DISTANCE:
                    x = Mathf.Clamp01(x);
                    break;
                case InfluenceMode.INVERTED_DISTANCE:
                    x = Mathf.Clamp01(1f - x);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(InfluenceMode)} \"{influenceMode}\"!");
                    break;
            }
            return Mathf.Pow(x, pow);
        }

        public enum InfluenceMode {
            DISTANCE,
            INVERTED_DISTANCE
        }

        public enum AttractionMode {
            ATTRACT,
            REPULSE
        }
        
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BoidRange))]
    public class BoidRangePropertyDrawer : PropertyDrawer {

        const float margin = 5f;
        const float preferredLabelFieldWidth = 40f;

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 2f * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            var iModeProp = property.FindPropertyRelative("m_influenceMode");
            var aModeProp = property.FindPropertyRelative("m_attractionMode");
            var powProp = property.FindPropertyRelative("m_pow");
            var aProp = property.FindPropertyRelative("m_a");
            var bProp = property.FindPropertyRelative("m_b");
            SerializedProperty minProp, maxProp;
            if(aProp.floatValue <= bProp.floatValue){
                minProp = aProp;
                maxProp = bProp;
            }else{
                minProp = bProp;
                maxProp = aProp;
            }
            var firstLineLabels = new List<string>(){
                "Infl.", "Attr."
            };
            var firstLineProps = new List<System.Action<Rect>>(){(rect) => {
                var origInflMode = (BoidRange.InfluenceMode)(iModeProp.enumValueIndex);
                iModeProp.enumValueIndex = (int)(BoidRange.InfluenceMode)(EditorGUI.EnumPopup(rect, origInflMode));
            }, (rect) => {
                var origAttrMode = (BoidRange.AttractionMode)(aModeProp.enumValueIndex);
                aModeProp.enumValueIndex = (int)(BoidRange.AttractionMode)(EditorGUI.EnumPopup(rect, origAttrMode));
            }};
            var secondLineLabels = new List<string>(){
                "Min", "Max", "Pow"
            };
            var secondLineProps = new List<System.Action<Rect>>(){(rect) => {
                var origMin = minProp.floatValue;
                minProp.floatValue = EditorGUI.FloatField(rect, origMin);
            }, (rect) => {
                var origMax = maxProp.floatValue;
                maxProp.floatValue = EditorGUI.FloatField(rect, origMax);
            }, (rect) => {
                var origPow = powProp.floatValue;
                powProp.floatValue = EditorGUI.FloatField(rect, origPow);
            }};
            PropLine(position.y, firstLineLabels, firstLineProps);
            PropLine(position.y + EditorGUIUtility.singleLineHeight, secondLineLabels, secondLineProps);

            EditorGUI.EndProperty();

            void PropLine (float rectY, List<string> labels, List<System.Action<Rect>> propDrawers) {
                if(labels.Count != propDrawers.Count){
                    Debug.LogError("Unequal input list lengths! Fix it!");
                }
                var p = labels.Count;
                var remainingWidth = position.width;
                var remainingMinusMargins = remainingWidth - ((2 * p - 1) * margin);
                var remainingFractioned = remainingMinusMargins / (2 * p);
                var labelFieldWidth = (remainingFractioned > preferredLabelFieldWidth) ? preferredLabelFieldWidth : remainingFractioned;
                var propFieldWidth = 2f * remainingFractioned - labelFieldWidth;

                var rect = new Rect(position.x, rectY, labelFieldWidth, EditorGUIUtility.singleLineHeight);
                for(int i=0; i<p; i++){
                    EditorGUI.LabelField(rect, labels[i]);
                    NextRect(propFieldWidth);
                    propDrawers[i](rect);
                    NextRect(labelFieldWidth);

                    void NextRect (float newWidth) {
                        rect.x += rect.width + margin;
                        rect.width = newWidth;
                    }
                }
            }
        }

    }
    #endif

}