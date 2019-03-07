using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Action = System.Action;
using Rethoric = Mesmer.SpeechMinigame.SpeechMinigameRhetoric;

namespace Mesmer {

	public class SpeechMinigameUIRhetoricSelector : MonoBehaviour {

		[Header("Components")]
		[SerializeField] RectTransform rectTransform;
		[SerializeField] Button button;
		[SerializeField] Text rhetoricText;
		[SerializeField] GameObject selectionConfirmObject;

		bool m_selectionState;

		public RectTransform RectTransform { get { return rectTransform; }}
		public Rethoric Rhetoric { get; private set; }

		public bool SelectionState {
			get {
				return m_selectionState;
			} set {
				m_selectionState = value;
				if(value){
					selectionConfirmObject.SetActive(true);
				}else{
					selectionConfirmObject.SetActive(false);
				}
			}
		}

		void Awake () {
			SelectionState = false;
		}

		public void Initialize (Rethoric rhetoric, Action buttonClickCallback) {
			this.Rhetoric = rhetoric;
			rhetoricText.text = rhetoric.ToString();
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => {
				SelectionState = !SelectionState;
				buttonClickCallback.Invoke();
			});
		}

		public void SetInteractable (bool value) {
			button.interactable = value;
		}

	}

}