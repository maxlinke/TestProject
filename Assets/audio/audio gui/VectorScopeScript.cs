using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VectorScopeScript : MonoBehaviour {

	public Image image;
	public Color32 lineColor;
	private Texture2D tex;
	private int width, height;
	private int width2, height2;
	private Color32[] pixels;
	private Color32 transparent;

	public float fadeFactor;

	void Start(){
		tex = image.sprite.texture;
		width = tex.width;
		height = tex.height;
		width2 = width/2;
		height2 = height/2;
		pixels = new Color32[width * height];
		transparent = new Color32(0,0,0,0);
		InitializeDisplay();
	}
	
	void FixedUpdate(){
		for(int y=0; y<height; y++){
			for(int x=0; x<width; x++){
				int pos = y * width + x;
				Color32 col = pixels[pos];
				byte r = col.r;
				byte g = col.g;
				byte b = col.b;
				byte a = (byte)(col.a * fadeFactor);
				pixels[pos] = new Color32(r,g,b,a);
			}
		}
		SetImage();
	}

	private void SetImage(){
		tex.SetPixels32(pixels);
		tex.Apply(false);
	}

	private void InitializeDisplay(){
		for(int y=0; y<height; y++){
			for(int x=0; x<width; x++){
				int pos = y * width + x;
				if(y == height2 && x == width2) pixels[pos] = lineColor;
				else pixels[pos] = transparent;
			}
		}
		SetImage();
	}

	public void GiveDualChannelData(float[] left, float[] right){
		for(int i=0; i<left.Length; i++){
			if(i >= left.Length || i >= right.Length) break;
			int x = width2 + (int)(left[i] * width2);
			int y = height2 + (int)(right[i] * height2);
			x = Mathf.Clamp(x, 0, width-1);
			y = Mathf.Clamp(y, 0, height-1);
			int pos = y * width + x;
			pixels[pos] = lineColor;
		}
		SetImage();
	}



}
