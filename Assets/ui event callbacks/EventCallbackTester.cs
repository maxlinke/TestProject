using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventCallbackTester : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {

	[SerializeField] bool logEnter;
	[SerializeField] bool logExit;
	[SerializeField] bool logDown;
	[SerializeField] bool logUp;
	[SerializeField] bool logClick;

	void Log (string msg) {
		Debug.Log(gameObject.name + " : " + msg);
	}

	public void OnPointerEnter (PointerEventData ped) {
		if(logEnter) Log("enter");
	}

	public void OnPointerExit (PointerEventData ped) {
		if(logExit) Log("exit");
	}

	public void OnPointerDown (PointerEventData ped) {
		if(logDown) Log("down");
	}

	public void OnPointerUp (PointerEventData ped) {
		if(logUp) Log("up");
	}

	public void OnPointerClick (PointerEventData ped) {
		if(logClick) Log("click");
	}

}
