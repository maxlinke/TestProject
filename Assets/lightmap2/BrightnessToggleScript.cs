using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessToggleScript : MonoBehaviour {

	public Light lightsource;
	public float brightnessOne;
	public float brightnessTwo;
	public KeyCode keyToggle;

	void Start () {
		lightsource.intensity = brightnessOne;
	}
	
	void Update () {
		if(Input.GetKeyDown(keyToggle)){
			if(lightsource.intensity == brightnessOne) lightsource.intensity = brightnessTwo;
			else lightsource.intensity = brightnessOne;
		}
	}
}
