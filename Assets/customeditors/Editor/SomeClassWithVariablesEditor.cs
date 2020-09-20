using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

[CustomEditor(typeof(SomeClassWithVariables))]
public class SomeClassWithVariablesEditor : Editor {

	bool toggleGroupToggle;

	public override void OnInspectorGUI () {
//		DrawDefaultInspector();
		SomeClassWithVariables scwv = target as SomeClassWithVariables;

		serializedObject.Update();

		EditorTools.DrawScriptReference(scwv);
//		DrawScriptReference(scwv);
//		EditorGUILayout.Space();
//		DrawConditionalPart_Old(scwv);
		DrawConditionalPart_New(scwv);
		DrawToggleGroup(scwv);
		DrawPrivateBoolGroup(scwv);

		serializedObject.ApplyModifiedProperties();
	}

	void DrawScriptReference (SomeClassWithVariables scwv) {
		GUI.enabled = false;
		MonoScript script = MonoScript.FromMonoBehaviour((SomeClassWithVariables)target);
		EditorGUILayout.ObjectField("Script", script, script.GetType(), false);
		GUI.enabled = true;
	}

	void DrawConditionalPart_Old (SomeClassWithVariables scwv) {
		Undo.RecordObject(scwv, scwv.name);
		int origLevel = EditorGUI.indentLevel;

		if(scwv.executeAction = EditorGUILayout.Toggle("Execute Action", scwv.executeAction)){
			EditorGUI.indentLevel = origLevel + 1;
			if(!(scwv.everyUpdate = EditorGUILayout.Toggle("Every Update", scwv.everyUpdate))){
				EditorGUI.indentLevel = origLevel + 2;
				scwv.waitTime = EditorGUILayout.FloatField("Wait Time", scwv.waitTime);
			}
			EditorGUI.indentLevel = origLevel + 1;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("someEvent"), new GUIContent("Some Event"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("someFloat"), new GUIContent("Some Float"), true);
		}

		EditorGUI.indentLevel = origLevel;
	}

	void DrawConditionalPart_New (SomeClassWithVariables scwv) {
		int origLevel = EditorGUI.indentLevel;

		SerializedProperty condProp = serializedObject.FindProperty("executeAction");
		EditorGUILayout.PropertyField(condProp, new GUIContent("Execute Action"), true);
		if(condProp.boolValue){
			EditorGUI.indentLevel = origLevel + 1;
			SerializedProperty updateProp = serializedObject.FindProperty("everyUpdate");
			EditorGUILayout.PropertyField(updateProp, new GUIContent("Every Update?"), true);
			if(!updateProp.boolValue){
				EditorGUI.indentLevel = origLevel + 2;
				SerializedProperty freqProp = serializedObject.FindProperty("waitTime");
				EditorGUILayout.PropertyField(freqProp, new GUIContent("Wait Time"), true);
			}
			EditorGUI.indentLevel = origLevel + 1;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("someEvent"), new GUIContent("Some Event"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("someFloat"), new GUIContent("Some Float"), true);
		}

		EditorGUI.indentLevel = origLevel;
	}

	void DrawToggleGroup (SomeClassWithVariables scwv) {
		toggleGroupToggle = EditorGUILayout.BeginToggleGroup("Toggle Group", toggleGroupToggle);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("firstInt"), new GUIContent("First Int"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("secondInt"), new GUIContent("Second Int"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("thirdInt"), new GUIContent("Third Int"), true);
		EditorGUILayout.EndToggleGroup();
	}

	void DrawPrivateBoolGroup (SomeClassWithVariables scwv) {
		SerializedProperty privateBoolProperty = serializedObject.FindProperty("aPrivateBool");
		EditorGUILayout.PropertyField(privateBoolProperty, new GUIContent("A Private Bool"), true);
		bool boolValue = privateBoolProperty.boolValue;
		if(boolValue){
			string message = "This is a message.\nIt has multiple lines, one of which is quite long.\n" +
				"The third line is really long. Because I want to see how long the help boxes allow text to be. " +
				"So let me tell you about my day... It's been hell, because hiding properties with a custom attribute " +
				"is a pain in the ass and doesn't even work properly. I mean, how does [HideInInspector] work exactly? " +
				"It can hide e.g. UnityEvents really nicely while I can't :<";
//			EditorGUILayout.LabelField(message);
			EditorGUILayout.HelpBox(message, MessageType.Info);
		}
	}

}
