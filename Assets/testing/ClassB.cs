using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassB : ClassA {

	protected override void Start () {
		base.Start();
		Debug.Log("B start");
	}

	void Update () {
		Debug.Log("B update");
	}

	void OnTriggerEnter (Collider otherCollider) {
		Debug.LogWarning("B triggerenter");
	}
}
