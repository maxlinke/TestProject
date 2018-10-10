using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionMover : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] KeyCode moveKey;
	[SerializeField] Vector3 velocity;

	void Start(){
		
	}
	
	void Update(){
		
	}

	void FixedUpdate(){
		if(Input.GetKey(moveKey)){
			rb.MovePosition(transform.position + (velocity * Time.fixedDeltaTime));
		}
	}

	void OnCollisionEnter(Collision collision){
		Debug.Log("hit");
	}
}
