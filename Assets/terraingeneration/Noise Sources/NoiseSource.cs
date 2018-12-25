using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseSource {

	[System.Serializable]
	public struct NoiseSettings {
		public float scale;
		[Range(0f, 1f)] public float strength;
	}

	public NoiseSettings settings;
	[HideInInspector] public Vector2 offset;

	public abstract float Evaluate (float x, float y);

	protected void ScaleAndOffset (ref float x, ref float y) {
		x = (x + offset.x) / settings.scale;
		y = (y + offset.y) / settings.scale;
	}

}
