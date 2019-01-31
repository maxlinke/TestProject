using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SomeSelectableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[SerializeField] Text textField;
	[SerializeField] Image outline;
	[SerializeField] float upscaleDuration;
	[SerializeField] float selectedScale;
	[SerializeField] float wigglePeriod;
	[SerializeField] float maxWiggleAngle;
	[SerializeField] float resetDuration;

	AnalogStickTestMenu menu;
	int m_index;
	bool m_selected;
	RectTransform m_rectTransform;

	public bool selected {
		get {
			return m_selected;
		} set {
			if(value != m_selected){
				if(value){
					OnSelect();
				}else{
					OnDeselect();
				}
			}
			m_selected = value;
		}
	}

	public RectTransform rectTransform {
		get {
			return m_rectTransform;
		}
	}

	public int index {
		get {
			return m_index;
		}
	}

	void OnEnable () {
		textField.text = GenerateRandomString(1);
		this.m_selected = false;
		OnDeselect();
		this.m_rectTransform = ((RectTransform) this.transform);
	}

	public void Initialize (AnalogStickTestMenu menu, int index) {
		this.menu = menu;
		this.m_index = index;
	}

	void OnSelect () {
		StopAllCoroutines();
		StartCoroutine(ScaleCoroutine(Vector3.one * selectedScale, upscaleDuration));
		outline.gameObject.SetActive(true);
	}

	void OnDeselect () {
		StopAllCoroutines();
		StartCoroutine(ScaleCoroutine(Vector3.one, resetDuration));
		outline.gameObject.SetActive(false);
	}

	public void OnPointerEnter (PointerEventData ped) {
		menu.HoverEnter(this);
	}

	public void OnPointerExit (PointerEventData ped) {
		menu.HoverExit(this);
	}

	IEnumerator ScaleCoroutine (Vector3 targetScale, float duration) {
		float startTime = Time.time;
		Vector3 startScale = transform.localScale;
		float t = 0;
		while(t < 1f){
			transform.localScale = Vector3.Lerp(startScale, targetScale, t);
			t = (Time.time - startTime) / duration;
			yield return null;
		}
		transform.localScale = targetScale;
	}

	IEnumerator RotationCoroutine (Quaternion targetRotation, float duration) {
		float startTime = Time.time;
		Quaternion startRotation = transform.localRotation;
		float t = 0;
		while(t < 1f){
			transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
			t = (Time.time - startTime) / duration;
			yield return null;
		}
		transform.localRotation = targetRotation;
	}

	IEnumerator WiggleCoroutine () {
		float startTime = Time.time;
		while(true){
			float t = (Time.time - startTime) / wigglePeriod;
			transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * maxWiggleAngle);
			yield return null;
		}
	}

	string GenerateRandomString (int length) {
		string output = "";
		for(int i=0; i<length; i++){
			output += (char)(Random.Range(65, 91));
		}
		return output;
	}

}
