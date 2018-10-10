using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BallisticArrowShooter))]
public class BallisticArrowShooterEditor : Editor {

	public override void OnInspectorGUI (){
		BallisticArrowShooter bas = target as BallisticArrowShooter;

		DrawDefaultInspector();

		if(bas.isInitialized){
			if(GUILayout.Button("Shoot High")){
				bas.ShootHigh();
			}
			if(GUILayout.Button("Shoot Low")){
				bas.ShootLow();
			}
		}

	}

}
