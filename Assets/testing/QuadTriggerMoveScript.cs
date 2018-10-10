using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTriggerMoveScript : MonoBehaviour {

	[Header("Straight up moving")]
	public float moveSpeed;

	[Header("Physicsy movement")]
	public Rigidbody rb;
	public float acceleration;
	public float boost;
	public float jumpVelocity;

	private Vector3 inputVector;
	private bool spaceDown;
	private bool leftShiftDown;
	private bool rDown;

	void Start () {
		
	}
	
	void Update () {
		inputVector = GetInputVector();
		SetInputBools();
		//UpdateMovement();
	}

	void FixedUpdate(){
		//FixedUpdateMovement();
		RigidbodyMovement();
	}

	void UpdateMovement(){
		float moveDistance = moveSpeed * Time.deltaTime;
		transform.position += inputVector.normalized * moveDistance;
	}

	void FixedUpdateMovement(){
		float moveDistance = moveSpeed * Time.fixedDeltaTime;
		rb.MovePosition(transform.position + inputVector.normalized * moveDistance);
	}

	void RigidbodyMovement(){
		Vector3 inputX = new Vector3(inputVector.x, 0f, 0f);
		rb.velocity += inputX.normalized * acceleration * Time.fixedDeltaTime;
		if(spaceDown) rb.velocity += Vector3.up * jumpVelocity;
		if(leftShiftDown){
			rb.velocity += inputX.normalized * boost;
		}
		if(rDown){
			rb.velocity = Vector3.zero;
			transform.position = new Vector3(0f ,1.5f, 0f);
		}
		rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0f);
		ResetInputBools();
	}

	Vector3 GetInputVector(){
		Vector3 output = new Vector3(0,0,0);
		if(Input.GetKey(KeyCode.W)) output += Vector3.up;
		if(Input.GetKey(KeyCode.S)) output += Vector3.down;
		if(Input.GetKey(KeyCode.A)) output += Vector3.left;
		if(Input.GetKey(KeyCode.D)) output += Vector3.right;
		return output;
	}

	void SetInputBools(){
		spaceDown = spaceDown || Input.GetKeyDown(KeyCode.Space);
		leftShiftDown = leftShiftDown || Input.GetKeyDown(KeyCode.LeftShift);
		rDown = rDown || Input.GetKeyDown(KeyCode.R);
	}

	void ResetInputBools(){
		spaceDown = false;
		leftShiftDown = false;
		rDown = false;
	}

}
