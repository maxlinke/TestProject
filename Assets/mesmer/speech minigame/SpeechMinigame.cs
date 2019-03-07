using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Rhetoric = Mesmer.SpeechMinigame.SpeechMinigameRhetoric;

namespace Mesmer {

	public partial class SpeechMinigame : MonoBehaviour {

		public class FactionGameState {

			public int mobSize;
			public int mobOpinion;

			public FactionGameState (int mobSize, int mobOpinion) {
				this.mobSize = mobSize;
				this.mobOpinion = mobOpinion;
			}

		}

		private const int TARGET_Rhetoric_NUMBER = 10;

		public const int NUMBER_OF_NORMAL_RHETORIC_CHOICES = 3;

		[Header("Components")]
		[SerializeField] SpeechMinigameUI ui;

		[Header("Important Data")]
		[SerializeField] SpeechMinigameFactionGroup allFactionGroup;
		[SerializeField] SpeechMinigameFaction workerFaction;
		[SerializeField] SpeechMinigameFaction targetFaction;

		[Header("Settings")]
		[SerializeField] int numberOfNewListenersPerRound;
		[SerializeField, Range(0f, 1f)] float chanceOfGettingAWorker;
		[SerializeField, Range(0f, 1f)] float chanceOfGettingATargetFactionMember;
		[SerializeField, Range(0f, 60f)] float timeBetweenNewPolicemenArriving;

		SpeechMinigameFaction[] factions;
		Dictionary<SpeechMinigameFaction, FactionGameState> mob;

		List<Rhetoric> gameRhetoric;

		Coroutine gameLoop;

//		public event Action<int> OnRhetoricApplied = delegate {};		//event means it cannot be called from the outside, delegate {} means it won't throw nullpointerexceptions when there is nothing assigned

		void Start () {
			InitFactionsWithRandomLike();
			InitMobOpinionsBasedOnFactionLike();
			var availableNormalRhetoric = GetAvailableNormalRhetoric();
			ui.Initialize(this);
			ui.ShowRhetoricSelection(availableNormalRhetoric, Mathf.Min(availableNormalRhetoric.Count, TARGET_Rhetoric_NUMBER));
		}

		public void SetChosenNormalRhetoric (List<Rhetoric> chosenRhetoric) {
			gameRhetoric = chosenRhetoric;
		}

		public void StartMinigame () {
			ui.HideRhetoricSelection();
			gameLoop.Stop(this);
			gameLoop = StartCoroutine(GameLoop());
		}

		void InitFactionsWithRandomLike () {
			factions = allFactionGroup.GetFactions();
			foreach(var faction in factions){
				faction.Like = Random.Range(SpeechMinigameFaction.MIN_FACTION_LIKE, SpeechMinigameFaction.MAX_FACTION_LIKE + 1);
			}
		}

		void InitMobOpinionsBasedOnFactionLike () {
			mob = new Dictionary<SpeechMinigameFaction, FactionGameState>();
			foreach(var faction in factions){
				if(mob.ContainsKey(faction)){
					Debug.LogWarning("Trying to init a mob opinion for faction \"" + faction.name + "\" but there already was one!");
					continue;
				}
				mob.Add(faction, new FactionGameState(mobSize: 0, mobOpinion:faction.Like));
			}
		}

		List<Rhetoric> GetAvailableNormalRhetoric () {
			List<Rhetoric> output = new List<Rhetoric>();
			foreach(var faction in factions){
				foreach(var otherFaction in factions){
					if(faction != otherFaction){
						output.Add(new SimpleRhetoric(faction, otherFaction));
					}
				}
			}
			return output;
		}

		public List<Rhetoric> GetAvailableSpecialRhetoric () {
			return new List<Rhetoric>();
		}

		IEnumerator GameLoop () {
			ui.ShowMinigameDisplay();
			var audience = new Dictionary<SpeechMinigameFaction, int>();
			foreach(var faction in factions){
				audience.Add(faction, 0);
			}
			bool gameOver = false;
			while(!gameOver){
				//get the new audience
				var newAudienceMembers = GetRoundStartAudience();
				foreach(var faction in newAudienceMembers.Keys){
					audience[faction] += newAudienceMembers[faction];
				}
				//wait until a new card is played,
				yield return null;

			}
		}

		IEnumerator PoliceCoroutine () {
			int numberOfPolicemen = 0;
			while(true){
				float startTime = Time.time;
				float t = 0;
				while(t < 1){

					t = (Time.time - startTime) / timeBetweenNewPolicemenArriving;
					yield return null;
				}
				numberOfPolicemen++;
				if(GetTotalAudienceSize() < GetNumberOfListenersRequiredToContinueForNumberOfPolicemen(numberOfPolicemen)){
					Debug.Log("game over yo");
				}
			}
		}

		public int GetTotalAudienceSize () {
			int output = 0;
			foreach(var faction in mob.Keys){
				output += mob[faction].mobSize;
			}
			return output;
		}

		public int NumberOfAvailableNormalRhetoric () {
			return gameRhetoric.Count;
		}

		public List<Rhetoric> GetAndRemoveAvailableNormalRhetoric (int number) {
			var output = new List<Rhetoric>();
			for(int i=0; i<number; i++){
				var gottenRhetoric = gameRhetoric[Random.Range(0, gameRhetoric.Count)];
				gameRhetoric.Remove(gottenRhetoric);
				output.Add(gottenRhetoric);
			}
			return output;
		}

		Dictionary<SpeechMinigameFaction, int> GetRoundStartAudience () {
			var output = new Dictionary<SpeechMinigameFaction, int>();
			var nonSpecialFactions = new List<SpeechMinigameFaction>();
			foreach(SpeechMinigameFaction faction in factions){
				output.Add(faction, 0);
				if((faction != workerFaction) && (faction != targetFaction)){
					nonSpecialFactions.Add(faction);
				}
			}
			for(int i=0; i<numberOfNewListenersPerRound; i++){
				float randomValue = Random.value;
				if(randomValue < chanceOfGettingAWorker){
					output[workerFaction]++;
				}else{
					randomValue -= chanceOfGettingAWorker;
					if(randomValue < chanceOfGettingATargetFactionMember){
						output[targetFaction]++;
					}else{
						output[nonSpecialFactions[Random.Range(0, nonSpecialFactions.Count)]]++;
					}
				}
			}
			return output;
		}

		int GetNumberOfListenersRequiredToContinueForNumberOfPolicemen (int round) {
			return 2 * (round * (round + 1));
		}

	}

}