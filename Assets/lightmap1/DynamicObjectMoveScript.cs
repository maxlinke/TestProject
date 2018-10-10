using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectMoveScript : MonoBehaviour {

	[Header("Input")]
	public KeyCode keyForwards;
	public KeyCode keyBackwards;
	public KeyCode keyLeft;
	public KeyCode keyRight;
	[Header("Params")]
	public Rigidbody rb;
	public float speed;

	void Start () {
		
	}
	
	void Update () {
		
	}

	void FixedUpdate(){
		if(Input.GetKey(keyForwards)) rb.AddTorque(Vector3.right * speed);
		if(Input.GetKey(keyBackwards)) rb.AddTorque(Vector3.left * speed);
		if(Input.GetKey(keyLeft)) rb.AddTorque(Vector3.forward * speed);
		if(Input.GetKey(keyRight)) rb.AddTorque(Vector3.back * speed);
	}

}
