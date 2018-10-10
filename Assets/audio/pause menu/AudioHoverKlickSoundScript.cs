using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AudioHoverKlickSoundScript : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler {

	public void OnPointerEnter(PointerEventData ped){
		AudioPauseMenuScript.PlayAckSound();
	}

	public void OnPointerDown(PointerEventData ped){
		AudioPauseMenuScript.PlayAckSound();
	}

}
