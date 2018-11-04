using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicRigidbodyController : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] float moveSpeed;
	[SerializeField] float turnSpeed;

	void Start () {
		rb.isKinematic = true;
	}

	void Reset () {
		rb = GetComponent<Rigidbody>();
		moveSpeed = 1f;
		turnSpeed = 90f;
	}
	
	void FixedUpdate () {
		Vector3 move = Vector3.zero;
		if(Input.GetKey(KeyCode.W)) move += Vector3.forward;
		if(Input.GetKey(KeyCode.S)) move += Vector3.back;
		if(Input.GetKey(KeyCode.A)) move += Vector3.left;
		if(Input.GetKey(KeyCode.D)) move += Vector3.right;
		if(Input.GetKey(KeyCode.E)) move += Vector3.up;
		if(Input.GetKey(KeyCode.Q)) move += Vector3.down;
		if(move.magnitude > 1f) move = move.normalized;
		if(Input.GetKey(KeyCode.LeftShift)) move *= 2f;
		move = rb.transform.TransformDirection(move) * moveSpeed;
		rb.MovePosition(rb.transform.position + (move * Time.fixedDeltaTime));
		float turn = 0f;
		if(Input.GetKey(KeyCode.LeftArrow)) turn -= 1f;
		if(Input.GetKey(KeyCode.RightArrow)) turn += 1f;
		Vector3 newEuler = rb.transform.localEulerAngles + (Vector3.up * turn * turnSpeed * Time.fixedDeltaTime);
		rb.MoveRotation(Quaternion.Euler(newEuler));
	}
}
