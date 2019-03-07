using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Mesmer {

	public class SpeechMinigameUIRhetoricCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

		[SerializeField] RectTransform rectTransform;
		[SerializeField] Text rhetoricTextField;

		Action clickAction;

		public RectTransform RectTransform { get { return rectTransform; } }

		void Awake () {
			transform.localScale = Vector3.one;
		}

		public void Initialize (SpeechMinigame.SpeechMinigameRhetoric rethoric, Action clickAction) {
			rhetoricTextField.text = rethoric.ToString();
			this.clickAction = clickAction;
		}

		public void OnPointerEnter (PointerEventData ped) {
			transform.localScale = Vector3.one * 0.8f;
		}

		public void OnPointerExit (PointerEventData ped) {
			transform.localScale = Vector3.one;
		}

		public void OnPointerClick (PointerEventData ped) {
			clickAction.Invoke();
		}

	}

}