using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LightSwitch : MonoBehaviour {

	[SerializeField] KeyCode shadowToggleKey;
	[SerializeField] List<Light> lights;

	[ContextMenu("Auto create array")]
	void AutoCreateArray () {
		lights.Clear();
		lights.AddRange(GameObject.FindObjectsOfType<Light>());
	}

	public Light[] GetLights () {
		return lights.ToArray();
	}

	public Light[] GetRealtimeLights () {
		List<Light> realtimeLights = new List<Light>();
		foreach(Light l in lights){
			if(l.lightmapBakeType != LightmapBakeType.Baked){
				realtimeLights.Add(l);
			}
		}
		return realtimeLights.ToArray();
	}
	
	void Update () {
		ToggleLight(KeyCode.Alpha1, 0);
		ToggleLight(KeyCode.Alpha2, 1);
		ToggleLight(KeyCode.Alpha3, 2);
		ToggleLight(KeyCode.Alpha4, 3);
		ToggleLight(KeyCode.Alpha5, 4);
		ToggleLight(KeyCode.Alpha6, 5);
		ToggleLight(KeyCode.Alpha7, 6);
		ToggleLight(KeyCode.Alpha8, 7);
		ToggleLight(KeyCode.Alpha9, 8);
		ToggleLight(KeyCode.Alpha0, 9);
		if(Input.GetKeyDown(shadowToggleKey)) ToggleShadows();
	}

	void ToggleShadows () {
		foreach(Light l in lights){
			if(l.renderMode != LightRenderMode.ForcePixel){			//if light "not important"
				if(l.lightmapBakeType != LightmapBakeType.Baked){	//if light not baked
					if(l.shadows == LightShadows.None){
						l.shadows = LightShadows.Soft;
					}else{
						l.shadows = LightShadows.None;
					}
				}
			}
		}
	}

	void ToggleLight (KeyCode kcode, int index) {
		if(Input.GetKeyDown(kcode)) TryToggleLight(index);
	}

	void TryToggleLight (int index) {
		try {
			lights[index].enabled = !lights[index].enabled;
		} catch (IndexOutOfRangeException e) {
			Debug.Log("No light at index \"" + index + "\"\n" + e.StackTrace);
		}
	}

}
