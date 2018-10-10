using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioPauseMenuScript : MonoBehaviour {

	[Header("External")]
	public AudioCameraMoveScript camScript;
	public AudioListenerScript audioListener;
	public NotQuiteLineDisplayScript waveformDisplay;
	public NotQuiteLineDisplayScript fourierDisplay;

	[Header("Internal")]
	public Text mouseSensitivityLabel;
	public Dropdown drawModeDropdown;
	public GameObject aboutTab;
	public GameObject optionsTab;

	public Text waveformSampleLengthText;
	public Text fourierMinFreqText;
	public Text fourierMaxFreqText;
	public Text fourierMinSampleLengthText;
	public Text fourierMinIntervalText;
	public Text fourierResolutionText;

	public AudioSource audioSrc;
	public AudioClip audioAck;
	public AudioClip audioConf;
	public AudioClip audioErr;

	private static AudioPauseMenuScript instance;

	void Start(){
		instance = this;
		SetMouseLabelText();
		PopulateDrawModeDropdown();
		SetDropdownValue();
		SetAudioListenerSettingsTexts();
		//ActivateAboutTab();
		ActivateOptionsTab();
	}

	void Update(){
		
	}

	void OnEnable(){
		Conf();
	}

	public static void PlayAckSound(){
		instance.Ack();
	}

	void Ack(){
		audioSrc.PlayOneShot(audioAck);
	}

	public static void PlayConfSound(){
		instance.Conf();
	}

	void Conf(){
		audioSrc.PlayOneShot(audioConf);
	}

	public static void PlayErrSound(){
		instance.Err();
	}

	void Err(){
		audioSrc.PlayOneShot(audioErr);
	}

	public void ActivateAboutTab(){
		aboutTab.SetActive(true);
		optionsTab.SetActive(false);
	}

	public void ActivateOptionsTab(){
		aboutTab.SetActive(false);
		optionsTab.SetActive(true);
	}

	public void QuitApplication(){
		Application.Quit();
	}

	public void SetMouseSensitivity(string stringVal){
		float value;
		if(float.TryParse(stringVal, out value)){
			camScript.mouseSensitivity = value;
			Conf();
		}
	}

	public void SetWaveformSampleLength(string stringVal){
		int value;
		if(int.TryParse(stringVal, out value)){
			if(value > 0){
				audioListener.waveformSampleLength = value;
				audioListener.ResetWaveformComponents();
				Conf();
			}else{
				Err();
			}
		}
	}

	public void SetFourierMinFreq(string stringVal){
		int value;
		if(int.TryParse(stringVal, out value)){
			if(value > 0 && value < audioListener.fourierMaxFreq){
				audioListener.fourierMinFreq = value;
				audioListener.ResetFourierComponents();
				Conf();
			}else{
				Err();
			}
		}
	}

	public void SetFourierMaxFreq(string stringVal){
		int value;
		if(int.TryParse(stringVal, out value)){
			if(value <= (AudioSettings.outputSampleRate/2) && value > audioListener.fourierMinFreq){
				audioListener.fourierMaxFreq = value;
				audioListener.ResetFourierComponents();
				Conf();
			}else{
				Err();
			}
		}
	}

	public void SetFourierMinSampleLength(string stringVal){
		int value;
		if(int.TryParse(stringVal, out value)){
			if(value >= 0){
				audioListener.fourierMinSampleLength = value;
				audioListener.ResetFourierComponents();
				Conf();
			}else{
				Err();
			}
		}
	}

	public void SetFourierMinUpdateInterval(string stringVal){
		float value;
		if(float.TryParse(stringVal, out value)){
			if(value >= 0f){
				audioListener.fourierUpdateInterval = value;
				audioListener.ResetFourierComponents();
				Conf();
			}else{
				Err();
			}
		}
	}

	public void SetFourierResolution(string stringVal){
		int value;
		if(int.TryParse(stringVal, out value)){
			if(value >= 1){
				audioListener.fourierResolutionPerOctave = value;
				audioListener.ResetFourierComponents();
				Conf();
			}else{
				Err();
			}
		}
	}

	public void SetDrawMode(int value){
		string stringValue = drawModeDropdown.options[value].text;
		NotQuiteLineDisplayScript.DrawMode mode = (NotQuiteLineDisplayScript.DrawMode)System.Enum.Parse(typeof(NotQuiteLineDisplayScript.DrawMode), stringValue);
		waveformDisplay.drawMode = mode;
		fourierDisplay.drawMode = mode;
		Conf();
	}

	void SetMouseLabelText(){
		string mouseLabelText = camScript.mouseSensitivity.ToString();
		if(mouseLabelText.Length > 3) mouseLabelText = mouseLabelText.Substring(0,3);
		mouseSensitivityLabel.text = mouseLabelText;
	}

	void PopulateDrawModeDropdown(){
		drawModeDropdown.ClearOptions();
		List<string> drawModeOptions = new List<string>();
		foreach(NotQuiteLineDisplayScript.DrawMode mode in System.Enum.GetValues(typeof(NotQuiteLineDisplayScript.DrawMode))){
			drawModeOptions.Add(mode.ToString());
		}
		drawModeDropdown.AddOptions(drawModeOptions);
	}

	void SetDropdownValue(){
		if(waveformDisplay.drawMode != fourierDisplay.drawMode){
			Debug.LogError("wave : " + waveformDisplay.drawMode.ToString());
			Debug.LogError("four : " + fourierDisplay.drawMode.ToString());
			throw new UnityException("set the default drawmodes to the same value");
		}
		NotQuiteLineDisplayScript.DrawMode activeMode = waveformDisplay.drawMode;
		int indexInList = -1;
		for(int i=0; i<drawModeDropdown.options.Count; i++){
			string listValue = drawModeDropdown.options[i].text;
			if(listValue.Equals(activeMode.ToString())){
				indexInList = i;
				break;
			}
		}
		if(indexInList == -1) throw new UnityException("the currently active drawmode isn't listed in the dropdown");
		drawModeDropdown.value = indexInList;
	}

	void SetAudioListenerSettingsTexts(){
		waveformSampleLengthText.text = audioListener.waveformSampleLength.ToString();
		fourierMinFreqText.text = audioListener.fourierMinFreq.ToString();
		fourierMaxFreqText.text = audioListener.fourierMaxFreq.ToString();
		fourierMinSampleLengthText.text = audioListener.fourierMinSampleLength.ToString();
		fourierMinIntervalText.text = audioListener.fourierUpdateInterval.ToString();
		fourierResolutionText.text = audioListener.fourierResolutionPerOctave.ToString();
	}

}
