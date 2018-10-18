using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePosWithVelocity : MonoBehaviour {

	[SerializeField] Rigidbody rb;
	[SerializeField] Vector3 targetVelocity;
	[SerializeField] Vector3 jumpDelta;
	[SerializeField] KeyCode posMoveKey;
	[SerializeField] KeyCode negMoveKey;
	[SerializeField] KeyCode posJumpKey;
	[SerializeField] KeyCode negJumpkey;
	[SerializeField] KeyCode slowmoKey;

	float posmove, negmove, posjump, negjump;
	Vector3 lastPos;
	
	void Update () {
		if(Input.GetKey(posMoveKey)) posmove = 1f;
		if(Input.GetKey(negMoveKey)) negmove = 1f;
		if(Input.GetKeyDown(posJumpKey)) posjump = 1f;
		if(Input.GetKeyDown(negJumpkey)) negjump = 1f;
		if(Input.GetKey(slowmoKey)){
			Time.timeScale = 0.02f;
		}else{
			Time.timeScale = 1f;
		}
	}

	void FixedUpdate () {
		Debug.DrawLine(lastPos, rb.transform.position, Color.green, Mathf.Infinity, false);

		Vector3 v = targetVelocity * (posmove - negmove);
		Vector3 j = jumpDelta * (posjump - negjump);

		Debug.DrawRay(rb.transform.position, j, Color.yellow, Mathf.Infinity, false);
		Debug.DrawRay(rb.transform.position, Vector3.up * 0.1f, Color.white, 1f, false);

		rb.velocity = v;
		rb.MovePosition(rb.transform.position + j);

		Debug.DrawRay(rb.transform.position, Vector3.down * 0.1f, Color.white, 1f, false);

		lastPos = rb.transform.position;
		posmove = negmove = posjump = negjump = 0f;
		Debug.LogWarning("asdf");
	}

}
