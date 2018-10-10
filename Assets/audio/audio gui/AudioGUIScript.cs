using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioGUIScript : MonoBehaviour {

	public AudioCameraMoveScript camScript;
	public Text mouseSensitivityLabel;
	public Text fileNameLabel;

	void Start(){
		string mouseLabelText = camScript.mouseSensitivity.ToString();
		if(mouseLabelText.Length > 3) mouseLabelText = mouseLabelText.Substring(0,3);
		mouseSensitivityLabel.text = mouseLabelText;
		fileNameLabel.text = "No song playing. Hit \"1\" to and play a song.";
	}
	
	void Update(){
		
	}

	public void QuitApplication(){
		Application.Quit();
	}

	public void SetMouseSensitivity(string stringVal){
		float value;
		if(float.TryParse(stringVal, out value)){
			camScript.mouseSensitivity = value;
		}
	}

	public void SetFileName(string filename){
		fileNameLabel.text = filename;
	}

}
