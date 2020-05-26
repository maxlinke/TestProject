using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boids {

    [System.Serializable]
    public struct BoidRange {

        public static BoidRange Default => new BoidRange(BoidRangeInfluenceMode.POSITIVE, 0, 1, 1);

        [SerializeField] BoidRangeInfluenceMode m_mode;
        [SerializeField] float m_a;
        [SerializeField] float m_b;
        [SerializeField] float m_pow;

        public BoidRangeInfluenceMode mode => m_mode;
        public float min => Mathf.Min(m_a, m_b);
        public float max => Mathf.Max(m_a, m_b);
        public float pow => m_pow;

        public BoidRange (BoidRangeInfluenceMode mode, float a, float b, float pow) {
            this.m_mode = mode;
            this.m_a = a;
            this.m_b = b;
            this.m_pow = pow;
        }

        public float Evaluate (float input) {
            var x = (input - min) / (max - min);
            switch(mode){
                case BoidRangeInfluenceMode.POSITIVE:
                    x = Mathf.Clamp01(x);
                    break;
                case BoidRangeInfluenceMode.NEGATIVE:
                    x = Mathf.Clamp01(1f - x);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(BoidRangeInfluenceMode)} \"{mode}\"!");
                    break;
            }
            return Mathf.Pow(x, pow);
        }
        
    }

    public enum BoidRangeInfluenceMode {
        POSITIVE,
        NEGATIVE
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BoidRange))]
    public class BoidRangePropertyDrawer : PropertyDrawer {

        const float margin = 5f;
        const float preferredLabelFieldWidth = 50f;

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 2f * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            var remainingWidth = position.width;
            var remainingMinusMargins = remainingWidth - (3f * margin);
            var remainingQuartered = remainingMinusMargins / 4f;
            var labelFieldWidth = (remainingQuartered > preferredLabelFieldWidth) ? preferredLabelFieldWidth : remainingQuartered;
            var propFieldWidth = 2f * remainingQuartered - labelFieldWidth;

            var modeProp = property.FindPropertyRelative("m_mode");
            var powProp = property.FindPropertyRelative("m_pow");

            PropLine(
                rectY: position.y,
                firstLabel: "Mode", 
                firstProp: (rect) => {
                    var origModeIndex = modeProp.enumValueIndex;
                    modeProp.enumValueIndex = (int)(BoidRangeInfluenceMode)(EditorGUI.EnumPopup(rect, (BoidRangeInfluenceMode)origModeIndex));
                },
                secondLabel: "Power",
                secondProp: (rect) => {
                    var origPow = powProp.floatValue;
                    powProp.floatValue = EditorGUI.FloatField(rect, origPow);
                }
            );
            
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

            PropLine(
                rectY: position.y + EditorGUIUtility.singleLineHeight,
                firstLabel: "Min", 
                firstProp: (rect) => {
                    var origMin = minProp.floatValue;
                    minProp.floatValue = EditorGUI.FloatField(rect, origMin);
                },
                secondLabel: "Max",
                secondProp: (rect) => {
                    var origMax = maxProp.floatValue;
                    maxProp.floatValue = EditorGUI.FloatField(rect, origMax);
                }
            );
            
            EditorGUI.EndProperty();

            void PropLine (float rectY, string firstLabel, System.Action<Rect> firstProp, string secondLabel, System.Action<Rect> secondProp) {
                var rect = new Rect(position.x, rectY, labelFieldWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, firstLabel);
                NextRect(propFieldWidth);
                firstProp(rect);
                NextRect(labelFieldWidth);
                EditorGUI.LabelField(rect, secondLabel);
                NextRect(propFieldWidth);
                secondProp(rect);

                void NextRect (float newWidth) {
                    rect.x += rect.width + margin;
                    rect.width = newWidth;
                }
            }
        }

    }
    #endif

}