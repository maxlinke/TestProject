using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionInstanceStatesHint : MonoBehaviour {

	[SerializeField] RectTransform rectTransform;
	[SerializeField] Text textField;
	[SerializeField] float horizontalPadding;
	[SerializeField] float verticalPadding;

	[System.NonSerialized] public bool activeStateOverridden;
	public RectTransform RectTransform { get { return rectTransform; } }

	int m_unreadMissions;
	int m_endingMissions;

	public int unreadMissions {
		get {
			return m_unreadMissions;
		} set {
			if(value != m_unreadMissions){
				m_unreadMissions = value;
				UpdateDisplay();
			}
		}
	}

	public int endingMissions {
		get {
			return m_endingMissions;
		} set {
			if(value != m_endingMissions){
				m_endingMissions = value;
				UpdateDisplay();
			}
		}
	}

	public void Init (int unreadMissions, int endingMissions) {
		this.unreadMissions = unreadMissions;
		this.endingMissions = endingMissions;
		UpdateDisplay();
	}

	public void UpdateDisplay () {
		if(unreadMissions > 0 || endingMissions > 0){
			string outputText = string.Empty;
			if(unreadMissions > 0) outputText += unreadMissions + " unread missions";
			if(unreadMissions > 0 && endingMissions > 0) outputText += "\n";
			if(endingMissions > 0) outputText += endingMissions + " missions running out";
			textField.text = outputText;
			ScaleToFitContent();
			if(!activeStateOverridden) gameObject.SetActive(true);
		}else{
			if(!activeStateOverridden) gameObject.SetActive(false);
		}
	}

	void ScaleToFitContent () {
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textField.preferredHeight + (2f * verticalPadding));
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textField.preferredWidth + (2f * horizontalPadding));
	}
}
