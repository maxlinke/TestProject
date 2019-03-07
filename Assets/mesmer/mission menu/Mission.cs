using UnityEngine;
using System;

using Debug = UnityEngine.Debug;

namespace Mesmer {

	public class Mission {

		public readonly string title;
		public readonly string description;
		public readonly float duration;

		public bool ended { get; private set; }

		public event Action OnMissionEnded = delegate {};

		public Mission (string title, string description, float duration) {
			this.title = title;
			this.description = description;
			this.duration = duration;
			this.ended = false;
		}

		public void Finish () {
			Debug.Log("Finished mission \"" + title + "\"");
			EndMission();
		}

		public void Abort () {
			Debug.Log("Aborting mission \"" + title + "\"");
			EndMission();
		}

		public void Fail () {
			Debug.Log("Failed mission \"" + title + "\"");
			EndMission();
		}

		void EndMission () {
			ended = true;
			OnMissionEnded.Invoke();
		}

	}

}
