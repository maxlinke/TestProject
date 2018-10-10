using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPointLightScript : MonoBehaviour {

	public Light lamp;
	public GameObject centerOfRotation;
	public Vector3 axisOfRotation;
	public float orbitTime;

	void Start(){
		
	}
	
	void Update(){
		lamp.transform.RotateAround(centerOfRotation.transform.position, axisOfRotation, (360f / orbitTime) * Time.deltaTime);
	}
}
