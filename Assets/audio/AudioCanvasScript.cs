using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCanvasScript : MonoBehaviour {

	public GameObject pauseMenu;

	void Start () {
		Unpause();
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)){
			if(pauseMenu.activeSelf){
				Unpause();
			}else{
				Pause();
			}
		}
	}

	public void Pause(){
		pauseMenu.SetActive(true);
		SetTimeScale(0f);
		Cursor.lockState = CursorLockMode.None;
	}

	public void Unpause(){
		pauseMenu.SetActive(false);
		SetTimeScale(1f);
		Cursor.lockState = CursorLockMode.Locked;
	}

	void SetTimeScale(float timeScale){
		Time.timeScale = timeScale;
		AffectAllAudioSources();
	}

	void AffectAllAudioSources(){
		AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
		foreach(AudioSource audSrc in audioSources){
			if(!(audSrc.gameObject.layer == LayerMask.NameToLayer("UI"))){
				audSrc.pitch = Time.timeScale;
			}
		}
	}

}
