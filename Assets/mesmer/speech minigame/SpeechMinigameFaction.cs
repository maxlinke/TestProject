using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mesmer {

	[CreateAssetMenu(fileName = "New Faction", menuName = "SpeechMinigame/Faction")]
	public class SpeechMinigameFaction : ScriptableObject {

		public const int MIN_FACTION_LIKE = -5;
		public const int MAX_FACTION_LIKE = 5;

		[SerializeField] Sprite sprite = null;
		[SerializeField] Color color = Color.black;
		[SerializeField] Color backgroundColor = Color.white;
		[SerializeField] int speechBonusThreshold = 10;
		[SerializeField] int like = 0;

		public Sprite Sprite { get { return sprite; } }
		public Color Color { get { return color; } }
		public Color BackgroundColor { get { return backgroundColor; } }
		public int SpeechBonusThreshold { get { return speechBonusThreshold; } }
		public int Like {
			get {
				return like;
			} set {
				if(value > MAX_FACTION_LIKE || value < MIN_FACTION_LIKE){
					Debug.LogWarning("Trying to set faction like to a value outside the bounds. (This might be intentional...)");
				}
				like = Mathf.Clamp(value, MIN_FACTION_LIKE, MAX_FACTION_LIKE);
			}
		}

	}

}