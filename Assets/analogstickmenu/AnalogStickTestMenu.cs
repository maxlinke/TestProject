using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalogStickTestMenu : MonoBehaviour {

	private struct Range {

		public float bound1, bound2;

		public float span {
			get {
				return Mathf.Abs(bound1 - bound2);
			}
		}

		public float this[int index] {
			get {
				if(index == 0) return bound1;
				if(index == 1) return bound2;
				throw new UnityException("Invalid Index \"" + index.ToString() + "\", only 0 or 1 allowed!");
			} set {
				if(index == 0){bound1 = value; return;}
				if(index == 1){bound2 = value; return;}
				throw new UnityException("Invalid Index \"" + index.ToString() + "\", only 0 or 1 allowed!");
			}
		}

		public Range (float bound1, float bound2) {
			this.bound1 = bound1;
			this.bound2 = bound2;
		}

		public bool Contains (float value) {
			float maxDelta = Mathf.Max(Mathf.Abs(bound1 - value), Mathf.Abs(bound2 - value));
			return (maxDelta <= this.span); 
		}

		public override string ToString () {
			float min = Mathf.Min(bound1, bound2);
			float max = Mathf.Max(bound1, bound2);
			return string.Format ("[{0:F2}, {1:F2}]", min, max);
		}

	}

	[Header("Components")]
	[SerializeField] GameObject someSelectablePrefab;
	[SerializeField] GameObject headerContentContainer;
	[SerializeField] GameObject decoObjectContainer;
	[SerializeField] GameObject selectablesParent;

	[Header("Placement Settings")]
	[SerializeField, Range(0, 5)] int numberOfObjects;
	[SerializeField, Range(0f, 50f)] float horizontalObjectOffset;
	[SerializeField] float circleRadius;

	[Header("Selection Settings")]
	[SerializeField, Range(0f, 1f)] float stickDeadzone;
	[SerializeField, Range(0f, 1f)] float selectionTightness;
	[SerializeField, Range(0f, 1f)] float fractionalThresholdBetweenSelections;
	[SerializeField, Range(0f, 360f)] float maxThresholdAngle;

	[Header("DEBUG")]
	[SerializeField] GameObject deadzoneDrawerOrigin;

	SomeSelectableObject currentlySelectedObject;
	SomeSelectableObject[] selectables;
	Range[] selectableRanges;
	Range[] inbetweenDeadzones;

	bool stickWasInDeadzone;

	void Start () {
		RegenerateSelectables(ref selectables, selectablesParent);
		UpdateRanges(ref selectableRanges, ref inbetweenDeadzones, selectables);
	}
	
	void Update () {
		Vector2 joyInput = new Vector2(
			Input.GetAxisRaw("LX"),
			Input.GetAxisRaw("LY")
		);
		bool stickIsInDeadZone = (joyInput.magnitude < stickDeadzone);
		Debug.DrawRay(
			decoObjectContainer.transform.position,
			joyInput * 40f,
			(stickIsInDeadZone ? Color.green : Color.red), 
			0f,
			false
		);
		if(stickIsInDeadZone){
			if(!stickWasInDeadzone){
				SetSelectedObject(null);
			}
		}else{
			float joyOri = Vec2Ori(joyInput);
			int selectionIndex = -1;	//init with -1 is unclean but necessary...
			bool selectedSomething = false;
			for(int i=0; i<selectableRanges.Length; i++){
				if(selectableRanges[i].Contains(joyOri) || selectableRanges[i].Contains(-joyOri)){		//mirroring the joystick along the vertical...
					selectionIndex = i;
					selectedSomething = true;
					break;
				}
			}
			if(!selectedSomething){
				Debug.LogWarning("no selection was made although there should have been one!!!");
			}else{
				if(currentlySelectedObject == null){
					SetSelectedObject(selectables[selectionIndex]);
				}else{
					bool selectionIsInDeadzone = false;
					if(selectionIndex > 0){		//check "left" deadzone
						selectionIsInDeadzone |= (inbetweenDeadzones[selectionIndex - 1].Contains(joyOri) || inbetweenDeadzones[selectionIndex - 1].Contains(-joyOri));
					}
					if(selectionIndex < inbetweenDeadzones.Length){
						selectionIsInDeadzone |= (inbetweenDeadzones[selectionIndex].Contains(joyOri) || inbetweenDeadzones[selectionIndex].Contains(-joyOri));
					}
					if(!selectionIsInDeadzone){
						SetSelectedObject(selectables[selectionIndex]);
					}
				}
			}
			DrawDeadzones(joyOri);
		}
		stickWasInDeadzone = stickIsInDeadZone;
	}

	void DrawDeadzones (float joyOri) {
		joyOri = -Mathf.Abs(joyOri);
		float scale = 30f;
		Vector3 start = deadzoneDrawerOrigin.transform.position;
		Vector3 right = deadzoneDrawerOrigin.transform.right;
		Vector3 up = deadzoneDrawerOrigin.transform.up;
		Debug.DrawLine(start + (scale * (-Mathf.PI - 0.1f) * right), start + (scale * 0.1f * right), Color.green);
		Debug.DrawRay(start, up * 0.1f * scale, Color.green);
		Debug.DrawRay(start + (scale * right * -Mathf.PI), up * 0.1f * scale, Color.green);
		Vector3 p = start + (right * scale * joyOri);
		Debug.DrawLine(p - (up * scale * 0.2f), p + (up * scale * 0.2f), Color.white);
		for(int i=0; i<selectableRanges.Length; i++){
			for(int j=0; j<2; j++){
				p = start + (scale * right * selectableRanges[i][j]);
				Debug.DrawRay(p, up * scale * -0.05f, Color.green);
			}
		}
		for(int i=0; i<inbetweenDeadzones.Length; i++){
			Vector3 l = start + (scale * right * inbetweenDeadzones[i].bound1) - (up * scale * 0.05f);
			Vector3 r = start + (scale * right * inbetweenDeadzones[i].bound2) - (up * scale * 0.05f);
			Debug.DrawLine(l, r, Color.red);
		}
	}

	[ContextMenu("Regenerate Selectables")]
	void ContextRegenerateSelectables () {
		RegenerateSelectables(ref selectables, selectablesParent);
		UpdateRanges(ref selectableRanges, ref inbetweenDeadzones, selectables);
	}

	public void HoverEnter (SomeSelectableObject obj) {
		SetSelectedObject(obj);
	}

	public void HoverExit (SomeSelectableObject obj) {
//		obj.selected = false;
	}

	void SetSelectedObject (SomeSelectableObject toSelect) {
		for(int i=0; i<selectables.Length; i++){
			selectables[i].selected = selectables[i].Equals(toSelect);
		}
		currentlySelectedObject = toSelect;
	}

	void RegenerateSelectables (ref SomeSelectableObject[] selectables, GameObject selectablesParent) {
		if(selectables != null){
			for(int i=0; i<selectables.Length; i++){
				Destroy(selectables[i].gameObject);
			}
		}
		selectables = new SomeSelectableObject[numberOfObjects];
		float halfDelta = (selectables.Length - 1f) / 2f;
		for(int i=0; i<selectables.Length; i++){
			SomeSelectableObject newObj = Instantiate(someSelectablePrefab, selectablesParent.transform).GetComponent<SomeSelectableObject>();
			float offsetMultiplier = i - halfDelta;
			newObj.rectTransform.anchoredPosition = new Vector3(offsetMultiplier * (horizontalObjectOffset + newObj.rectTransform.rect.width), 0f, 0f);
			newObj.Initialize(this, i);
			selectables[i] = newObj;
		}
		PositionOnCircle(ref selectables, circleRadius);
	}

	void UpdateRanges (ref Range[] selectableRanges, ref Range[] inbetweenDeadzones, SomeSelectableObject[] selectables) {
		selectableRanges = new Range[selectables.Length];
		inbetweenDeadzones = new Range[selectables.Length - 1];
		float[] borders = new float[selectables.Length - 1];
		float midpoint = -0.5f * Mathf.PI;
		for(int i=0; i<selectables.Length; i++){
			float lowerBound = (1f - (((float) i) / selectables.Length)) * Mathf.PI * -1f;
			float upperBound = (1f - (((float) i+1) / selectables.Length)) * Mathf.PI * -1f;
			selectableRanges[i] = new Range(lowerBound, upperBound);
			for(int j=0; j<2; j++){
				float value = selectableRanges[i][j];
				if((value < 0) && (value > -Mathf.PI)){
					selectableRanges[i][j] = Mathf.Lerp(value, midpoint, selectionTightness);
				}
			}
			if(i>0){
				borders[i-1] = Mathf.Min(selectableRanges[i].bound1, selectableRanges[i].bound2);
			}
		}
		float rangeSpan = selectableRanges[0].span;
		float threshold = Mathf.Max(fractionalThresholdBetweenSelections * rangeSpan, Mathf.Deg2Rad * maxThresholdAngle);
		for(int i=0; i<inbetweenDeadzones.Length; i++){
			float lowerBound = borders[i] - (threshold / 2f);
			float upperBound = borders[i] + (threshold / 2f);
			inbetweenDeadzones[i] = new Range(lowerBound, upperBound);
		}
	}

	void PositionOnCircle (ref SomeSelectableObject[] selectables, float circleRadius) {
		for(int i=0; i<selectables.Length; i++){
//			float radiusFraction = Mathf.Abs(-selectables[i].rectTransform.anchoredPosition.x / circleRadius);
			float radiusFraction = selectables[i].rectTransform.anchoredPosition.x / circleRadius;
			float verticalOffset = circleRadius * (Mathf.Sqrt(1f - (radiusFraction * radiusFraction)) - 1f);
			Vector2 newPos = new Vector2(
				selectables[i].rectTransform.anchoredPosition.x,
				-verticalOffset
			);
			Vector3 newEuler = new Vector3(
				0f,
				0f,
				Mathf.Asin(radiusFraction) * Mathf.Rad2Deg
			);
			selectables[i].rectTransform.anchoredPosition = newPos;
			selectables[i].rectTransform.localEulerAngles = newEuler;
		}
	}

	float Vec2Ori (Vector2 vec) {
		vec = vec.normalized;
		return Mathf.Atan2(vec.y, vec.x);
	}

	Vector2 Ori2Vec (float ori) {
		return new Vector2(
			Mathf.Cos(ori),
			Mathf.Sin(ori)
		);
	}

	float DeltaOri (float src, float dst) {
		src = Mathf.Repeat(src + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;
		dst = Mathf.Repeat(dst + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;
		float delta = dst - src;
		if(Mathf.Abs(delta) > Mathf.PI){
			delta -= 2f * Mathf.PI * Mathf.Sign(delta);
		}
		return delta;
	}

}
