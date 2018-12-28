using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class LightEvaluator : MonoBehaviour {

		[Header("Scene References")]
	[SerializeField] LightSwitch lightSwitch;

		[Header("Components")]
	[SerializeField] GameObject camParent;
	[SerializeField] GameObject cam;
	[SerializeField] MeshRenderer mainMeshRenderer;

		[Header("UI")]
	[SerializeField] Image directLightImage;
	[SerializeField] Image lightProbeImage;
	[SerializeField] Image visibilityImage;

		[Header("Settings")]
	[SerializeField] float speedRegular;
	[SerializeField] float speedFast;
	[SerializeField] float speedSlow;
	[SerializeField] float mouseSensitivity;
	[SerializeField] float zoomSensitivity;

		[Header("Keybinds")]
	[SerializeField] KeyCode key_fwd;
	[SerializeField] KeyCode key_bwd;
	[SerializeField] KeyCode key_left;
	[SerializeField] KeyCode key_right;
	[SerializeField] KeyCode key_up;
	[SerializeField] KeyCode key_down;
	[SerializeField] KeyCode key_slow;
	[SerializeField] KeyCode key_fast;
	[SerializeField] KeyCode key_zoom;

	Light[] realtimeLights;
	Collider[] allCollidersOnThisGameObject;

	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		realtimeLights = lightSwitch.GetRealtimeLights();
		allCollidersOnThisGameObject = gameObject.GetComponentsInChildren<Collider>();
	}

	void Update () {
		if(Cursor.lockState == CursorLockMode.Locked){
			NormalOperation();
		}else{
			CheckForKlick();
		}
		LightGUI();
	}

	void LightGUI () {
		float directLight = GetTotalDirectLight();
		directLightImage.transform.localScale = new Vector3(directLight, 1f, 1f);
		float lightProbeLight = GetTotalLightProbeLight();
		lightProbeImage.transform.localScale = new Vector3(lightProbeLight, 1f, 1f);
		float visibility = Mathf.Pow(Mathf.Clamp01(directLight + lightProbeLight), 0.5f);	//there's a square root (pow(x, 0.5)) here, which makes sense when you see it in action...
		visibilityImage.transform.localScale = new Vector3(visibility, 1f, 1f);
	}

	float GetTotalLightProbeLight () {
		SphericalHarmonicsL2 probe;
		LightProbes.GetInterpolatedProbe(transform.position, mainMeshRenderer, out probe);
		Vector3[] directions = GetLightProbeDirections();
		Color[] colors = new Color[directions.Length];
		probe.Evaluate(directions, colors);
		float lightSum = 0f;
		for(int i=0; i<colors.Length; i++){
			lightSum += ColorToLuminance(colors[i]);
		}
		lightSum /= colors.Length;
		return lightSum;
	}

	Vector3[] GetLightProbeDirections () {
		return new Vector3[]{
			new Vector3(1f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 0f, -1f)
		};
	}

	float GetTotalDirectLight () {
		//cache enabled state of all colliders and disable them so they dont get hit by raycasts
		bool[] colliderEnabledState = new bool[allCollidersOnThisGameObject.Length];
		for(int i=0; i<allCollidersOnThisGameObject.Length; i++){
			colliderEnabledState[i] = allCollidersOnThisGameObject[i].enabled;
			allCollidersOnThisGameObject[i].enabled = false;
		}
		//calculate lighting
		float lightSum = 0f;
		for(int i=0; i<realtimeLights.Length; i++){
			lightSum += GetDirectLight(realtimeLights[i]);
		}
		//re-enable colliders to what they were before
		for(int i=0; i<allCollidersOnThisGameObject.Length; i++){
			allCollidersOnThisGameObject[i].enabled = colliderEnabledState[i];
		}
		return lightSum;
	}

	float GetDirectLight (Light l) {
		if(!(l.enabled && l.gameObject.activeSelf && l.gameObject.activeInHierarchy)){
			return 0f;
		}
		Vector3 deltaPos;
		float distance;
		float overallFactor;
		if(l.type == LightType.Directional){
			deltaPos = -1f * l.transform.forward;
			distance = Mathf.Infinity;
			overallFactor = 1f;
		}else{
			deltaPos = l.transform.position - this.transform.position;
			distance = deltaPos.magnitude;
			overallFactor = Mathf.Clamp01(1f - (distance / l.range));
			if(l.type == LightType.Spot){
				float dot = Vector3.Dot(-deltaPos.normalized, l.transform.forward);
				float angle = Mathf.Rad2Deg * Mathf.Acos(dot);
				overallFactor *= SpotlightAngleAtten(l, angle);
			}
		}
		if(overallFactor <= 0f){
			return 0f;
		}
		overallFactor *= FuzzyVisibility(l, 0.25f);
		return l.intensity * ColorToLuminance(l.color) * overallFactor;
	}

	float SpotlightAngleAtten (Light l, float angleToLight) {
		float x = angleToLight * 2f;
		float fullBrightFraction = 0.67f;
		float t = l.spotAngle;
		float s = t * fullBrightFraction;
		float m = -1f / (t - s);
		float n = t / (t - s);
		float f = (m * x) + n;
		return Mathf.Clamp01(f);
	}

	float FuzzyVisibility (Light l, float testRadius = 1f) {
		bool fixedDirection = (l.type == LightType.Directional);
		float visibility = 0f;
		Vector3[] offsets = GetLightProbeDirections();
		for(int i=0; i<offsets.Length; i++){
			Vector3 rayOrigin = transform.position + (offsets[i] * testRadius);
			Vector3 rayDirection;
			float rayLength;
			if(fixedDirection){
				rayDirection = -1f * l.transform.forward;
				rayLength = Mathf.Infinity;
			}else{
				rayDirection = l.transform.position - rayOrigin;
				rayLength = rayDirection.magnitude;
			}
			if(!Physics.Raycast(rayOrigin, rayDirection, rayLength)){
				Debug.DrawRay(rayOrigin, rayDirection, Color.green, 0f, false);
				visibility += 1f;
			}else{
				Debug.DrawRay(rayOrigin, rayDirection, Color.red, 0f, false);
			}
		}
		visibility /= offsets.Length;
		return visibility;
	}

	float ColorToLuminance (Color col) {
		return 0.299f * col.r + 0.587f * col.g + 0.114f * col.b;
	}

	void NormalOperation () {
		if(Input.GetKey(key_zoom)){
			Zoom();
		}else{
			MouseLook();
			Movement();
		}
	}

	void CheckForKlick () {
		if(Input.GetKeyDown(KeyCode.Mouse0)){
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	void Zoom () {
		float mouseY = Input.GetAxisRaw("Mouse Y") * zoomSensitivity;
		float oldZ = cam.transform.localPosition.z;
		float newZ = oldZ * (1f + mouseY);
		cam.transform.localPosition = new Vector3(0f, 0f, newZ);
	}

	void Movement () {
		Vector3 moveVec = Vector3.zero;
		if(Input.GetKey(key_fwd)) moveVec += Vector3.forward;
		if(Input.GetKey(key_bwd)) moveVec += Vector3.back;
		if(Input.GetKey(key_left)) moveVec += Vector3.left;
		if(Input.GetKey(key_right)) moveVec += Vector3.right;
		if(Input.GetKey(key_up)) moveVec += Vector3.up;
		if(Input.GetKey(key_down)) moveVec += Vector3.down;
		moveVec = transform.TransformDirection(moveVec).normalized;
		float speed = speedRegular;
		if(Input.GetKey(key_fast)) speed = speedFast;
		if(Input.GetKey(key_slow)) speed = speedSlow;
		transform.position += moveVec * speed * Time.deltaTime;
	}

	void MouseLook () {
		float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
		transform.Rotate(new Vector3(0f, mouseX, 0f));
		camParent.transform.Rotate(new Vector3(-mouseY, 0f, 0f));
	}

}
