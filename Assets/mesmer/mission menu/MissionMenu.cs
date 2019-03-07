using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mesmer {

	public class MissionMenu : MonoBehaviour {

		[Header("Components")]
		[SerializeField] RectTransform missionInstanceParent;
		[SerializeField] MissionInstanceStatesHint missionInstanceStatesHint;

		[Header("Prefabs")]
		[SerializeField] MissionInstanceUI missionInstanceUIPrefab;

		[Header("Settings")]
		[SerializeField] KeyCode missionAbortKey;
		[SerializeField] KeyCode newMissionKey;
		[SerializeField] float minMissionLength;
		[SerializeField] float maxMissionLength;

		List<MissionInstance> missionInstances;
		List<MissionInstanceUI> instanceUIs;
		MissionInstanceUI prioritisedInstanceUI;

		bool maximizedMode;

		void Start () {
			missionInstances = new List<MissionInstance>();
			instanceUIs = new List<MissionInstanceUI>();
			maximizedMode = false;
			missionInstanceStatesHint.Init(0, 0);
		}

		void Update () {
			int unread, ending;
			CheckMissionsAndFailIfTimedOut(out unread, out ending);
			missionInstanceStatesHint.unreadMissions = unread;
			missionInstanceStatesHint.endingMissions = ending;

			if(Input.GetKeyDown(newMissionKey)){
				NewMissionGot(GetNewRandomMission());
			}

			if(Input.GetKeyDown(KeyCode.LeftArrow)){
				if(!maximizedMode && missionInstances.Count > 0) Maximize();
			}else if(Input.GetKeyDown(KeyCode.RightArrow)){
				if(maximizedMode) Minimize();
			}

			if(maximizedMode){
				if(Input.GetKeyDown(KeyCode.UpArrow)){
					SetPrioritisedViaOffset(-1);
				}else if(Input.GetKeyDown(KeyCode.DownArrow)){
					SetPrioritisedViaOffset(+1);
				}
			}
		}

		void Maximize () {
			foreach(var missionInstace in missionInstances){
				if(!missionInstace.Equals(prioritisedInstanceUI.missionInstance)){
					var newInstanceUI = CreateNewInstanceUIOffscreenAndMoveOnscreenMinimized(missionInstace, 0);	//the y coord 0 is replaced a few lines later...
					newInstanceUI.rectTransform.SetSiblingIndex(0);
				}
			}
			float[] yCoords = GetYPositionsForAllInstanceUIs();
			for(int i=0; i<instanceUIs.Count; i++){
				instanceUIs[i].rectTransform.SetAnchoredYPos(yCoords[i]);
				instanceUIs[i].UpdateContentPositionBasedOnOverhangSpace();
			}
			if(prioritisedInstanceUI != null){
				prioritisedInstanceUI.ShowMissionInfo();
				prioritisedInstanceUI.abortButtonInteractable = true;
			}

			missionInstanceStatesHint.activeStateOverridden = true;
			missionInstanceStatesHint.gameObject.SetActive(false);

			maximizedMode = true;
		}

		void Minimize () {
			for(int i=instanceUIs.Count-1; i>=0; i--){
				var instanceUI = instanceUIs[i];
				if(!instanceUI.Equals(prioritisedInstanceUI)){
					instanceUI.MoveOffscreenAndDestroy();
					instanceUIs.RemoveAt(i);
				}
			}
			if(prioritisedInstanceUI != null){
				var newInstanceList = new List<MissionInstance>();
				newInstanceList.Add(prioritisedInstanceUI.missionInstance);
				foreach(var otherInstance in missionInstances){
					if(!otherInstance.Equals(prioritisedInstanceUI.missionInstance)){
						newInstanceList.Add(otherInstance);
					}
				}
				missionInstances = newInstanceList;
				prioritisedInstanceUI.Minimize();
				prioritisedInstanceUI.upDownMovement.Stop(prioritisedInstanceUI);
				prioritisedInstanceUI.upDownMovement = prioritisedInstanceUI.StartCoroutine(prioritisedInstanceUI.MoveYCoordinate(0, prioritisedInstanceUI.UpdateContentPositionBasedOnOverhangSpace, 0.2f));
			}

			missionInstanceStatesHint.activeStateOverridden = false;
			missionInstanceStatesHint.UpdateDisplay();

			maximizedMode = false;
		}

		public void RemoveMissionInstanceUI (MissionInstanceUI toRemove) {
			missionInstances.Remove(toRemove.missionInstance);
			instanceUIs.Remove(toRemove);
			toRemove.MoveOffscreenAndDestroy();
			if(toRemove.Equals(prioritisedInstanceUI)){
				SetPrioritised(FindOrCreateNewPrioritisedIfPossible());
			}
			if(instanceUIs.Count <= 0 && maximizedMode){
				Minimize();
			}
		}

		public void NewMissionGot (Mission mission) {
			var newMissionInstance = new MissionInstance(mission);
			missionInstances.Add(newMissionInstance);
			if(maximizedMode){
				Debug.LogWarning("Got a mission while in maximized mode. That's not supposed to happen...");
			}else{
				if(prioritisedInstanceUI == null){
					prioritisedInstanceUI = CreateNewInstanceUIOffscreenAndMoveOnscreenMinimized(newMissionInstance, 0);
					prioritisedInstanceUI.sparkling = true;
				}
			}
		}

		void SetPrioritised (MissionInstanceUI toPrioritise) {
			if(!maximizedMode){
				Debug.LogWarning("Getting setprioritised while not in maximized mode. aborting");
				return;
			}
			foreach(var instanceUI in instanceUIs){
				if(instanceUI.Equals(toPrioritise)){
					instanceUI.ShowMissionInfo();
					instanceUI.abortButtonInteractable = true;
				}else{
					instanceUI.Minimize();
					instanceUI.abortButtonInteractable = false;
				}
			}
			prioritisedInstanceUI = toPrioritise;
		}

		void SetPrioritisedViaOffset (int offset) {
			if(!maximizedMode){
				Debug.LogWarning("trying to switch through instanceuis while not in maximized mode. aborting.");
				return;
			}
			if(instanceUIs.Count > 0){
				int newPrioIndex;
				if(prioritisedInstanceUI == null){
					newPrioIndex = 0;
				}else{
					newPrioIndex = instanceUIs.IndexOf(prioritisedInstanceUI) + offset;
					while(newPrioIndex < 0){
						newPrioIndex += instanceUIs.Count;
					}
					while(newPrioIndex >= instanceUIs.Count){
						newPrioIndex -= instanceUIs.Count;
					}
				}
				SetPrioritised(instanceUIs[newPrioIndex]);
			}
		}

		Mission GetNewRandomMission () {
			return new Mission(
				title: RandomTextGenerator.GetRandomWordSequence(new RandomTextGenerator.Distribution(1, 2, 4), true),
				description: RandomTextGenerator.GetRandomParagraph(new RandomTextGenerator.Distribution(1, 3, 5)),
				duration: Random.Range(minMissionLength, maxMissionLength)
			);
		}

		void CheckMissionsAndFailIfTimedOut (out int unreadMissions, out int endingMissions) {
			var timedOutMissions = new List<Mission>();
			unreadMissions = 0;
			endingMissions = 0;
			for(int i=missionInstances.Count-1; i>=0; i--){
				if(missionInstances[i].timeLeft <= 0f){
					timedOutMissions.Add(missionInstances[i].mission);
					missionInstances.RemoveAt(i);
				}else{
					if(missionInstances[i].runningOut) endingMissions++;
					if(missionInstances[i].unread) unreadMissions++;
				}
			}
			//this separation is important because upon failing the corresponding ui will be removed and if it is the prioritised one, a new one will be searched for
			//and that just takes the first missioninstance and runs with it, therefore we need to remove all invalid instances first and then fail them...
			for(int i=0; i<timedOutMissions.Count; i++){
				timedOutMissions[i].Fail();
			}
		}

		float[] GetYPositionsForAllInstanceUIs () {
			float elementHeight = instanceUIs[0].rectTransform.rect.height;
			float parentHeight = missionInstanceParent.rect.height;
			float scalingFactor = Mathf.Clamp01((parentHeight - elementHeight) / ((instanceUIs.Count - 1) * elementHeight));
			float[] output = new float[instanceUIs.Count];
			for(int i=0; i<instanceUIs.Count; i++){
				output[i] = -i * scalingFactor * elementHeight;
			}
			return output;
		}

		MissionInstanceUI FindOrCreateNewPrioritisedIfPossible () {
			if(maximizedMode && (instanceUIs.Count > 0)){
				return instanceUIs[0];
			}else if(!maximizedMode && (missionInstances.Count > 0)){
				return CreateNewInstanceUIOffscreenAndMoveOnscreenMinimized(missionInstances[0], 0);
			}else{
				return null;
			}
		}

		MissionInstanceUI CreateNewInstanceUIOffscreenAndMoveOnscreenMinimized (MissionInstance instance, float yCoord) {
			var newInstanceUI = Instantiate(missionInstanceUIPrefab, missionInstanceParent);
			newInstanceUI.Initialize(this, instance, missionAbortKey);
			newInstanceUI.rectTransform.anchoredPosition = new Vector2(100, yCoord);
			newInstanceUI.sparkling = instance.unread;
			newInstanceUI.Minimize();
			instanceUIs.Add(newInstanceUI);
			return newInstanceUI;
		}

	}

}


