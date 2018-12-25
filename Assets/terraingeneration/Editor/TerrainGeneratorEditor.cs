using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {

	TerrainGenerator tg;

	void OnEnable () {
		tg = target as TerrainGenerator;
	}

	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		if(GUILayout.Button("Generate")){
			tg.Generate();
		}
	}

}
