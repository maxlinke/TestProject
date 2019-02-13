using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wheel Config", menuName = "SpinnyWheels/SpinnyWheelConfig")]
public class SpinnyWheelConfig : ScriptableObject {

	public const int SEGMENTS_PER_WHEEL = 12;

	[SerializeField] Color desaturatedBackgroundColor = Color.white;
	[SerializeField] Color saturatedBackgroundColor = Color.white;
	[SerializeField] List<SpinnyWheelSlice> slices = new List<SpinnyWheelSlice>();

	public Color DesaturatedBackgroundColor { get { return desaturatedBackgroundColor; } }
	public Color SaturatedBackgroundColor { get { return saturatedBackgroundColor; } }
	public SpinnyWheelSlice[] Slices { get { return slices.ToArray(); } }

	public bool IsValid () {
		return (GetTotalNumberOfSegments() == SEGMENTS_PER_WHEEL);
	}

	int GetTotalNumberOfSegments () {
		int totalSegmentCount = 0;
		if(slices != null){
			for(int i=0; i<slices.Count; i++){
				totalSegmentCount += slices[i].NumberOfSegments;
			}
		}
		return totalSegmentCount;
	}

}
