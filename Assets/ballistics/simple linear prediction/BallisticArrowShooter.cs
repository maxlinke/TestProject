using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallisticArrowShooter : MonoBehaviour {

	[SerializeField] GameObject launchPointObject;
	[SerializeField] GameObject targetObject;
	[SerializeField] GameObject arrowPrefab;

	[SerializeField] float minArrowVelocity;
	[SerializeField] float maxArrowVelocity;

	[SerializeField] bool autoLaunch;
	[SerializeField] float autoLaunchInterval;
	float nextAutoLaunch;

	bool initialized;
	public bool isInitialized{get{return initialized;}}

	void Start () {
		initialized = true;
	}
	
	void Update () {
		if(autoLaunch){
			if(Time.time > nextAutoLaunch){
				Shoot(Random.value > 0.5f);
				nextAutoLaunch = Time.time + autoLaunchInterval;
			}
		}
	}

	public void ShootLow(){
		Shoot(false);
	}

	public void ShootHigh(){
		Shoot(true);
	}

	void Shoot(bool high){
		float projectileVelocity = Random.Range(minArrowVelocity, maxArrowVelocity);
		Vector3 targetDelta = targetObject.transform.position - launchPointObject.transform.position;
		float targetDistance = new Vector3(targetDelta.x, 0f, targetDelta.z).magnitude;
		float targetHeight = targetDelta.y;
		float lowAngle, highAngle;
		CalculateLaunchAngles(projectileVelocity, targetDistance, targetHeight, out lowAngle, out highAngle);
		float launchAngle = (high ? highAngle : lowAngle);
		if(float.IsNaN(launchAngle)){
			Debug.LogWarning("no launch solution found | " + Time.time.GetHashCode().ToString());
		}else{
			float verticalVelocity = Mathf.Sin(launchAngle) * projectileVelocity;
			float lateralVelocity = Mathf.Cos(launchAngle) * projectileVelocity;
			Vector3 targetDir = new Vector3(targetDelta.x, 0f, targetDelta.z).normalized;
			Vector3 launchVelocity = targetDir * lateralVelocity + Vector3.up * verticalVelocity;
			Rigidbody arrowRB = Instantiate(arrowPrefab, launchPointObject.transform.position, Quaternion.LookRotation(launchVelocity)).GetComponent<Rigidbody>();
			arrowRB.velocity = launchVelocity;
		}
	}

	void CalculateLaunchAngles(float projectileVelocity, float targetDistance, float targetHeight, out float lowAngle, out float highAngle){
		float v = projectileVelocity;
		float g = Physics.gravity.magnitude;
		float x = targetDistance;
		float y = targetHeight;

		float sqrtThing = Mathf.Sqrt(Mathf.Pow(v, 4) - (g * ((g * x * x) + (2f * y * v * v))));
		float angle1 = Mathf.Atan(((v * v) - sqrtThing) / (g * x));
		float angle2 = Mathf.Atan(((v * v) + sqrtThing) / (g * x));
		lowAngle = Mathf.Min(angle1, angle2);
		highAngle = Mathf.Max(angle1, angle2);
	}
}
