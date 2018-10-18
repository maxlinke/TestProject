using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedUpdateLogger : MonoBehaviour {

	[SerializeField] string fixedUpdateMessage;

	void FixedUpdate () {
		Debug.Log(fixedUpdateMessage);
	}

}
