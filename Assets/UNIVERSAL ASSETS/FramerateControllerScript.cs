using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramerateControllerScript : MonoBehaviour {

	void Start(){
		
	}
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.Alpha0)) Application.targetFrameRate = -1;
		if(Input.GetKeyDown(KeyCode.Alpha1)) Application.targetFrameRate = 60;
		if(Input.GetKeyDown(KeyCode.Alpha2)) Application.targetFrameRate = 30;
		if(Input.GetKeyDown(KeyCode.Alpha3)) Application.targetFrameRate = 15;
		if(Input.GetKeyDown(KeyCode.Alpha4)) Application.targetFrameRate = 5;
		if(Input.GetKeyDown(KeyCode.Alpha5)) Application.targetFrameRate = 1;
	}
}
