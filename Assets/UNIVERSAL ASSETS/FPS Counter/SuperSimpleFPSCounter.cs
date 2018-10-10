using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuperSimpleFPSCounter : MonoBehaviour {

	[SerializeField] Text textField;
	
	void Update () {
		textField.text = ((int)(1f / Time.deltaTime)).ToString();
	}
}
