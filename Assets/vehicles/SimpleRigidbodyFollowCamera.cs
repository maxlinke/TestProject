using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRigidbodyFollowCamera : MonoBehaviour {

	[SerializeField] Rigidbody targetRb;
	[SerializeField] float behind;
	[SerializeField] float above;

	void LateUpdate () {
		Vector3 flattenedVelocity = Vector3.ProjectOnPlane(targetRb.velocity, Vector3.up);
		Vector3 flattenedForward = Vector3.ProjectOnPlane(targetRb.transform.forward, Vector3.up);
		Vector3 referenceVector = Vector3.Lerp(flattenedForward, flattenedVelocity, Mathf.Clamp01(flattenedVelocity.magnitude)).normalized;
		Vector3 actualOffset = (referenceVector.normalized * -1f * behind) + (Vector3.up * above);
		transform.position = targetRb.transform.position + actualOffset;
		transform.LookAt(targetRb.transform.position, Vector3.up);
	}
	
}
