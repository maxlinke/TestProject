using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SimpleAnimation))]
public class SimpleAnimationDrawer : PropertyDrawer {

    const float margin = 5f;
    const float preferredFloatFieldWidth = 50f;

    float halfMargin => margin / 2f;

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);
        float remainingWidth = position.width;
        Rect animCurveRect, floatFieldRect;
        if(remainingWidth < 2f * preferredFloatFieldWidth){
            float halfWidth = remainingWidth / 2f;
            animCurveRect = new Rect(position.x, position.y, halfWidth - halfMargin, position.height);
            floatFieldRect = new Rect(position.x + halfWidth + margin, position.y, halfWidth - halfMargin, position.height);
        }else{
            animCurveRect = new Rect(position.x, position.y, remainingWidth - preferredFloatFieldWidth - halfMargin, position.height);
            floatFieldRect = new Rect(position.x + animCurveRect.width + margin, position.y, preferredFloatFieldWidth - halfMargin, position.height);
        }
        EditorGUI.PropertyField(animCurveRect, property.FindPropertyRelative("m_animCurve"), GUIContent.none);
        EditorGUI.PropertyField(floatFieldRect, property.FindPropertyRelative("m_animDuration"), GUIContent.none);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }
	
}
