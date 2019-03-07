using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Rhetoric = Mesmer.SpeechMinigame.SpeechMinigameRhetoric;

namespace Mesmer {

	public class SpeechMinigameUI : MonoBehaviour {

		[Header("Components (Rhetoric Selection)")]
		[SerializeField] RectTransform rhetoricSelectionContainer;
		[SerializeField] RectTransform rhetoricSelectionScrollView;
		[SerializeField] RectTransform rhetoricSelectionScrollViewContent;
		[SerializeField] Text rhetoricSelectionProgressText;
		[SerializeField] Button rhetoricSelectionConfirmButton;

		[Header("Components (Minigame Display)")]
		[SerializeField] Image nextPolicemanRadialImage;

		[Header("Prefabs")]
		[SerializeField] SpeechMinigameUIRhetoricSelector rhetoricSelectorPrefab;
		[SerializeField] SpeechMinigameUIRhetoricCard rhetoricCardPrefab;

//		SpeechMinigame speechMinigame;
		List<SpeechMinigameUIRhetoricSelector> rhetoricSelectors;
		int rhetoricSelectionTarget;

		public void Initialize (SpeechMinigame speechMinigame) {
//			this.speechMinigame = speechMinigame;
			this.rhetoricSelectionConfirmButton.onClick.AddListener(() => {
				speechMinigame.SetChosenNormalRhetoric(GetRhetoricFromActiveSelectors());
				speechMinigame.StartMinigame();
			});
		}

		#region Rhetoric Selection

		public void ShowRhetoricSelection (IEnumerable<Rhetoric> choosableRhetorics, int numberToChoose) {
			rhetoricSelectionTarget = numberToChoose;
			DestroyExistingSelectorsAndClearList();
			foreach(var choosableRhetoric in choosableRhetorics){
				var newSelector = Instantiate(rhetoricSelectorPrefab, rhetoricSelectionScrollViewContent);
				newSelector.Initialize(choosableRhetoric, RhetoricSelectionChanged);
				newSelector.RectTransform.localRotation = Quaternion.identity;
				newSelector.RectTransform.localScale = Vector3.one;
				rhetoricSelectors.Add(newSelector);
			}
			if(rhetoricSelectors.Count > 0){
				float selectorHeight = rhetoricSelectors[0].RectTransform.rect.height;
				float selectorWidth = rhetoricSelectors[0].RectTransform.rect.width;
				rhetoricSelectionScrollView.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, selectorWidth);
				rhetoricSelectionScrollViewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rhetoricSelectors.Count * selectorHeight);
				for(int i=0; i<rhetoricSelectors.Count; i++){
					rhetoricSelectors[i].RectTransform.anchoredPosition = new Vector2(0, -(0.5f + i) * selectorHeight);
				}
			}else{
				Debug.LogWarning("No rhetoric selectors were spawned (probably because there was no rhetoric to choose from?");
			}
			rhetoricSelectionContainer.SetGOActive(true);
			RhetoricSelectionChanged();
		}

		public void HideRhetoricSelection () {
			rhetoricSelectionContainer.SetGOActive(false);
		}

		void DestroyExistingSelectorsAndClearList () {
			if(rhetoricSelectors != null){
				foreach(var selector in rhetoricSelectors){
					Destroy(selector.gameObject);
				}
				rhetoricSelectors.Clear();
			}else{
				rhetoricSelectors = new List<SpeechMinigameUIRhetoricSelector>();
			}
		}

		void RhetoricSelectionChanged () {
			int numberOfSelectedRhetorics = 0;
			foreach(var selector in rhetoricSelectors){
				if(selector.SelectionState) numberOfSelectedRhetorics++;
			}
			if(numberOfSelectedRhetorics > rhetoricSelectionTarget){
				Debug.LogError("More selectors selected than should be possible!");
			}else{
				bool limitReached = (numberOfSelectedRhetorics == rhetoricSelectionTarget);
				foreach(var selector in rhetoricSelectors){
					selector.SetInteractable(limitReached ? selector.SelectionState : true);
				}
				rhetoricSelectionConfirmButton.interactable = limitReached;
			}
			rhetoricSelectionProgressText.text = numberOfSelectedRhetorics.ToString() + "/" + rhetoricSelectionTarget.ToString();
		}

		List<Rhetoric> GetRhetoricFromActiveSelectors () {
			var output = new List<Rhetoric>();
			foreach(var selector in rhetoricSelectors){
				if(selector.SelectionState){
					output.Add(selector.Rhetoric);
				}
			}
			return output;
		}

		#endregion

		#region Speech Minigame Display

		public void ShowMinigameDisplay () {
			//spawn cards for 3 normal and all special rhetorics. remove from list in speechminigame
			//each time a normal card is clicked, it vanishes and the ui REMOVES an available rhetoric from the minigame
//			var initialNormalRhetoric = speechMinigame.GetAndRemoveAvailableNormalRhetoric(SpeechMinigame.NUMBER_OF_NORMAL_RHETORIC_CHOICES);
//			var initialSpecialRhetoric = speechMinigame.GetAvailableSpecialRhetoric();
		}

		public void HideMinigameDisplay () {

		}

		public void UpdateAudienceDisplay (Dictionary<SpeechMinigameFaction, SpeechMinigame.FactionGameState> mobData) {
			//big like bars in the middle
			//a "fractioned" bar at the side, next to the police bar
		}

		public void UpdateNextPolicemanProgressDisplay (float progress01) {
			nextPolicemanRadialImage.fillAmount = Mathf.Clamp01(progress01);
		}

		public void UpdatePoliceThreatDisplay (int policeForce) {
			//policeforce = number of people needed to keep the protest alive. so it uses the same scale as the people bars...
		}

		#endregion

	}

}