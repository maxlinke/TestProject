using UnityEngine;
using UnityEditor;

public static class EditorTools {

	public static void DrawScriptField (MonoBehaviour monoBehaviour) {
		DrawScriptField(GetMonoScript(monoBehaviour), monoBehaviour.GetType());
	}

	public static void DrawScriptField (ScriptableObject scriptableObject) {
		DrawScriptField(GetMonoScript(scriptableObject), scriptableObject.GetType());
	}

	public static void DrawScriptField (MonoScript monoScript, System.Type type) {
		bool guiEnabled = GUI.enabled;
		GUI.enabled = false;
		EditorGUILayout.ObjectField("Script", monoScript, type, false);
		GUI.enabled = guiEnabled;
	}

	static MonoScript GetMonoScript (MonoBehaviour monoBehaviour) {
		if(monoBehaviour == null) return null;
		else return MonoScript.FromMonoBehaviour(monoBehaviour);
	}

	static MonoScript GetMonoScript (ScriptableObject scriptableObject) {
		if(scriptableObject == null) return null;
		else return MonoScript.FromScriptableObject(scriptableObject);
	}

}
