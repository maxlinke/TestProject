using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleArrow : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] Collider[] colliders;
	[SerializeField] GameObject dragPoint;
	[SerializeField] GameObject centerOfMass;
	[SerializeField] float maxStickAngle;
	[SerializeField] float minStickVelocity;
	[SerializeField] float customDrag;
	[SerializeField] float stickDistance;

	Vector3 lastVelocity;

	void Start () {
		rb.centerOfMass = centerOfMass.transform.position - this.transform.position;
	}
	
	void Update () {

	}

	void FixedUpdate () {
		if(rb != null){
			Vector3 customDragVec = Vector3.ProjectOnPlane(-rb.velocity, transform.forward) * customDrag;
			rb.AddForceAtPosition(customDragVec, dragPoint.transform.position);
			lastVelocity = rb.velocity;
		}
	}

	void OnCollisionEnter(Collision collision){
		float collDot = Vector3.Dot(-collision.relativeVelocity.normalized, lastVelocity.normalized);
		float tipDot = Vector3.Dot(-collision.contacts[0].normal, transform.forward);
		if((collDot > Mathf.Cos(Mathf.Deg2Rad * maxStickAngle)) && (collision.relativeVelocity.magnitude > minStickVelocity) && (tipDot > Mathf.Cos(maxStickAngle))){
			Rigidbody otherRB = collision.collider.attachedRigidbody;
			if(otherRB != null){
				otherRB.AddForce(this.rb.velocity * this.rb.mass, ForceMode.Impulse);
			}
			Destroy(rb);
			foreach(Collider c in colliders){
				Destroy(c);
			}
			this.transform.position += lastVelocity.normalized * stickDistance;
			this.transform.parent = collision.collider.transform;
			Destroy(this);
		}else{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
	}

}
