using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyCenterOfMassSetterAndShower : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] Transform customCenterOfMass;

	void Start () {
		if(customCenterOfMass != null) rb.centerOfMass = rb.transform.InverseTransformPoint(customCenterOfMass.position);
	}

	void Reset () {
		rb = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate () {
		Debug.DrawRay(rb.worldCenterOfMass, rb.transform.right, Color.red, Time.fixedDeltaTime, true);
		Debug.DrawRay(rb.worldCenterOfMass, rb.transform.up, Color.green, Time.fixedDeltaTime, true);
		Debug.DrawRay(rb.worldCenterOfMass, rb.transform.forward, Color.blue, Time.fixedDeltaTime, true);
		Debug.DrawRay(rb.worldCenterOfMass, rb.angularVelocity, Color.white, Time.fixedDeltaTime, false);
	}
}
