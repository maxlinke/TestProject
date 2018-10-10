using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KlickableWaypointAI : MonoBehaviour {

		[Header("Components")]
	[SerializeField] Rigidbody rb;

		[Header("Settings")]
	[SerializeField] [Range(0, 10f)] float waypointRadius;
	[SerializeField] bool arrive;
	[SerializeField] float maxVelocity;
	[SerializeField] float maxAccel;

	Vector3 lastPos;
	Vector3 currentTarget;

	void Start () {
		
	}
	
	void Update () {
		Debug.DrawRay(currentTarget, Vector3.up, Color.green, 0f, false);
		if(Input.GetKeyDown(KeyCode.Mouse0)){
			SetNewTargetViaClick();
		}
	}

	void FixedUpdate () {
		Vector3 deltaPos = currentTarget - transform.position;
		Vector3 desiredVelocity;
		if((deltaPos.magnitude > maxVelocity) || (!arrive)){
			desiredVelocity = deltaPos.normalized * maxVelocity;
		}else{
			desiredVelocity = deltaPos;
		}
		Vector3 deltaV = desiredVelocity - rb.velocity;
		Vector3 accel;
		if(deltaV.magnitude > maxAccel){
			accel = deltaV.normalized * maxAccel;
		}else{
			accel = deltaV;
		}

		Debug.DrawRay(transform.position, desiredVelocity, Color.cyan, Time.fixedDeltaTime, false);
		Debug.DrawRay(transform.position, accel, Color.yellow, Time.fixedDeltaTime, false);

		rb.velocity += accel * Time.fixedDeltaTime;
		rb.MoveRotation(Quaternion.LookRotation(rb.velocity, Vector3.up));

		Debug.DrawLine(transform.position, lastPos, Color.white, 10f, false);
		lastPos = transform.position;
	}

	void SetNewTargetViaClick () {
		Vector3 camPos = Camera.main.transform.position;
		Vector3 mouseDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward) - camPos).normalized;
		float angle = Vector3.Angle(mouseDir, Vector3.down);
//		Debug.Log(angle);
		float d = camPos.y / Mathf.Cos(Mathf.Deg2Rad * angle);
		Vector3 point = camPos + (mouseDir * d);
//		Debug.DrawRay(point, Vector3.up, Color.red, 1f, false);
		currentTarget = point;
	}
}
