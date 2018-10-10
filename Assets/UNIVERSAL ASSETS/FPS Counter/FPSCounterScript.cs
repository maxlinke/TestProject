using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounterScript : MonoBehaviour {

	public Text textField;
	[Range(0, 600)]
	public int smoothing;
	public bool updateEveryFrame;

	private float[] timeDeltas;
	private int rollingIndex;
	private int fps;


	void Start () {
		timeDeltas = new float[smoothing];
	}
	
	void Update () {
		RecordCurrentValue();
		bool updateNow = updateEveryFrame || rollingIndex == 0;
		if(updateNow){
			CalculateFPS();
			DisplayFPS();
		}
	}

	private void RecordCurrentValue(){
		timeDeltas[rollingIndex] = Time.unscaledDeltaTime;
		rollingIndex = (rollingIndex + 1) % timeDeltas.Length;
	}

	private void CalculateFPS(){
		float sum = 0f;
		for(int i=0; i<timeDeltas.Length; i++){
			sum += timeDeltas[i];
		}
		float average = sum / timeDeltas.Length;
		fps = (int)(1f / average);
	}

	private void DisplayFPS(){
		textField.text = fps.ToString();
	}
}
