using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mesmer {

	public partial class SpeechMinigame {

		public abstract class SpeechMinigameRhetoric {

			public abstract void Apply ();

		}

		public class SimpleRhetoric : SpeechMinigameRhetoric {

			public readonly SpeechMinigameFaction appeasedFaction;
			public readonly SpeechMinigameFaction provokedFaction;

			public SimpleRhetoric (SpeechMinigameFaction appeasedFaction, SpeechMinigameFaction provokedFaction) {
				this.appeasedFaction = appeasedFaction;
				this.provokedFaction = provokedFaction;
			}

			public override void Apply () {
				appeasedFaction.Like++;
				provokedFaction.Like--;
			}

			public override string ToString () {
				return "+" + appeasedFaction.name + "\n-" + provokedFaction.name;
			}

		}

	}

}