using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectMover : MonoBehaviour {

	[SerializeField] float speed;

	void Update () {
		Vector3 input = Vector3.zero;
		if(Input.GetKey(KeyCode.W)) input += Vector3.forward;
		if(Input.GetKey(KeyCode.A)) input += Vector3.left;
		if(Input.GetKey(KeyCode.S)) input += Vector3.back;
		if(Input.GetKey(KeyCode.D)) input += Vector3.right;
		Vector3 transformed = Camera.main.transform.TransformDirection(input);
		Vector3 projected = Vector3.ProjectOnPlane(transformed, Vector3.up);
		Vector3 actualInput = projected.normalized;
		transform.position += actualInput * speed * Time.deltaTime;
	}
}
