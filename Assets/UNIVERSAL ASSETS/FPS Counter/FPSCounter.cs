using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

	[SerializeField] Text textField = default;
	[SerializeField, Range(0, 100)] int smoothing = 10;
	[SerializeField] bool updateEveryFrame = true;

	float[] timeDeltas;
	int rollingIndex;

	void Start () {
		timeDeltas = new float[smoothing+1];
	}
	
	void Update () {
		RecordCurrentValue();
		bool updateNow = updateEveryFrame || rollingIndex == 0;
		if(updateNow){
			UpdateDisplay();
		}
	}

	private void RecordCurrentValue(){
		timeDeltas[rollingIndex] = Time.unscaledDeltaTime;
		rollingIndex = (rollingIndex + 1) % timeDeltas.Length;
	}

	private void UpdateDisplay(){
		float sum = 0f;
		for(int i=0; i<timeDeltas.Length; i++){
			sum += timeDeltas[i];
		}
		float average = sum / timeDeltas.Length;
		textField.text = ((int)(1f / average)).ToString();
	}
}
