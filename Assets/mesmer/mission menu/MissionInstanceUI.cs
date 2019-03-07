using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Action = System.Action;

namespace Mesmer {

	public class MissionInstanceUI : MonoBehaviour {

		[Header("Components")]
		[SerializeField] RectTransform instanceUIRectTransform;
		[SerializeField] RectTransform contentContainerRectTransform;
		[SerializeField] RectTransform missionInfoContainerRectTransform;
		[SerializeField] RectTransform abortButtonContainerRectTransform;
		[SerializeField] Text missionTitleTextField;
		[SerializeField] Text missionDescriptionTextField;
		[SerializeField] Image timeLeftImage; 
		[SerializeField] GameObject sparkleContainer;
		[SerializeField] Transform sparkleLines;
		[SerializeField] Transform sparkleDots;
		[SerializeField] Button abortButton;
		[SerializeField] Text abortButtonKeyLabel;
		[SerializeField] Image abortButtonHoldProgressImage;

		[Header("Settings")]
		[SerializeField] float paddingAboveTitle;
		[SerializeField] float paddingBetweenTitleAndDescription;
		[SerializeField] float paddingBeneathDescription;
		[SerializeField] float paddingBeneathAbortButton;
		[SerializeField, Range(-60f, 60f)] float sparkleLineRPM;
		[SerializeField, Range(-60f, 60f)] float sparkleDotRPM;
		[SerializeField] float leftRightMoveDuration;
		[SerializeField] AnimationCurve leftRightMovementCurve;
		[SerializeField] float upDownMoveDuration;
		[SerializeField] AnimationCurve upDownMovementCurve;
		[SerializeField] float keyHoldTimeToAbort;

		KeyCode abortKey;
		float abortHoldStart;

		bool m_sparkling;
		bool initialized;
		bool responsive;
		MissionMenu ownerMenu;
		RectTransform missionTitleRectTransform;
		RectTransform missionDescriptionRectTransform;
		float contentOverhang;

		Coroutine leftRightMovement;

		public Coroutine upDownMovement;
		public RectTransform rectTransform { get { return instanceUIRectTransform; } }
		public MissionInstance missionInstance { get; private set; }

		public bool sparkling {
			get {
				return m_sparkling;
			} set {
				if((value != m_sparkling) && value){
					sparkleLines.localRotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
					sparkleDots.localRotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
				}
				sparkleContainer.SetActive(value);
				m_sparkling = value;
			}
		}

		public bool abortButtonInteractable {
			get {
				return abortButton.interactable;
			} set {
				if(!value) abortButtonHoldProgressImage.fillAmount = 0f;
				abortButton.interactable = value;
			}
		}

		void Awake () {
			sparkling = false;
			responsive = true;
			initialized = false;
			abortHoldStart = Mathf.Infinity;
			abortButtonInteractable = false;
			missionTitleRectTransform = (RectTransform)(missionTitleTextField.transform);
			missionDescriptionRectTransform = (RectTransform)(missionDescriptionTextField.transform);
		}

		void Update () {
			if(missionInstance.mission.ended){
				return;
			}
			if(sparkling){
				sparkleLines.localEulerAngles += new Vector3(0f, 0f, Time.deltaTime * sparkleLineRPM * 6f);		//360 * rpm / 60 = 6 * rpm
				sparkleDots.localEulerAngles += new Vector3(0f, 0f, Time.deltaTime * sparkleDotRPM * 6f);
			}
			if(abortButton.interactable){
				if(Input.GetKeyDown(abortKey)){
					abortHoldStart = Time.time;
				}
				if(Input.GetKeyUp(abortKey)){
					abortHoldStart = Mathf.Infinity;
					abortButtonHoldProgressImage.fillAmount = 0f;
				}
				if(abortHoldStart < Mathf.Infinity){
					float timeHeld = Time.time - abortHoldStart;
					abortButtonHoldProgressImage.fillAmount = timeHeld / keyHoldTimeToAbort;
					if(timeHeld > keyHoldTimeToAbort){
						abortButton.onClick.Invoke();
					}
				}
			}

			timeLeftImage.fillAmount = missionInstance.timeLeft01;
			timeLeftImage.color = (missionInstance.runningOut ? Color.red : Color.white);
		}

		void OnMissionEnded () {
			ownerMenu.RemoveMissionInstanceUI(this);
		}

		public void Initialize (MissionMenu ownerMenu, MissionInstance missionInstance, KeyCode abortKey) {
			if(initialized){
				Debug.LogWarning("Tried to initialize object \"" + gameObject.name + "\" twice...");
			}else{
				if(!gameObject.activeSelf){
					gameObject.SetActive(true);
				}
				this.ownerMenu = ownerMenu;
				this.missionInstance = missionInstance;
				this.abortKey = abortKey;
				missionInstance.mission.OnMissionEnded += OnMissionEnded;
				missionTitleTextField.text = missionInstance.mission.title;
				missionDescriptionTextField.text = missionInstance.mission.description;
				abortButton.onClick.AddListener(() => {missionInstance.mission.Abort();});
				abortButtonKeyLabel.text = "Hold " + abortKey + " to abort";
				SizeAndPositionAllTheRectTransforms();
				initialized = true;
			}
		}

		void SizeAndPositionAllTheRectTransforms () {
			float titleHeight = missionTitleTextField.preferredHeight;
			float descriptionHeight = missionDescriptionTextField.preferredHeight;
			missionTitleRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, titleHeight);
			missionDescriptionRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descriptionHeight);
			float totalOffset = paddingAboveTitle;
			missionTitleRectTransform.anchoredPosition = new Vector2(0f, -totalOffset);
			totalOffset += titleHeight + paddingBetweenTitleAndDescription;
			missionDescriptionRectTransform.anchoredPosition = new Vector2(0f, -totalOffset);
			totalOffset += descriptionHeight + paddingBeneathDescription;
			abortButtonContainerRectTransform.anchoredPosition = new Vector2(0f, -totalOffset);
			totalOffset += abortButtonContainerRectTransform.rect.height + paddingBeneathAbortButton;
			float totalContentHeight = totalOffset;
			contentContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(totalContentHeight, instanceUIRectTransform.rect.height));
			contentOverhang = contentContainerRectTransform.rect.height - rectTransform.rect.height;
		}

		public void UpdateContentPositionBasedOnOverhangSpace () {
			float spaceBelow = ((RectTransform)(rectTransform.parent)).rect.height + rectTransform.anchoredPosition.y - rectTransform.rect.height;
			if(spaceBelow < contentOverhang){
				contentContainerRectTransform.pivot = new Vector2(0f, 0f);
				contentContainerRectTransform.SetAnchoredYPos(-rectTransform.rect.height);
			}else{
				contentContainerRectTransform.pivot = new Vector2(0f, 1f);
				contentContainerRectTransform.SetAnchoredYPos(0);
			}
		}

		public void Minimize () {
			DoIfResponsive(() => {
				leftRightMovement.Stop(this);
				leftRightMovement = StartCoroutine(MoveXCoordinate(0, leftRightMoveDuration, leftRightMovementCurve));
			});
		}

		public void ShowMissionInfo () {
			DoIfResponsive(() => {
				if(sparkling) sparkling = false;
				if(missionInstance.unread) missionInstance.unread = false;
				leftRightMovement.Stop(this);
				leftRightMovement = StartCoroutine(MoveXCoordinate(-(contentContainerRectTransform.rect.width + rectTransform.rect.width), leftRightMoveDuration, leftRightMovementCurve));
			});
		}

		public void MoveOffscreenAndDestroy () {
			DoIfResponsive(() => {
				responsive = false;
				missionInstance.mission.OnMissionEnded -= OnMissionEnded;
				leftRightMovement.Stop(this);
				leftRightMovement = StartCoroutine(MoveXCoordinate(200, leftRightMoveDuration, leftRightMovementCurve, () => { Destroy(this.gameObject); }));
			});
		}

		void DoIfResponsive (Action actionToDo) {
			if(responsive){
				actionToDo.InvokeNullsafe();
			}else{
				Debug.LogWarning(gameObject.name + "was asked to invoke \"" + actionToDo.Method.Name + "\" but was already set to be unresponsive.");
			}
		}

		IEnumerator MoveXCoordinate (float targetX, float duration, AnimationCurve curve = null, Action endAction = null, float delay = 0f) {
			if(delay > 0f) yield return new WaitForSeconds(delay);
			float startX = rectTransform.anchoredPosition.x;
			float startTime = Time.time;
			float t = 0f;
			while(t < 1f){
				rectTransform.SetAnchoredXPos(Mathf.LerpUnclamped(startX, targetX, curve.EvaluateNullsafe(t)));
				t = (Time.time - startTime) / duration;
				yield return null;
			}
			rectTransform.SetAnchoredXPos(targetX);
			endAction.InvokeNullsafe();
		}

		IEnumerator MoveYCoordinate (float targetY, float duration, AnimationCurve curve = null, Action endAction = null, float delay = 0f) {
			if(delay > 0f) yield return new WaitForSeconds(delay);
			float startY = rectTransform.anchoredPosition.y;
			float startTime = Time.time;
			float t = 0f;
			while(t < 1f){
				rectTransform.SetAnchoredYPos(Mathf.LerpUnclamped(startY, targetY, curve.EvaluateNullsafe(t)));
				t = (Time.time - startTime) / duration;
				yield return null;
			}
			rectTransform.SetAnchoredYPos(targetY);
			endAction.InvokeNullsafe();
		}

		public IEnumerator MoveYCoordinate (float targetY, Action endAction = null, float delay = 0f) {
			yield return MoveYCoordinate(targetY, upDownMoveDuration, upDownMovementCurve, endAction, delay);
		}
			
	}

}