using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotQuiteLineDisplayScript : MonoBehaviour {

	public enum DrawMode{
		LEFT, RIGHT, BOTH, SUM
	};

	public Image image;
	public Color32 lineColor;
	public Color32 lineColorAlt;
	private Texture2D tex;
	private int width, height;
	private int width2, height2;
	private Color32[] pixels;
	public DrawMode drawMode;

	/*
		loop template

		for(int y=0; y<height; y++){
			for(int x=0; x<width; x++){
				int pos = y * width + x;
				//do stuff
			}
		}
	*/

	void Start(){
		tex = image.sprite.texture;
		width = tex.width;
		height = tex.height;
		width2 = width/2;
		width2 = width2 * 1;	//to make the compiler shut up
		height2 = height/2;
		pixels = new Color32[width * height];
		InitializeDisplay();
	}
	
	void Update(){
		
	}

	private void SetImage(){
		tex.SetPixels32(pixels);
		tex.Apply(false);
	}

	private void MirrorTopToBottom(){
		for(int x=0; x<width; x++){
			for(int y=0; y<height2; y++){
				int origPos = y * width + x;
				int newPos = (height - y - 1) * width + x;
				Color32 origColor = pixels[origPos];
				Color32 newColor = pixels[newPos];
				pixels[origPos] = newColor;
				pixels[newPos] = origColor;
			}
		}
	}

	private void InitializeDisplay(){
		for(int y=0; y<height; y++){
			for(int x=0; x<width; x++){
				int pos = y * width + x;
				if(y == height/2) pixels[pos] = lineColor;
				else pixels[pos] = new Color32(0,0,0,0);
			}
		}
		SetImage();
	}

	private void ClearAllPixels(){
		for(int i=0; i<pixels.Length; i++){
			pixels[i] = new Color32(0,0,0,0);
		}
	}

	private void DrawLinearData(float[] data, bool normalize, Color32 drawColor){
		float absMax;
		if(normalize){
			absMax = 0;
			for(int i=0; i<data.Length; i++){
				float val = Mathf.Abs(data[i]);
				if(val > absMax) absMax = val;
			}
		}else{
			absMax = 1;
		}
		int lastProperY = Mathf.Clamp(height2 - (int)((height2 * data[0]) / absMax), 0, height-1);
		for(int x=0; x<width; x++){
			int positionInData = (data.Length * x) / width;
			int properY = height2 - (int)((height2 * data[positionInData]) / absMax);
			properY = Mathf.Clamp(properY, 0, height-1);

			int bottom, top;
			if(lastProperY < properY){
				bottom = lastProperY;
				top = properY;
			}else{
				bottom = properY;
				top = lastProperY;
			}
			for(int y=bottom; y<=top; y++){
				int pos = y * width + x;
				pixels[pos] = drawColor;
			}

			lastProperY = properY;
		}
	}

	public void GiveLinearData(float[] data, bool normalize){
		/*
		float absMax;
		if(normalize){
			absMax = 0;
			for(int i=0; i<data.Length; i++){
				float val = Mathf.Abs(data[i]);
				if(val > absMax) absMax = val;
			}
		}else{
			absMax = 1;
		}
		int lastProperY = height2 - (int)((height2 * data[0]) / absMax);
		for(int x=0; x<width; x++){
			int positionInData = (data.Length * x) / width;
			int properY = height2 - (int)((height2 * data[positionInData]) / absMax);
			properY = Mathf.Clamp(properY, 0, height-1);
			for(int y=0; y<height; y++){
				int pos = y * width + x;
				if(y == properY || (y > lastProperY && y < properY) || (y < lastProperY && y > properY)) pixels[pos] = lineColor;
				else pixels[pos] = new Color32(0,0,0,0);
			}
			lastProperY = properY;
		}
		*/
		ClearAllPixels();
		DrawLinearData(data, normalize, lineColor);
		MirrorTopToBottom();
		SetImage();
	}

	public void GiveDualChannelData(float[] left, float[] right, bool normalize){
		ClearAllPixels();
		switch(drawMode){
		case DrawMode.LEFT:
			DrawLinearData(left, normalize, lineColor);
			break;
		case DrawMode.RIGHT:
			DrawLinearData(right, normalize, lineColor);
			break;
		case DrawMode.BOTH:
			DrawLinearData(right, normalize, lineColorAlt);
			DrawLinearData(left, normalize, lineColor);
			break;
		case DrawMode.SUM:
			float[] sum = new float[Mathf.Max(left.Length, right.Length)];
			for(int i=0; i<sum.Length; i++){
				sum[i] = (left[i] + right[i] ) / 2f;
			}
			DrawLinearData(sum, normalize, lineColor);
			break;
		}
		MirrorTopToBottom();
		SetImage();
	}

}
