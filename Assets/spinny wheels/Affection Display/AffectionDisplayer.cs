using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AffectionDisplayer : MonoBehaviour {

	public const int MIN_PERSON_LIKE = -1;
	public const int MAX_PERSON_LIKE = 1;
	public const int MIN_GROUP_LIKE = -4;
	public const int MAX_GROUP_LIKE = 4;

	[Header("Components")]
	[SerializeField] Text nameTextField;
	[SerializeField] Text groupTextField;
	[SerializeField] RectTransform personLikeViewport;
	[SerializeField] RectTransform groupLikeViewport;

	[Header("Generated Stuff")]
	[SerializeField] float personLikeElementHeightToWidth;
	[SerializeField] float groupLikeElementHeightToWidth;
	[SerializeField] int personLikeFontSize;
	[SerializeField] int groupLikeFontSize;
	[SerializeField] Color textColor;

	[Header("Settings")]
	[SerializeField] float advanceTime;
	[SerializeField] AnimationCurve advanceCurve;
	[SerializeField] float returnTime;
	[SerializeField] AnimationCurve returnCurve;

	public int personLike { get; private set; }
	public int groupLike { get; private set; }

	int numberOfPersonLikeStates;
	int numberOfGroupLikeStates;

	RectTransform personLikeRectTransform;
	RectTransform groupLikeRectTransform;

	Coroutine personAnimationCoroutine;
	Coroutine groupAnimationCoroutine;

	void Awake () {
		numberOfPersonLikeStates = Mathf.Abs(MIN_PERSON_LIKE - MAX_PERSON_LIKE) + 1;
		numberOfGroupLikeStates = Mathf.Abs(MIN_GROUP_LIKE - MAX_GROUP_LIKE) + 1;

		float personLikeElementHeight = personLikeElementHeightToWidth * personLikeViewport.rect.width;
		float groupLikeElementHeight = groupLikeElementHeightToWidth * groupLikeViewport.rect.width;

		personLikeRectTransform = FillViewportAndGetTheElement(personLikeViewport, personLikeElementHeight, MIN_PERSON_LIKE, MAX_PERSON_LIKE, personLikeFontSize);
		groupLikeRectTransform = FillViewportAndGetTheElement(groupLikeViewport, groupLikeElementHeight, MIN_GROUP_LIKE, MAX_GROUP_LIKE, groupLikeFontSize);
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			DEBUG_SIMPLE_LIKE_UPDATE(1);
		}else if(Input.GetKeyDown(KeyCode.DownArrow)){
			DEBUG_SIMPLE_LIKE_UPDATE(-1);
		}
	}

	RectTransform FillViewportAndGetTheElement (RectTransform viewport, float elementHeight, int min, int max, int fontSize) {
		int numberOfElements = Mathf.Abs(min - max) + 1;
		RectTransform elementParent = new GameObject("Element Parent", typeof(RectTransform)).GetComponent<RectTransform>();
		elementParent.parent = viewport;
		elementParent.pivot = new Vector2(0.5f, 0.5f);
		elementParent.anchoredPosition = Vector2.zero;
		elementParent.localRotation = Quaternion.identity;
		elementParent.sizeDelta = new Vector2(viewport.rect.width, elementHeight * (numberOfElements - 1));
		for(int i=0; i<numberOfElements; i++){
			int number = min + i;
			float iFrac = ((float)i)/(numberOfElements-1);
			GameObject newObject = new GameObject("Viewport Element", typeof(RectTransform), typeof(Text));
			RectTransform newRectTransform = newObject.GetComponent<RectTransform>();
			newRectTransform.parent = elementParent;
			newRectTransform.pivot = new Vector2(0.5f, 0.5f);
			newRectTransform.anchoredPosition = new Vector2(0f, (iFrac - 0.5f) * elementParent.rect.height);
			newRectTransform.localRotation = Quaternion.identity;
			newRectTransform.sizeDelta = new Vector2(elementParent.rect.width, elementHeight);
			Text newText = newObject.GetComponent<Text>();
			newText.text = ((number > 0) ? "+" : "") + number.ToString();
			newText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			newText.fontSize = fontSize;
			newText.color = textColor;
			newText.alignment = TextAnchor.MiddleCenter;
			newText.horizontalOverflow = HorizontalWrapMode.Overflow;
			newText.verticalOverflow = VerticalWrapMode.Overflow;
		}
		return elementParent;
	}

	void CreateANewTextObjectAndStuff () {
		GameObject newObject = new GameObject("object", typeof(RectTransform));
		newObject.transform.parent = this.transform.parent;
		Text newText = new GameObject("text",  typeof(RectTransform), typeof(Text)).GetComponent<Text>();
		newText.transform.parent = newObject.transform;
		newText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		newText.fontSize = 24;
		newText.alignment = TextAnchor.MiddleCenter;
		newText.verticalOverflow = VerticalWrapMode.Overflow;
		newText.horizontalOverflow = HorizontalWrapMode.Wrap;
		newText.text = "Hello, I was created by script instead of instantiated from a prefab!";
		newText.color = Color.black;
		((RectTransform)newText.transform).sizeDelta = new Vector2(240f, 50f);
	}

	public void Initialize (string personName, string personGroup, int personLike, int groupLike) {
		string firstName, lastName;
		SplitName(personName, out firstName, out lastName);
		nameTextField.text = firstName + "\n" + lastName;
		groupTextField.text = personGroup;
		SetLikeWithoutAnimation(personLike, groupLike);
	}

	public void DEBUG_SIMPLE_LIKE_UPDATE (int delta) {
		if(delta > 0){
			if(personLike >= MAX_PERSON_LIKE){
				UpdateLike(delta, 1);
			}else{
				UpdateLike(delta, 0);
			}
		}else if(delta < 0){
			if(personLike <= MIN_PERSON_LIKE){
				UpdateLike(delta, -1);
			}else{
				UpdateLike(delta, 0);
			}
		}
	}

	public void SetLikeWithoutAnimation (int personLike, int groupLike) {
		this.personLike = Mathf.Clamp(personLike, MIN_PERSON_LIKE, MAX_PERSON_LIKE);
		this.groupLike = Mathf.Clamp(groupLike, MIN_GROUP_LIKE, MAX_GROUP_LIKE);
		Vector2 personPivot, groupPivot;
		GetPivotsForValues(this.personLike, this.groupLike, out personPivot, out groupPivot);
		personLikeRectTransform.pivot = personPivot;
		groupLikeRectTransform.pivot = groupPivot;
	}

	public void UpdateLike (int personDelta, int groupDelta) {
		int newPersonLike = Mathf.Clamp(personLike + personDelta, MIN_PERSON_LIKE, MAX_PERSON_LIKE);
		int newGroupLike = Mathf.Clamp(groupLike + groupDelta, MIN_GROUP_LIKE, MAX_GROUP_LIKE);
		bool gotPersonDelta = (personDelta != 0);
		bool gotGroupDelta = (groupDelta != 0);
		bool personLikeChanged = (personLike != newPersonLike);
		bool groupLikeChanged = (groupLike != newGroupLike);
		Vector2 advancePersonPivot, advanceGroupPivot;
		GetPivotsForValues(personLike + personDelta, groupLike + groupDelta, out advancePersonPivot, out advanceGroupPivot);
		Vector2 endPersonPivot, endGroupPivot;
		GetPivotsForValues(newPersonLike, newGroupLike, out endPersonPivot, out endGroupPivot);
		if(gotPersonDelta && gotGroupDelta){
			StartAdvancing(ref personAnimationCoroutine, personLikeRectTransform, advancePersonPivot, endPersonPivot, 0, personLikeChanged);
			StartAdvancing(ref groupAnimationCoroutine, groupLikeRectTransform, advanceGroupPivot, endGroupPivot, advanceTime, groupLikeChanged);
		}else if(gotPersonDelta){
			StartAdvancing(ref personAnimationCoroutine, personLikeRectTransform, advancePersonPivot, endPersonPivot, 0, personLikeChanged);
		}else if(gotGroupDelta){
			Debug.LogWarning("Got a group delta, but no person, delta, weird... (" + personDelta + ", " + groupDelta + ")");
		}else{
			Debug.LogWarning("Why the update call? Both are 0, right? (" + personDelta + ", " + groupDelta + ")");
		}
		personLike = newPersonLike;
		groupLike = newGroupLike;
	}

	void StartAdvancing (ref Coroutine animationCoroutine, RectTransform rectTransform, Vector2 advancePivot, Vector2 endPivot, float delay, bool stayAtAdvancePosition){
		animationCoroutine.Stop(this);
		if(stayAtAdvancePosition){
			animationCoroutine = StartCoroutine(AdvanceToPosition(rectTransform, advancePivot, delay));
		}else{
			animationCoroutine= StartCoroutine(AdvanceToPositionAndBounceBack(rectTransform, advancePivot, endPivot, delay));
		}
	}

	void GetPivotsForValues (int targetPersonLike, int targetGroupLike, out Vector2 personPivot, out Vector2 groupPivot) {
		float normedPersonLike = ((float)(targetPersonLike - MIN_PERSON_LIKE) / (MAX_PERSON_LIKE - MIN_PERSON_LIKE));
		float normedGroupLike = ((float)(targetGroupLike - MIN_GROUP_LIKE) / (MAX_GROUP_LIKE - MIN_GROUP_LIKE));
		personPivot = new Vector2(0.5f, normedPersonLike);
		groupPivot = new Vector2(0.5f, normedGroupLike);
	}

	void SplitName (string name, out string partOne, out string partTwo) {
		string[] parts = name.Split(null);	//null means whitespace... weird, i know...
		if(parts.Length == 1){
			partOne = parts[0];
			partTwo = null;
		}else if(parts.Length == 2){
			partOne = parts[0];
			partTwo = parts[1];
		}else{
			bool doneWithPartOne = false;
			partOne = string.Empty;
			partTwo = string.Empty;
			for(int i=0; i<parts.Length-1; i++){
				if(!doneWithPartOne){
					if(!IsPreposition(parts[i])){
						partOne += parts[i] + " ";
					}else{
						partTwo = parts[i] + " ";
						doneWithPartOne = true;
					}
				}else{
					partTwo += parts[i] + " ";
				}
			}
			partTwo += parts[parts.Length-1];
			if(partOne.EndsWith(" ")) partOne = partOne.Remove(partOne.Length-1, 1);
		}
	}

	IEnumerator AdvanceToPosition (RectTransform rectTransform, Vector2 advancePivot, float delay = 0){
		if(delay > 0){
			yield return new WaitForSeconds(delay);
		}
		Vector2 startPivot = rectTransform.pivot;
		float startTime = Time.time;
		float t = 0;
		while(t<1){
			rectTransform.pivot = Vector2.LerpUnclamped(startPivot, advancePivot, advanceCurve.Evaluate(t));
			t = (Time.time - startTime) / advanceTime;
			yield return null;
		}
		rectTransform.pivot = advancePivot;
	}

	IEnumerator AdvanceToPositionAndBounceBack (RectTransform rectTransform, Vector2 advancePivot, Vector2 backBouncePivot, float delay = 0) {
		yield return AdvanceToPosition(rectTransform, advancePivot, delay);
		float startTime = Time.time;
		float t = 0;
		while(t<1){
			rectTransform.pivot = Vector2.LerpUnclamped(backBouncePivot, advancePivot, returnCurve.Evaluate(t));
			t = (Time.time - startTime) / returnTime;
			yield return null;
		}
		rectTransform.pivot = backBouncePivot;
	}

	static string[] prefixes = new string[]{
		"mr", "ms", "mrs", "dr", "dipl", "ing", "med"
	};

	bool IsPrefix (string possiblePrefix) {
		Regex rgx = new Regex("[^a-zA-z0-9 -]");
		possiblePrefix = rgx.Replace(possiblePrefix.ToLower(), "");
		for(int i=0; i<prefixes.Length; i++){
			if(prefixes[i].Equals(possiblePrefix)) return true;
		}
		return false;
	}

	static string[] prepositions = new string[]{
		"von", "van", "zu", "of", "de", "di"
	};

	bool IsPreposition (string possiblePreposition) {
		possiblePreposition = possiblePreposition.ToLower();
		for(int i=0; i<prepositions.Length; i++){
			if(prepositions[i].Equals(possiblePreposition)) return true;
		}
		return false;
	}

}
