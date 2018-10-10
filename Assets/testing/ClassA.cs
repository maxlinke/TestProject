using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassA : MonoBehaviour {

	protected virtual void Start () {
		Debug.Log("A start");
	}
	
	void Update () {
		Debug.Log("A update");
	}

	void OnTriggerEnter (Collider otherCollider) {
		Debug.LogWarning("A triggerenter");
	}
}
