using UnityEngine;
using UnityEditor;

namespace Boids {

    public abstract class BehaviourParametersEditor : PropertyDrawer {

        float lineHeight => EditorGUIUtility.singleLineHeight;
        float verticalSpace => EditorGUIUtility.standardVerticalSpacing;

        const float propLineInset = 10f;
        const float horizontalMargin = 10f;
        const float propLabelWidth = 60f;

        const float leftPropWidthFactor = 0.45f;
        const float rightPropWidthFactor = 1f - leftPropWidthFactor;

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
            return 2f * lineHeight + verticalSpace;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
            Rect labelRect, leftLabelRect, leftPropRect, rightLabelRect, rightPropRect;
            SetupRects();
            EditorGUI.LabelField(labelRect, label);
            var noLabel = GUIContent.none;
            EditorGUI.LabelField(leftLabelRect, SpecialPropertyDisplayName);
            var leftProp = Find(SpecialPropertyName);
            var bgCache = GUI.backgroundColor;
            if(leftProp.propertyType == SerializedPropertyType.ObjectReference){
                if(leftProp.objectReferenceValue == null){
                    GUI.backgroundColor = Color.Lerp(bgCache, Color.red, EditorTools.BACKGROUND_TINT_STRENGTH);
                }                
            }
            EditorGUI.PropertyField(leftPropRect, leftProp, noLabel);
            GUI.backgroundColor = bgCache;
            EditorGUI.LabelField(rightLabelRect, "Weight");
            EditorGUI.PropertyField(rightPropRect, Find("m_weight"), noLabel);

            SerializedProperty Find (string propName) {
                return property.FindPropertyRelative(propName);
            }

            void SetupRects () {
                var x = position.x;
                var y = position.y;
                var width = position.width;
                var h = lineHeight;
                labelRect = Current(width);
                y += lineHeight + verticalSpace;
                x += propLineInset;
                width -= propLineInset;
                MakeHalfRects(leftPropWidthFactor, out leftLabelRect, out leftPropRect);
                x = leftPropRect.x + leftPropRect.width + horizontalMargin;
                MakeHalfRects(rightPropWidthFactor, out rightLabelRect, out rightPropRect);

                Rect Current (float currentWidth) {
                    return new Rect(x, y, currentWidth, h);
                }

                void MakeHalfRects (float totalWidthFactor, out Rect outputLabelRect, out Rect outputPropRect) {
                    var combinedWidth = (width * totalWidthFactor) - (horizontalMargin / 2f);
                    outputLabelRect = Current(propLabelWidth);
                    x += propLabelWidth;
                    outputPropRect = Current(combinedWidth - propLabelWidth);
                }
            }
        }

        protected abstract string SpecialPropertyName { get; }
        protected abstract string SpecialPropertyDisplayName { get; }
        
    }

    [CustomPropertyDrawer(typeof(DistancedBehaviourParameters))]
    public class DistancedBehaviourParametersEditor : BehaviourParametersEditor { 

        protected override string SpecialPropertyName => "m_distance";
        protected override string SpecialPropertyDisplayName => "Distance";

    }

    [CustomPropertyDrawer(typeof(TimedBehaviourParameters))]
    public class TimedBehaviourParametersEditor : BehaviourParametersEditor { 

        protected override string SpecialPropertyName => "m_interval";
        protected override string SpecialPropertyDisplayName => "Interval";

    }

    [CustomPropertyDrawer(typeof(TargetBehaviourParameters))]
    public class TargetBehaviourParametersEditor : BehaviourParametersEditor { 

        protected override string SpecialPropertyName => "m_target";
        protected override string SpecialPropertyDisplayName => "Target";

    }

    [CustomPropertyDrawer(typeof(WaypointBehaviourParameters))]
    public class WaypointBehaviourParametersEditor : BehaviourParametersEditor { 

        protected override string SpecialPropertyName => "m_mode";
        protected override string SpecialPropertyDisplayName => "Mode";

    }

}