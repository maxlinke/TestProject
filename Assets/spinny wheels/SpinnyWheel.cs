using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinnyWheel : MonoBehaviour {

	[Header("Components")]
	[SerializeField] RectTransform wheelRectTransform;
	[SerializeField] Transform sliceParent;
	[SerializeField] Transform dividerParent;
	[SerializeField] Transform iconParent;
	[SerializeField] Image indicatorBackground;
	[SerializeField] Image indicatorImage;
	[SerializeField] Text indicatorText;

	[Header("Prefabs")]
	[SerializeField] GameObject slicePrefab;
	[SerializeField] GameObject dividerPrefab;
	[SerializeField] GameObject spriteIconPrefab;
	[SerializeField] GameObject textIconPrefab;

	[Header("Spin Settings")]
	[SerializeField] float spinStartupTime;
	[SerializeField] float minRPM;
	[SerializeField] float maxRPM;
	[SerializeField] float minStopTime;
	[SerializeField] float maxStopTime;
	[SerializeField] AnimationCurve stopBounceCurve;

	[Header("Visual Settings")]
	[SerializeField] Sprite[] sliceSprites;
	[SerializeField] Sprite[] dividerSprites;
	[SerializeField] Material sliceMaterial;
	[SerializeField] Material dividerMaterial;
	[SerializeField, Range(0f, 1f)] float normalIconScale;
	[SerializeField, Range(0f, 1f)] float normalIconPosition;
	[SerializeField, Range(0f, 1f)] float smallIconScale;
	[SerializeField, Range(0f, 1f)] float smallIconPosition;

	[Header("Debug")]
	[SerializeField] SpinnyWheelConfig debugData;

	RectTransform[] iconRectTransforms;
	SpinnyWheelConfig data;
	SpinnyWheelSlice[] slices;
	Coroutine spinCoroutine;
	float spinRPMLerpFactor;
	float spinRPM;

	public bool IsSpinning { get; private set; }
	public RectTransform RectTransform { get; private set; }
	public RectTransform WheelRectTransform { get { return wheelRectTransform; } }

	void Awake () {
		IsSpinning = false;
		RectTransform = (RectTransform)transform;
		iconRectTransforms = new RectTransform[0];
	}

	void LateUpdate () {
		for(int i=0; i<iconRectTransforms.Length; i++){
			iconRectTransforms[i].localEulerAngles = -wheelRectTransform.localEulerAngles;
		}
		SpinnyWheelSlice topSlice = GetSliceOnTop();
		SpinnyWheelEffect topEffect = topSlice.Effect;
		indicatorBackground.color = topSlice.color;
		indicatorBackground.material = (topEffect.OverrideBackgroundMaterial ? topEffect.BackgroundMaterial : null);
		if(topEffect.Type.Equals(SpinnyWheelEffect.EffectType.VALUE)){
			indicatorImage.SetGOActive(false);
			indicatorText.SetGOActive(true);
			indicatorText.text = topEffect.Value.ToString();
			indicatorText.color = topEffect.ElementColor;
		}else{
			indicatorText.SetGOActive(false);
			indicatorImage.SetGOActive(true);
			indicatorImage.sprite = topEffect.Sprite;
			indicatorImage.color = topEffect.ElementColor;
		}
	}

	[ContextMenu("Initialize with debug data")]
	void DebugInit () {
		Initialize(debugData);
	}

	public SpinnyWheelSlice GetSliceOnTop () {
		SpinnyWheelSlice output = null;
		if(data != null){
			float angle = Mathf.Repeat(wheelRectTransform.localEulerAngles.z, 360f);
			int segmentIndex = Mathf.FloorToInt((angle / 360f) * SpinnyWheelConfig.SEGMENTS_PER_WHEEL);
			int segmentCounter = 0;
			for(int i=0; i<slices.Length; i++){
				int startSegment = segmentCounter;
				int endSegment = segmentCounter + slices[i].NumberOfSegments - 1;
				if((segmentIndex >= startSegment) && (segmentIndex <= endSegment)){
					output = slices[i];
					break;
				}
				segmentCounter += slices[i].NumberOfSegments;
			}
		}
		return output;
	}

	public void Initialize (SpinnyWheelConfig inputData) {
		if(inputData == null){
			DestroySpawnedObjects();
		}else if(!inputData.IsValid()){
			Debug.LogError("Error, data is not valid.");
		}else{
			this.data = inputData;
			this.slices = inputData.Slices;
			DestroySpawnedObjects();
			SpawnVisualParts();
		}
	}

	public void StartSpinning (float delay) {
		spinCoroutine.Stop(this);
		spinRPMLerpFactor = Random.value;
		spinRPM = Mathf.Lerp(minRPM, maxRPM, spinRPMLerpFactor);
		spinCoroutine = StartCoroutine(SpinCoroutine(delay, spinStartupTime, spinRPM));
	}

	public void StopSpinning () {
		spinCoroutine.Stop(this);
		float stopTime = Mathf.Lerp(minStopTime, maxStopTime, spinRPMLerpFactor);
		spinCoroutine = StartCoroutine(SpinStopCoroutine(stopTime, spinRPM));
	}

	void DestroySpawnedObjects () {
		for(int i=sliceParent.childCount-1; i>=0; i--){
			DestroyObject(sliceParent.GetChild(i).gameObject);
		}
		for(int i=dividerParent.childCount-1; i>=0; i--){
			DestroyObject(dividerParent.GetChild(i).gameObject);
		}
		for(int i=iconParent.childCount-1; i>=0; i--){
			DestroyObject(iconParent.GetChild(i).gameObject);
		}
	}

	void DestroyObject (GameObject toDestroy) {
		if(Application.isPlaying) Destroy(toDestroy);
		else DestroyImmediate(toDestroy, false);
	}

	void SpawnVisualParts () {
		int definedMaxSegmentCount = SpinnyWheelConfig.SEGMENTS_PER_WHEEL;
		int numberOfSlices = slices.Length;
		iconRectTransforms = new RectTransform[numberOfSlices];
		if(numberOfSlices > 1){
			int segmentsPassed = 0;
			for(int i=0; i<numberOfSlices; i++){
				if(segmentsPassed >= definedMaxSegmentCount){
					break;
				}
				SpinnyWheelSlice slice = slices[i];
				float segFrac = ((float)segmentsPassed) / definedMaxSegmentCount;
				float segEndFrag = ((float)(segmentsPassed + slice.NumberOfSegments) / definedMaxSegmentCount);
				float spawnAngle = (-segFrac * 360f);

				SpawnNewSliceBackground(slice, spawnAngle);
				SpawnNewDivider(spawnAngle);

				float iconSegFrag = (segFrac + segEndFrag) * 0.5f;
				float iconSpawnAngle = (-iconSegFrag * 360f);
				RectTransform newIconRectTransform = SpawnNewEffectIconAndGetRectTransform(slice, iconSpawnAngle);

				iconRectTransforms[i] = newIconRectTransform;
				segmentsPassed += slice.NumberOfSegments;
			}
		}else if(numberOfSlices == 1){
			SpawnNewSliceBackground(slices[0], 0f);
			SpawnNewEffectIconAndGetRectTransform(slices[0], 0f);
		}else{
			//there are no slices, don't do anything...
		}
	}

	void SpawnNewSliceBackground (SpinnyWheelSlice slice, float angle) {
		GameObject newBackground = Instantiate(slicePrefab, sliceParent);
		RectTransform newBackgroundRectTransform = (RectTransform)newBackground.transform;
		newBackgroundRectTransform.anchoredPosition = Vector3.zero;
		newBackgroundRectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
		Image imageComponent = newBackground.GetComponent<Image>();
		imageComponent.sprite = sliceSprites[slice.NumberOfSegments-1];
		Color backgroundColor = Color.Lerp(data.DesaturatedBackgroundColor, data.SaturatedBackgroundColor, slice.Effect.BackgroundSaturation);
		backgroundColor = Color.Lerp(Color.black, backgroundColor, slice.Effect.BackgroundBrightness);
		imageComponent.color = backgroundColor;
		slice.color = backgroundColor;
		if(slice.Effect.OverrideBackgroundMaterial){
			imageComponent.material = slice.Effect.BackgroundMaterial;
		}else{
			imageComponent.material = sliceMaterial;
		}
	}

	void SpawnNewDivider (float angle) {
		GameObject newDivider = Instantiate(dividerPrefab, dividerParent);
		RectTransform newDividerRectTransform = (RectTransform)newDivider.transform;
		newDividerRectTransform.anchoredPosition = Vector2.zero;
		newDividerRectTransform.localEulerAngles= new Vector3(0f, 0f, angle);
		Image imageComponent = newDivider.GetComponent<Image>();
		imageComponent.sprite = RandomFromArray(dividerSprites);
		imageComponent.material = dividerMaterial;
	}

	RectTransform SpawnNewEffectIconAndGetRectTransform (SpinnyWheelSlice slice, float angle) {
		GameObject newIcon = Instantiate((slice.Effect.UseSprite ? spriteIconPrefab : textIconPrefab), iconParent);
		RectTransform newIconRectTransform = (RectTransform)newIcon.transform;
		bool small = (slice.NumberOfSegments == 1);
		float offset = (wheelRectTransform.rect.width / 2f) * (small ? smallIconPosition : normalIconPosition);
		float radAngle = Mathf.Deg2Rad * angle * -1f;
		newIconRectTransform.anchoredPosition = new Vector2(Mathf.Sin(radAngle), Mathf.Cos(radAngle)) * offset;
		newIconRectTransform.localScale = Vector3.one * (small ? smallIconScale : normalIconScale);
		if(slice.Effect.UseSprite){
			Image imageComponent = newIcon.GetComponent<Image>();
			imageComponent.sprite = slice.Effect.Sprite;
			imageComponent.color = slice.Effect.ElementColor;
		}else{
			Text textComponent = newIcon.GetComponent<Text>();
			textComponent.text = slice.Effect.Text;
			textComponent.color = slice.Effect.ElementColor;
		}
		return newIconRectTransform;
	}

	void RotateWithRPM (float rpm) {
		wheelRectTransform.localEulerAngles += new Vector3(0f, 0f, Time.deltaTime * 360f * rpm / 60f);
	}

	T RandomFromArray<T> (T[] array) {
		return array[Random.Range(0, array.Length)];
	}

	IEnumerator SpinCoroutine (float startupDelay, float startupTime, float targetRPM) {
		yield return new WaitForSeconds(startupDelay);
		IsSpinning = true;
		float startTime = Time.time;
		float t = 0f;
		while(t<1f){
			RotateWithRPM(Mathf.Lerp(0f, targetRPM, t));
			t = (Time.time - startTime) / startupTime;
			yield return null;
		}
		while(true){
			RotateWithRPM(targetRPM);
			yield return null;
		}
	}

	IEnumerator SpinStopCoroutine (float duration, float startRPM) {
		Vector3 startEuler = wheelRectTransform.localEulerAngles;
		Vector3 maxBounceEuler = startEuler - new Vector3(0f, 0f, startRPM * duration);
		float startTime = Time.time;
		float t = 0f;
		while(t<1f){
			float eval = stopBounceCurve.Evaluate(t);
			wheelRectTransform.localEulerAngles = Vector3.Lerp(startEuler, maxBounceEuler, eval);
			t = (Time.time - startTime) / duration;
			yield return null;
		}
		wheelRectTransform.localEulerAngles = startEuler;
		IsSpinning = false;
	}

}
