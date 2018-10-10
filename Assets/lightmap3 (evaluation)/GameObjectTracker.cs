using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTracker : MonoBehaviour {

	[SerializeField] GameObject target;
	[SerializeField] float speed;
	
	void Update () {
		transform.rotation = Quaternion.LookRotation(target.transform.position - this.transform.position, Vector3.up);
		if(Input.GetKey(KeyCode.UpArrow)) transform.position += transform.forward * speed * Time.deltaTime;
		if(Input.GetKey(KeyCode.DownArrow)) transform.position -= transform.forward * speed * Time.deltaTime;
	}
}
