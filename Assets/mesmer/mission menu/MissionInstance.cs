using UnityEngine;

namespace Mesmer {

	public class MissionInstance {

		public const float runningOutWarningTime = 10f;

		public readonly Mission mission;
		public readonly float startTime;

		public float endTime { get { return startTime + mission.duration; } }
		public float timeLeft { get { return endTime - Time.time; } }
		public float timeLeft01 { get { return timeLeft / mission.duration; } }
		public bool runningOut { get { return timeLeft < runningOutWarningTime; } }

		bool m_unread;

		public bool unread {
			get {
				return m_unread;
			} set {
				if(value){
					Debug.LogWarning("someone tried to set a mission instance to unread, which is not allowed outside the constuctor...");
				}else{
					m_unread = false;
				}
			}
		}

		public MissionInstance (Mission mission) {
			this.mission = mission;
			this.startTime = Time.time;
			this.m_unread = true;
		}

	}

}