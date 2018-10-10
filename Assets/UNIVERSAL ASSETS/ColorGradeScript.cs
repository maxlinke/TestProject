using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGradeScript : MonoBehaviour {

	[SerializeField]
	private Material effectMat;

	void OnRenderImage(RenderTexture src, RenderTexture dst){
		Graphics.Blit(src, null, effectMat);
	}

}
