using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheeledVehicle : MonoBehaviour {

	private const float EPSILON = 0.001f;

	[System.Serializable]
	public class Axle {

		public WheelCollider[] wheelColliders;
		public bool powered;
		public bool steering;

		public bool IsValid (Rigidbody mainRB) {
			if(wheelColliders == null){
				return false;
			}else{
				Vector3 localPos = mainRB.transform.InverseTransformPoint(wheelColliders[0].transform.position);
				float averageX = localPos.x;
				float initialY = localPos.y;
				float initialZ = localPos.z;
				for(int i=1; i<wheelColliders.Length; i++){
					localPos = mainRB.transform.InverseTransformPoint(wheelColliders[i].transform.position);
					averageX += localPos.x;
					float deltaY = localPos.y - initialY;
					float deltaZ = localPos.z - initialZ;
					if(Mathf.Abs(deltaY) > EPSILON) return false;
					if(Mathf.Abs(deltaZ) > EPSILON) return false;
				}
				return (Mathf.Abs(averageX) < EPSILON);
			}
		}

	}

	[SerializeField] Rigidbody rb;
	[SerializeField] Transform customCenterOfMass;
	[SerializeField] Axle[] axles;
	[SerializeField] GameObject wheelPrefab;

	[Header("Driving")]
	[SerializeField] float forwardSpeedLimit;
	[SerializeField] float reverseSpeedLimit;
	[SerializeField] float maxTorque;
	[Tooltip("Torque multiplier in relation to speed")] [SerializeField] AnimationCurve torqueCurve;
	[SerializeField] float maxBrakeTorque;

	[Header("Steering")]
	[SerializeField] float turnRadius;
	[Tooltip("1 \"steer\" is equal to the difference from neutral steer to full steer...")] [SerializeField] float steerPerSecond;
	[Tooltip("Tightness of turn radius in relation to speed")] [SerializeField] AnimationCurve steerCurve;

	float steer;
	Vector3 steerPivot;
	int numberOfPoweredWheels;

	void Start () {
		ApplyCustomCenterOfMass(rb, customCenterOfMass);
		CheckValidityOfAxles(axles, rb);
		steerPivot = GetPivot(axles, rb);
		for(int i=0; i<axles.Length; i++){
			for(int j=0; j<axles[i].wheelColliders.Length; j++){
				WheelCollider wc = axles[i].wheelColliders[j];
				Instantiate(wheelPrefab, wc.transform);
			}
			if(axles[i].powered) numberOfPoweredWheels += axles[i].wheelColliders.Length;
		}
	}
	
	void FixedUpdate () {
		Vector3 projectedVelocity = Vector3.Project(rb.velocity, rb.transform.forward);
		float currentSpeed = projectedVelocity.magnitude * Mathf.Sign(Vector3.Dot(projectedVelocity, rb.transform.forward));

		UpdateSteer(GetSteerInput(), Time.fixedDeltaTime, ref steer);
		float steerCurveInput = Mathf.Clamp01(Mathf.Abs(currentSpeed) / forwardSpeedLimit);
		float steerCurveEval = steerCurve.Evaluate(steerCurveInput);
		float currentTurnRadius = turnRadius / (steer * steerCurveEval);
		Vector3 turnCenter = steerPivot + (Vector3.right * currentTurnRadius);

		float gasInput = GetGasInput();
		float speedLimit = ((gasInput > 0f) ? forwardSpeedLimit : reverseSpeedLimit);
		float speedTarget = gasInput * speedLimit;
		bool braking = GetIsBraking(speedTarget, currentSpeed);
		float torqueCurveInput = Mathf.Clamp01(Mathf.Abs(currentSpeed / speedLimit));
		float torqueCurveEval = torqueCurve.Evaluate(torqueCurveInput);
		float motorTorque = maxTorque * torqueCurveEval * gasInput;

		bool useSimpleFractionalTorque = (Mathf.Abs(currentTurnRadius) > 100f);
		float totalDistanceSum = 0f;

		//important for differential steering
		if(!useSimpleFractionalTorque){
			for(int i=0; i<axles.Length; i++){
				if(axles[i].powered){
					for(int j=0; j<axles[i].wheelColliders.Length; j++){
						WheelCollider wc = axles[i].wheelColliders[j];
						Vector3 localPos = rb.transform.InverseTransformPoint(wc.transform.position);
						totalDistanceSum += (localPos - turnCenter).magnitude;
					}
				}
			}
		}

		for(int i=0; i<axles.Length; i++){
			for(int j=0; j<axles[i].wheelColliders.Length; j++){
				WheelCollider wc = axles[i].wheelColliders[j];
				if(braking){
					wc.motorTorque = 0f;
					wc.brakeTorque = maxBrakeTorque;
				}else{
					if(axles[i].powered){
						float factor;
						if(useSimpleFractionalTorque){
							factor = 1f / numberOfPoweredWheels;
						}else{
							Vector3 localPos = rb.transform.InverseTransformPoint(wc.transform.position);
							float dist = (turnCenter - localPos).magnitude;
							factor = dist / totalDistanceSum;
						}
						wc.motorTorque = motorTorque * factor;
						wc.brakeTorque = 0f;
					}else{
						wc.motorTorque = 0f;
						wc.brakeTorque = 0f;
					}
				}
				if(axles[i].steering){
					Vector3 localPos = rb.transform.InverseTransformPoint(wc.transform.position);
					float deltaZ = (localPos - steerPivot).z;
					float r = currentTurnRadius - localPos.x;
					wc.steerAngle = Mathf.Rad2Deg * Mathf.Atan(deltaZ / r);
				}

				MatchChildrenToCollider(wc);
			}
		}
	}

	void ApplyCustomCenterOfMass (Rigidbody rb, Transform newCenterOfMass) {
		if(newCenterOfMass != null){
			Vector3 comLocalPos = rb.transform.InverseTransformPoint(newCenterOfMass.position);
			if(Mathf.Abs(comLocalPos.x) > EPSILON){
				throw new UnityException("Center of mass must not be to the left or right of the rigidbody");
			}else{
				rb.centerOfMass = comLocalPos;
			}
		}
	}

	void CheckValidityOfAxles (Axle[] axles, Rigidbody mainRB) {
		bool valid = true;
		for(int i=0; i<axles.Length; i++){
			valid &= axles[i].IsValid(mainRB);
		}
		if(!valid){
			for(int i=0; i<axles.Length; i++){
				Vector3 lastPos = axles[i].wheelColliders[0].transform.position;
				for(int j=0; j<axles[i].wheelColliders.Length; j++){
					WheelCollider wc = axles[i].wheelColliders[j];
					Color drawColor = (axles[i].IsValid(mainRB) ? Color.green : Color.red);
					Debug.DrawRay(wc.transform.position, Vector3.down, drawColor, Mathf.Infinity, false);
					Debug.DrawLine(wc.transform.position, lastPos, drawColor, Mathf.Infinity, false);
					lastPos = wc.transform.position;
				}
			}
			throw new UnityException("Not all axles are valid. See the red rays to find out which...");
		}
	}

	Vector3 GetPivot (Axle[] axles, Rigidbody mainRB) {
		int numberOfStiffAxles = 0;
		int numberOfFreeAxles = 0;
		Vector3 stiffAverage = Vector3.zero;
		Vector3 freeAverage = Vector3.zero;
		for(int i=0; i<axles.Length; i++){
			if(axles[i].steering){
				numberOfFreeAxles++;
				freeAverage += mainRB.transform.InverseTransformPoint(axles[i].wheelColliders[0].transform.position);
			}else{
				numberOfStiffAxles++;
				stiffAverage += mainRB.transform.InverseTransformPoint(axles[i].wheelColliders[0].transform.position);
			}
		}
		if(numberOfStiffAxles > 0){
			return stiffAverage / numberOfStiffAxles;
		}else{
			return freeAverage / numberOfFreeAxles;
		}
	}

	//TODO externalize and stuff
	float GetSteerInput () {
		float steerInput = 0f;
		if(Input.GetKey(KeyCode.A)) steerInput -= 1f;
		if(Input.GetKey(KeyCode.D)) steerInput += 1f;
		return steerInput;
	}

	float GetGasInput () {
		float gasInput = 0f;
		if(Input.GetKey(KeyCode.W)) gasInput += 1f;
		if(Input.GetKey(KeyCode.S)) gasInput -= 1f;
		return gasInput;
	}

	bool GetIsBraking (float speedTarget, float currentSpeed) {
		if(Mathf.Abs(currentSpeed) < 0.01f) return false;	//HACK but hey it works :P
		else{
			float deltaSpeed = speedTarget - currentSpeed;
			return (Mathf.Sign(currentSpeed) != Mathf.Sign(deltaSpeed));
		}
	}

	void UpdateSteer (float steerTarget, float deltaTime, ref float steer) {
		float deltaSteer = steerTarget - steer;
		if(Mathf.Abs(deltaSteer / deltaTime) > steerPerSecond){
			deltaSteer = Mathf.Sign(deltaSteer) * steerPerSecond;
		}else{
			deltaSteer /= deltaTime;
		}
		steer += deltaSteer * deltaTime;
	}

	void MatchChildrenToCollider (WheelCollider wheelCollider) {
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose(out pos, out rot);
		for(int k=0; k<wheelCollider.transform.childCount; k++){
			Transform child = wheelCollider.transform.GetChild(k);
			child.position = pos;
			child.rotation = rot;
		}
	}
	
}
