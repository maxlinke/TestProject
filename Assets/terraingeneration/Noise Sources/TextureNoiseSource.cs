using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextureNoiseSource : NoiseSource {

	public Texture2D texture;

	public override float Evaluate (float x, float y) {
		ScaleAndOffset(ref x, ref y);
		x = Mathf.Repeat(x, 1f);
		y = Mathf.Repeat(y, 1f);
		Color col = texture.GetPixelBilinear(x, y);
		float lum = 0.299f * col.r + 0.587f * col.g + 0.115f * col.b;
		return settings.strength * ((2f * lum) - 1f);
	}

}
