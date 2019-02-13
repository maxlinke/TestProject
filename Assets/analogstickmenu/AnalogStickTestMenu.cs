using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnalogStickTestMenu : MonoBehaviour {

	private const int MIN_NUMBER_OF_OBJECTS = 2;
	private const int MAX_NUMBER_OF_OBJECTS = 5;

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
	[SerializeField] Button entryExitButton;
	[SerializeField] Button shuffleButton;
	[SerializeField] GameObject someSelectablePrefab;
	[SerializeField] RectTransform selectablesParent;
	[SerializeField] RectTransform decoObject;

	[Header("Placement Settings")]
	[SerializeField, Range(MIN_NUMBER_OF_OBJECTS, MAX_NUMBER_OF_OBJECTS)] int numberOfObjects;
	[SerializeField, Range(0f, 50f)] float horizontalObjectOffset;
	[SerializeField] float circleRadius;
	[SerializeField] RectTransform selectablesParentExtended;
	[SerializeField] RectTransform selectablesParentRetracted;

	[Header("Selection Settings")]
	[SerializeField, Range(0f, 1f)] float stickDeadzone;
	[SerializeField, Range(0f, 1f)] float selectionTightness;
	[SerializeField, Range(0f, 1f)] float fractionalThresholdBetweenSelections;
	[SerializeField, Range(0f, 360f)] float maxThresholdAngle;

	[Header("Animations")]
	[SerializeField] float entryDuration;
	[SerializeField] AnimationCurve parentEntryCurve;
	[SerializeField] AnimationCurve selectableEntryCurve;
	[SerializeField] float exitDuration;
	[SerializeField] AnimationCurve parentExitCurve;
	[SerializeField] AnimationCurve selectableExitCurve;
	[SerializeField] float shuffleDuration;

	[Header("DEBUG")]
	[SerializeField] GameObject DEBUG_deadzoneDrawerOrigin;
	[SerializeField] GameObject DEBUG_screenPositionTestObject;

	Canvas parentCanvas;

	SomeSelectableObject currentlySelectedObject;
	SomeSelectableObject[] selectables;
	RectTransform[] selectableRectTransforms;
	Range[] selectableRanges;
	Range[] inbetweenDeadzones;

	Vector3[] spreadSelectablePositions;
	Quaternion[] spreadSelectableRotations;
	Vector3[] crampedSelectablePositions;
	Quaternion[] crampedSelectableRotations;

	bool stickWasInDeadzone;

	Coroutine parentCoroutine;
	Coroutine selectableCoroutine;

	void Awake () {
		parentCanvas = GetComponentInParent<Canvas>();
		if(parentCanvas.renderMode == RenderMode.ScreenSpaceCamera){
			parentCanvas.planeDistance = parentCanvas.worldCamera.nearClipPlane + 0.01f;
		}
	}

	void Start () {
		RegenerateSelectables();
		UpdateRanges();
		SetParentRetracted();
		SetSelectablesCramped();
		SetButtonToEntry();
		shuffleButton.onClick.RemoveAllListeners();
		shuffleButton.onClick.AddListener( () => { StartCoroutine(Shuffle(shuffleDuration)); });
	}

	void Update () {
		PositionTesting();
		if(Input.GetKeyDown(KeyCode.R)){
			Start();
		}
		Vector2 joyInput = new Vector2(
			Input.GetAxisRaw("LX"),
			Input.GetAxisRaw("LY")
		);
		bool stickIsInDeadZone = (joyInput.magnitude < stickDeadzone);
		Debug.DrawRay(
			decoObject.position,
			joyInput * 40f,
			(stickIsInDeadZone ? Color.red : Color.green),
			0f,
			false
		);
		if(stickIsInDeadZone){
			if(!stickWasInDeadzone){
				SetSelectedObject(null);
			}
//			if(currentlySelectedObject != null){
				//TODO point in direction of mouse... yey... so project some ray on some plane and do some math yo
//				Rect pixelRect = RectTransformUtility.PixelAdjustRect(decoObject, parentCanvas);
//				Vector2 decoObjectScreenPos = new Vector2(pixelRect.x + (pixelRect.width / 2f), pixelRect.y + (pixelRect.height / 2f));
//				Vector2 decoObjectScreenPos = decoObject.TransformPoint(decoObject.rect.center);
//				Vector2 mouseScreenPos = Input.mousePosition;
//				Vector2 delta = mouseScreenPos - decoObjectScreenPos;
//				Debug.Log(decoObjectScreenPos);
//				decoObject.localRotation = Quaternion.Euler(0f, 0f, OriToAngle(Mathf.Atan2(delta.y, delta.x), 0.5f * Mathf.PI));
//			}else{
				decoObject.localRotation = Quaternion.identity;
//			}
		}else{
			float joyOri = Vec2Ori(joyInput);
			decoObject.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * ((0.5f * Mathf.PI) - joyOri));
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

	void PositionTesting () {
		Vector2 mouseScreenPos = Input.mousePosition;
		Vector3 objectPos = DEBUG_screenPositionTestObject.transform.TransformPoint(DEBUG_screenPositionTestObject.transform.position);
		Vector3 objectScreenPos;
		switch(parentCanvas.renderMode){
			case RenderMode.ScreenSpaceOverlay:
				objectScreenPos = objectPos;
				break;
			case RenderMode.ScreenSpaceCamera:
				objectScreenPos = parentCanvas.worldCamera.transform.InverseTransformPoint(objectPos);
				break;
			case RenderMode.WorldSpace:
				objectScreenPos = Vector3.zero;	//it's basically the same as screen space camera
				break;
			default:
				throw new UnityException("wat?");
		}
		Debug.Log(string.Format("({0:F3}, {1:F3}, {2:F3})", objectScreenPos.x, objectScreenPos.y, objectScreenPos.z));
	}

	void DrawDeadzones (float joyOri) {
		joyOri = -Mathf.Abs(joyOri);
		float scale = 30f;
		Vector3 start = DEBUG_deadzoneDrawerOrigin.transform.position;
		Vector3 right = DEBUG_deadzoneDrawerOrigin.transform.right;
		Vector3 up = DEBUG_deadzoneDrawerOrigin.transform.up;
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

	public void HoverEnter (SomeSelectableObject obj) {
		SetSelectedObject(obj);
	}

	public void HoverExit (SomeSelectableObject obj) {
		if(currentlySelectedObject.Equals(obj)){
			SetSelectedObject(null);
		}else{
			obj.selected = false;
		}
	}

	void SetSelectedObject (SomeSelectableObject toSelect) {
		for(int i=0; i<selectables.Length; i++){
			selectables[i].selected = selectables[i].Equals(toSelect);
		}
		currentlySelectedObject = toSelect;
	}

	void RegenerateSelectables () {
		for(int i=selectablesParent.childCount-1; i>=0; i--){
			Destroy(selectablesParent.GetChild(i).gameObject);
		}
		selectables = new SomeSelectableObject[numberOfObjects];
		selectableRectTransforms = new RectTransform[numberOfObjects];
		crampedSelectablePositions = new Vector3[numberOfObjects];
		crampedSelectableRotations = new Quaternion[numberOfObjects];
		for(int i=0; i<selectables.Length; i++){
			SomeSelectableObject newObj = Instantiate(someSelectablePrefab, selectablesParent.transform).GetComponent<SomeSelectableObject>();
			newObj.Initialize(this, i);
			newObj.rectTransform.anchoredPosition = Vector3.zero;
			newObj.rectTransform.localRotation = Quaternion.identity;
			selectables[i] = newObj;
			selectableRectTransforms[i] = newObj.rectTransform;
			crampedSelectablePositions[i] = newObj.rectTransform.anchoredPosition;
			crampedSelectableRotations[i] = newObj.rectTransform.localRotation;
		}
		GetLocalTransformDataOnCircle(selectableRectTransforms, out spreadSelectablePositions, out spreadSelectableRotations);
	}

	void UpdateRanges () {
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

	void GetLocalTransformDataOnCircle (RectTransform[] rectTransforms, out Vector3[] outputAnchoredPositions, out Quaternion[] outputLocalRotations) {
		outputAnchoredPositions = new Vector3[rectTransforms.Length];
		outputLocalRotations = new Quaternion[rectTransforms.Length];

		float halfDelta = (rectTransforms.Length - 1f) / 2f;
		for(int i=0; i<rectTransforms.Length; i++){
			float offsetMultiplier = i - halfDelta;
			float xPosition = offsetMultiplier * (horizontalObjectOffset + rectTransforms[i].rect.width);
			float radiusFraction = xPosition / circleRadius;
			float verticalOffset = circleRadius * (Mathf.Sqrt(1f - (radiusFraction * radiusFraction)) - 1f);
			Vector2 newPos = new Vector2(
				xPosition,
				-verticalOffset
			);
			Vector3 newEuler = new Vector3(
				0f,
				0f,
				Mathf.Asin(radiusFraction) * Mathf.Rad2Deg
			);
			outputAnchoredPositions[i] = newPos;
			outputLocalRotations[i] = Quaternion.Euler(newEuler);
		}
	}

	void SetLocalTransformData (RectTransform rectTransform, Vector3 position, Quaternion rotation) {
		rectTransform.anchoredPosition = position;
		rectTransform.localRotation = rotation;
	}

	void SetLocalTransformData (RectTransform[] rectTransforms, Vector3[] positions, Quaternion[] rotations) {
		for(int i=0; i<rectTransforms.Length; i++){
			rectTransforms[i].anchoredPosition = positions[i];
			rectTransforms[i].localRotation = rotations[i];
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

	//TODO entryanimation, exitanimation, shuffleanimation
	//TODO button to make menu enter/exit
	//TODO button to shuffle

	void SetButtonToEntry () {
		entryExitButton.onClick.RemoveAllListeners();
		entryExitButton.onClick.AddListener(() => {
			AnimateEntry();
			SetButtonToExit();
		});
	}

	void SetButtonToExit () {
		entryExitButton.onClick.RemoveAllListeners();
		entryExitButton.onClick.AddListener(() => {
			AnimateExit();
			SetButtonToEntry();
		});
	}

	void SetParentRetracted () {
		SetLocalTransformData(selectablesParent, selectablesParentRetracted.anchoredPosition, selectablesParentRetracted.localRotation);
	}

	void SetParentExtended () {
		SetLocalTransformData(selectablesParent, selectablesParentExtended.anchoredPosition, selectablesParentExtended.localRotation);
	}

	void SetSelectablesCramped () {
		SetLocalTransformData(selectableRectTransforms, crampedSelectablePositions, crampedSelectableRotations);
	}

	void SetSelectablesSpread () {
		SetLocalTransformData(selectableRectTransforms, spreadSelectablePositions, spreadSelectableRotations);
	}

	void AnimateEntry () {
		parentCoroutine.Stop(this);
		selectableCoroutine.Stop(this);
		parentCoroutine = StartCoroutine(MoveAndRotate(selectablesParent, selectablesParentExtended.anchoredPosition, selectablesParentExtended.localRotation, entryDuration, parentEntryCurve));
		selectableCoroutine = StartCoroutine(MoveAndRotate(selectableRectTransforms, spreadSelectablePositions, spreadSelectableRotations, entryDuration, selectableEntryCurve));
	}

	void AnimateExit () {
		parentCoroutine.Stop(this);
		selectableCoroutine.Stop(this);
		parentCoroutine = StartCoroutine(MoveAndRotate(selectablesParent, selectablesParentRetracted.anchoredPosition, selectablesParentRetracted.localRotation, exitDuration, parentExitCurve));
		selectableCoroutine = StartCoroutine(MoveAndRotate(selectableRectTransforms, crampedSelectablePositions, crampedSelectableRotations, exitDuration, selectableExitCurve));
	}

	IEnumerator MoveAndRotate (RectTransform rectTransform, Vector3 targetPosition, Quaternion targetLocalRotation, float duration, AnimationCurve animCurve = null) {
		return MoveAndRotate(new RectTransform[]{rectTransform}, new Vector3[]{targetPosition}, new Quaternion[]{targetLocalRotation}, duration, animCurve);
	}

	IEnumerator MoveAndRotate (RectTransform[] rectTransforms, Vector3[] targetAnchoredPositions, Quaternion[] targetLocalRotations, float duration, AnimationCurve animCurve = null) {
		Vector3[] startAnchoredPositions = new Vector3[rectTransforms.Length];
		Quaternion[] startLocalRotations = new Quaternion[rectTransforms.Length];
		for(int i=0; i<rectTransforms.Length; i++){
			startAnchoredPositions[i] = rectTransforms[i].anchoredPosition;
			startLocalRotations[i] = rectTransforms[i].localRotation;
		}
		if(duration > 0f){
			float startTime = Time.time;
			float t = 0f;
			while(t<1f){
				float eval = ((animCurve != null) ? animCurve.Evaluate(t) : t);
				for(int i=0; i<rectTransforms.Length; i++){
					rectTransforms[i].anchoredPosition = Vector3.LerpUnclamped(startAnchoredPositions[i], targetAnchoredPositions[i], eval);
					rectTransforms[i].localRotation = Quaternion.LerpUnclamped(startLocalRotations[i], targetLocalRotations[i], eval);
				}
				t = (Time.time - startTime) / duration;
				yield return null;
			}
		}
		for(int i=0; i<rectTransforms.Length; i++){
			rectTransforms[i].anchoredPosition = targetAnchoredPositions[i];
			rectTransforms[i].localRotation = targetLocalRotations[i];
		}
	}

	IEnumerator Shuffle (float duration) {
		bool entryExitButtonInteractable = entryExitButton.interactable;
		float halfDuration = duration * 0.5f;
		entryExitButton.interactable = false;
		selectableCoroutine.Stop(this);
		selectableCoroutine = StartCoroutine(MoveAndRotate(selectableRectTransforms, crampedSelectablePositions, crampedSelectableRotations, halfDuration));
		yield return new WaitForSeconds(halfDuration);
		numberOfObjects = Random.Range(MIN_NUMBER_OF_OBJECTS, MAX_NUMBER_OF_OBJECTS + 1);
		RegenerateSelectables();
		UpdateRanges();
		SetParentExtended();
		SetSelectablesCramped();
		selectableCoroutine.Stop(this);
		selectableCoroutine = StartCoroutine(MoveAndRotate(selectableRectTransforms, spreadSelectablePositions, spreadSelectableRotations, halfDuration));
		yield return new WaitForSeconds(halfDuration);
		entryExitButton.interactable = entryExitButtonInteractable;
	}

}
