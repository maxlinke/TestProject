using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTriggerEnterScript : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		
	}

	void OnTriggerEnter(Collider coll){
		Debug.Log(coll.name + " just entered " + gameObject.name);
	}

	void OnCollisionEnter(Collision coll){
		Vector3 impulse = coll.impulse;
		Vector3 relativeVelocity = coll.relativeVelocity;
		string firstLine = coll.collider.name + " just hit " + gameObject.name;
		string secondLine = "impulse : " + impulse.magnitude;
		string thirdLine = "relative velocity : " + relativeVelocity.magnitude;
		Debug.Log(firstLine + "\n" + secondLine + "\n" + thirdLine);
	}

}
