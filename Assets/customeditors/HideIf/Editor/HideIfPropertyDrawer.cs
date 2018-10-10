using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfPropertyDrawer : PropertyDrawer {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		HideIfAttribute hideIf = attribute as HideIfAttribute;
		bool shouldHide = ShouldHide(hideIf, property);
		if(shouldHide){
			switch(hideIf.disablingType){
			case DisablingType.GREYOUT: return base.GetPropertyHeight(property, label);
			case DisablingType.HIDE : return 0f;
			default : return base.GetPropertyHeight(property, label);
			}
		}else{
			return base.GetPropertyHeight(property, label);
		}
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		HideIfAttribute hideIf = attribute as HideIfAttribute;
		bool shouldHide = ShouldHide(hideIf, property);
		if (shouldHide) {
			DisablingType type = hideIf.disablingType;
			if (type.Equals(DisablingType.GREYOUT)) {
				GUI.enabled = false;
				EditorGUI.PropertyField(position, property, true);
				GUI.enabled = true;
			}else if (type.Equals(DisablingType.HIDE)) {
				//do nothing...
			}else{
				Debug.LogError("Unsupported DisablingType \"" + type.ToString() + "\"");
				EditorGUI.PropertyField(position, property, true);
			}
		}else{
			EditorGUI.PropertyField(position, property, true);
		}
	}

	bool ShouldHide (HideIfAttribute hideIf, SerializedProperty property) {
		SerializedProperty comparedField = property.serializedObject.FindProperty(hideIf.comparedPropertyName);
		if(comparedField == null){
			Debug.LogError("Unable to find property named \"" + hideIf.comparedPropertyName + "\"");
			return false;
		}
		ComparisonType compType = hideIf.comparisonType;
		switch(comparedField.propertyType){
		case SerializedPropertyType.Integer : 
			return NumericComparison(comparedField.intValue, (int)hideIf.comparedValue, compType);
		case SerializedPropertyType.Float :
			return NumericComparison(comparedField.floatValue, (float)hideIf.comparedValue, compType);
		case SerializedPropertyType.Boolean : 
			return StandardComparison(comparedField.boolValue, hideIf.comparedValue, compType);
		case SerializedPropertyType.String :
			return StandardComparison(comparedField.stringValue, hideIf.comparedValue, compType);
		case SerializedPropertyType.Enum :
			return StandardComparison(comparedField.enumValueIndex, (int)hideIf.comparedValue, compType);
		default : return false;
		}
	}

	bool NumericComparison (float a, float b, ComparisonType type) {
		switch(type){
		case ComparisonType.EQUALS : return a == b;
		case ComparisonType.GREATEROREQUAL : return a >= b;
		case ComparisonType.GREATERTHAN : return a > b;
		case ComparisonType.LESSOREQUAL : return a <= b;
		case ComparisonType.LESSTHAN : return a < b;
		case ComparisonType.NOTEQUALS : return a != b;
		default : return false;
		}
	}

	bool StandardComparison (object a, object b, ComparisonType type) {
		bool eq = a.Equals(b);
		if(type == ComparisonType.NOTEQUALS) return (!eq);
		else return eq;
	}

}
