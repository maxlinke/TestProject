using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomboxControllerScript : MonoBehaviour {

	[SerializeField]
	AudioSource audioSource;
	[SerializeField]
	AudioGUIScript audioGUI;

	public KeyCode keyClipCycle;
	public KeyCode key2DToggle;

	private List<AudioClip> audioClips;
	private int songIndex;

	void Start () {
		audioClips = new List<AudioClip>();
		songIndex = 0;
		NextSong();
	}
	
	void Update () {
		if(Input.GetKeyDown(keyClipCycle)){
			NextSong();
		}
		if(Input.GetKeyDown(key2DToggle)){
			if(audioSource.spatialBlend < 0.5f)	audioSource.spatialBlend = 1f;
			else audioSource.spatialBlend = 0f;
		}
	}

	private void NextSong(){
		if(audioClips.Count > 0){
			audioSource.clip = audioClips[songIndex];
			audioGUI.SetFileName(audioSource.clip.name);
			audioSource.Play();
			songIndex = (songIndex + 1) % audioClips.Count;
		}
	}

	public bool IsInitialized(){
		return audioClips != null;
	}

	public void AddAudioClip(AudioClip clip){
		audioClips.Add(clip);
	}
}
