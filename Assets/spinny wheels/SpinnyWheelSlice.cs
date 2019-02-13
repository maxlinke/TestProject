using UnityEngine;

[System.Serializable]
public class SpinnyWheelSlice {
	
	[SerializeField, Range(1, SpinnyWheelConfig.SEGMENTS_PER_WHEEL)] int numberOfSegments;
	[SerializeField] SpinnyWheelEffect effect;

	[System.NonSerialized] public Color color;

	public int NumberOfSegments { get { return numberOfSegments; } }
	public SpinnyWheelEffect Effect { get { return effect; } }

}
