using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinMenu : MonoBehaviour {

	[Header("Components")]
	[SerializeField] Transform wheelParent;
	[SerializeField] AffectionDisplayer affectionDisplayer;

	[Header("Prefabs")]
	[SerializeField] SpinnyWheel wheelPrefab;

	[Header("Data")]
	[SerializeField] SpinnyWheelConfig baseData;
	[SerializeField] List<SpinnyWheelConfig> additionalData;
	[SerializeField] int maxBaseData;
	[SerializeField] int maxOtherData;

	[Header("Keybinds")]
	[SerializeField] KeyCode keyStart;
	[SerializeField] KeyCode keyStop;
	[SerializeField] KeyCode keyRandomize;

	[Header("Settings")]
	[SerializeField] float horizontalOffset;
	[SerializeField] float minStartDelay;
	[SerializeField] float maxStartDelay;
	[SerializeField] bool randomizeRotationOnSpawn;
	[SerializeField] float durationUntilAutoStop;
	[SerializeField] bool timerScalesParent;
	[SerializeField] float minWheelParentScale;

	[Header("Debug")]
	[SerializeField] Image debug_visualTimer;
	[SerializeField] Text debug_typeText;
	[SerializeField] Text debug_resultText;
	[SerializeField] int debug_numberOfBaseData;
	[SerializeField] int debug_numberOfOtherData;
	[SerializeField] SpinnyWheelConfig debug_otherData;

	bool wheelsWereSpinning;
	List<SpinnyWheel> wheels;
	Coroutine timerAndAutoStopCoroutine;

	void Awake () {
		wheelsWereSpinning = false;
		wheels = new List<SpinnyWheel>();
		debug_typeText.text = "";
		debug_resultText.text = "";
		debug_visualTimer.transform.localScale = new Vector3(0f, 1f, 1f);
		wheelParent.transform.localScale = Vector3.one;
	}

	void Start () {
		RandomlyGenerateNew();
	}
	
	void Update () {
		if(Input.GetKeyDown(keyRandomize)){
			RandomlyGenerateNew();
		}
		bool allWheelsStopped = AllWheelsStopped();
		bool allWheelsSpinning = AllWheelsSpinning();
		if(Input.GetKeyDown(keyStart) && allWheelsStopped){
			StartSpin();
		}else if(Input.GetKeyDown(keyStop) && allWheelsSpinning){
			StopSpin();
		}
		if(allWheelsStopped && wheelsWereSpinning){
			DisplayWheelResultAndUpdateAffectionDisplay();
		}else if(!allWheelsStopped && !wheelsWereSpinning){
			debug_resultText.text = "";
		}
		wheelsWereSpinning = !allWheelsStopped;
	}

	void StartSpin () {
		for(int i=0; i<wheels.Count; i++){
			wheels[i].StartSpinning(Random.Range(minStartDelay, maxStartDelay));
		}
		wheelParent.transform.localScale = Vector3.one;
		timerAndAutoStopCoroutine = StartCoroutine(VisualTimer(maxStartDelay, durationUntilAutoStop));
	}

	void StopSpin () {
		for(int i=0; i<wheels.Count; i++){
			wheels[i].StopSpinning();
		}
		timerAndAutoStopCoroutine.Stop(this);
		debug_visualTimer.transform.localScale = new Vector3(0f, 1f, 1f);
	}

	void DisplayWheelResultAndUpdateAffectionDisplay () {
		string result = "";
		int totalValue = 0;
		int likeDelta = 0;
		for(int i=0; i<wheels.Count; i++){
			SpinnyWheelSlice topSlice = wheels[i].GetSliceOnTop();
			SpinnyWheelEffect topEffect = topSlice.Effect;
			if(topEffect.Type.Equals(SpinnyWheelEffect.EffectType.VALUE)){
				totalValue += topEffect.Value;
			}else{
				result += ", " + topEffect.name;
				if(topEffect.Type.Equals(SpinnyWheelEffect.EffectType.LIKEDELTA)){
					likeDelta += topEffect.Value;
				}
			}
		}
		debug_resultText.text = totalValue.ToString() + result;
		affectionDisplayer.DEBUG_SIMPLE_LIKE_UPDATE(likeDelta);
	}

	[ContextMenu("Generate new with given params")]
	void ContextGenerateNew () {
		GenerateNew(debug_numberOfBaseData, debug_numberOfOtherData, debug_otherData);
	}

	void RandomlyGenerateNew () {
		int baseNumber = Random.Range(0, maxBaseData + 1);
		int otherNumber = Random.Range(0, maxOtherData + 1);
		if((baseNumber == 0) && (otherNumber == 0)){
			baseNumber = 1;
		}
		SpinnyWheelConfig otherData = additionalData[Random.Range(0, additionalData.Count)];
		GenerateNew(baseNumber, otherNumber, otherData);
	}

	void GenerateNew (int baseNumber, int otherNumber, SpinnyWheelConfig extraData) {
		if(baseNumber < 0){
			Debug.LogError("Base Number must be non-negative!");
		}else if(otherNumber < 0){
			Debug.LogError("Other number must be non-negative!");
		}else if(extraData == null){
			Debug.LogError("Other data must not be null!");
		}else{
			for(int i=wheelParent.childCount-1; i>=0; i--){
				DestroyObject(wheelParent.GetChild(i).gameObject);
			}
			wheels.Clear();
			wheelParent.transform.localScale = Vector3.one;
			int totalNumber = baseNumber + otherNumber;
			float halfDelta = (totalNumber - 1f) / 2f;
			for(int i=0; i<totalNumber; i++){
				SpinnyWheelConfig dataToUse = ((i<baseNumber) ? baseData : extraData);
				SpinnyWheel newWheel = Instantiate(wheelPrefab, wheelParent);
				float offsetMultiplier = i - halfDelta;
				float xPos = offsetMultiplier * (horizontalOffset + newWheel.RectTransform.rect.width);
				newWheel.RectTransform.anchoredPosition = new Vector2(xPos, 0f);
				newWheel.RectTransform.localRotation = Quaternion.identity;
				newWheel.WheelRectTransform.localRotation = (randomizeRotationOnSpawn ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) : Quaternion.identity);
				newWheel.Initialize(dataToUse);
				wheels.Add(newWheel);
			}
			string typeText = "";
			if(baseNumber > 0){
				typeText += baseNumber.ToString() + "x " + baseData.name;
				if(otherNumber > 0){
					typeText += ", ";
				}
			}
			if(otherNumber > 0){
				typeText += otherNumber.ToString() + "x " + extraData.name;
			}
			debug_typeText.text = typeText;
		}
	}

	void DestroyObject (GameObject toDestroy) {
		if(Application.isPlaying) Destroy(toDestroy);
		else DestroyImmediate(toDestroy, false);
	}

	bool AllWheelsSpinning () {
		bool output = true;
		for(int i=0; i<wheels.Count; i++){
			output &= wheels[i].IsSpinning;
		}
		return output;
	}

	bool AllWheelsStopped () {
		bool output = true;
		for(int i=0; i<wheels.Count; i++){
			output &= !wheels[i].IsSpinning;
		}
		return output;
	}

	IEnumerator VisualTimer (float delay, float duration) {
		yield return new WaitForSeconds(delay);
		float startTime = Time.time;
		float t = 0;
		while(t<1f){
			if(timerScalesParent) wheelParent.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * minWheelParentScale, t);
			debug_visualTimer.transform.localScale = new Vector3((1f - t), 1f, 1f);
			t = (Time.time - startTime) / duration;
			yield return null;
		}
		if(AllWheelsSpinning()){
			for(int i=0; i<wheels.Count; i++){
				wheels[i].StopSpinning();
			}
		}
		debug_visualTimer.transform.localScale = new Vector3(0f, 1f, 1f);
	}

}
