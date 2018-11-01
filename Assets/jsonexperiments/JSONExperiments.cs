using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONExperiments : MonoBehaviour {

	StructForJSONifying jsonStruct;

	void Update () {
		if(Input.GetKeyDown(KeyCode.P)){
			Debug.Log(jsonStruct);
		}
		if(Input.GetKeyDown(KeyCode.Q)){
			string jsonified = JsonUtility.ToJson(jsonStruct);
			Debug.Log(jsonified);
		}
		if(Input.GetKeyDown(KeyCode.R)){
			jsonStruct = StructForJSONifying.defaultConfig;
		}
	}


}
