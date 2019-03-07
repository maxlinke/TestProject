using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mesmer {

	[CreateAssetMenu(fileName = "New Faction Group", menuName = "SpeechMinigame/Faction Group")]
	public class SpeechMinigameFactionGroup : ScriptableObject {

		[SerializeField] List<SpeechMinigameFaction> factions;

		public SpeechMinigameFaction[] GetFactions () {
			return factions.ToArray();
		}

	}

}