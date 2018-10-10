using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerScript : MonoBehaviour {

	public AudioGUIScript audioGUI;
	public FilterReadGUIScript perFilterGUI;
	public NotQuiteLineDisplayScript waveformDisplay;
	public NotQuiteLineDisplayScript fourierDisplay;
	public VectorScopeScript vectorScope;

	private int dataLength;
	private int numberOfChannels;
	private float maximum;
	private int filterCalls;

	private float fixedTimer;

	public int waveformSampleLength;
	private float[] waveformSampleLeft;
	private float[] waveformSampleRight;
	private bool waveformSampleReadyToSend;
	private int waveformSampleIndex;

	public int fourierMinFreq;
	public int fourierMaxFreq;
	public int fourierMinSampleLength;
	public int fourierResolutionPerOctave;
	public float fourierUpdateInterval;

	private float fourierUpdateTimer;
	private float[] fourierFrequencies;
	private int fourierSampleLength;
	private float[] fourierSampleLeft;
	private float[] fourierSampleRight;
	private bool fourierSampleReady;
	private int fourierSampleIndex;
	private Vector2[][] baseVecs;

	private List<float[]> leftChannels;
	private List<float[]> rightChannels;

	private bool recordData;

	void Start(){
		ResetWaveformComponents();
		ResetFourierComponents();
		leftChannels = new List<float[]>();
		rightChannels = new List<float[]>();
		recordData = true;
	}
	
	void Update(){
		if(Time.timeScale > 0f){
			vectorScope.GiveDualChannelData(GetFullArray(leftChannels), GetFullArray(rightChannels));
			leftChannels.Clear();
			rightChannels.Clear();
			if(waveformSampleReadyToSend){
				//waveformDisplay.GiveLinearData(waveformSample, false);
				//waveformDisplay.GiveLinearData(waveformSampleLeft, false);
				waveformDisplay.GiveDualChannelData(waveformSampleLeft, waveformSampleRight, false);
				waveformSampleReadyToSend = false;
				waveformSampleIndex = 0;
			}
			if(fourierSampleReady && fourierUpdateTimer >= fourierUpdateInterval){
				//float[] fourierData = GetFourierData(fourierSample);
				//fourierDisplay.GiveLinearData(fourierData, true);
				float[] fourierDataLeft = GetFourierData(fourierSampleLeft);
				float[] fourierDataRight = GetFourierData(fourierSampleRight);
				fourierDisplay.GiveDualChannelData(fourierDataLeft, fourierDataRight, true);
				fourierSampleReady = false;
				fourierSampleIndex = 0;
				fourierUpdateTimer -= fourierUpdateInterval;
			}
			fourierUpdateTimer += Time.deltaTime;
			recordData = true;
		}else{
			recordData = false;
		}
	}

	void FixedUpdate(){
		fixedTimer += Time.fixedDeltaTime;
		if(fixedTimer >= 1f){
			UpdatePerFilterGUIDisplay();
			fixedTimer = 0f;
		}
	}

	public void ResetWaveformComponents(){
		//Debug.LogWarning("resetting waveform components");
		waveformSampleReadyToSend = false;
		waveformSampleIndex = 0;
		waveformSampleLeft = new float[waveformSampleLength];
		waveformSampleRight = new float[waveformSampleLength];
	}

	public void ResetFourierComponents(){
		//Debug.LogWarning("resetting fourier components");
		fourierSampleLength = AudioSettings.outputSampleRate / fourierMinFreq;
		if(fourierSampleLength < fourierMinSampleLength) fourierSampleLength = fourierMinSampleLength;
		fourierSampleReady = false;
		fourierSampleIndex = 0;
		fourierSampleLeft = new float[fourierSampleLength];
		fourierSampleRight = new float[fourierSampleLength];
		InitializeFourierFrequencies();
		InitializeVectorCache();
	}

	void OnAudioFilterRead(float[] data, int channels){
		if(recordData){
			UpdatePerFilterGUIValues(data, channels);
			UpdateWaveformDisplayData(data, channels);
			UpdateFourierSampleData(data, channels);
			UpdateChannelData(data, channels);
		}
	}

	private void UpdatePerFilterGUIValues(float[] data, int channels){
		filterCalls++;
		dataLength = data.Length;
		numberOfChannels = channels;
		for(int i=0; i<data.Length; i+=channels){
			for(int j=0; j<channels; j++){
				float currentVal = data[i+j];
				if(currentVal > maximum) maximum = currentVal;
			}
		}
	}

	private void UpdatePerFilterGUIDisplay(){
		perFilterGUI.SetDataLength(dataLength);
		perFilterGUI.SetChannels(numberOfChannels);
		perFilterGUI.SetMaximum(maximum);
		perFilterGUI.SetCallsPerSecond(filterCalls);
		dataLength = 0;
		numberOfChannels = 0;
		maximum = 0;
		filterCalls = 0;
	}

	private void UpdateWaveformDisplayData(float[] data, int channels){
		if(waveformSampleLeft == null) return;
		if(waveformSampleRight == null) return;
		if(!waveformSampleReadyToSend){
			for(int i=0; i<data.Length; i+=channels){
				if(waveformSampleIndex < waveformSampleLength){
					waveformSampleLeft[waveformSampleIndex] = data[i];
					waveformSampleRight[waveformSampleIndex] = data[i+1];
				}else{
					waveformSampleReadyToSend = true;
					return;
				}
				waveformSampleIndex++;
			}
		}
	}

	private void UpdateFourierSampleData(float[] data, int channels){
		if(fourierSampleLeft == null) return;
		if(fourierSampleRight == null) return;
		if(!fourierSampleReady){
			for(int i=0; i<data.Length; i+=channels){
				if(fourierSampleIndex < fourierSampleLength){
					fourierSampleLeft[fourierSampleIndex] = data[i];
					fourierSampleRight[fourierSampleIndex] = data[i+1];
				}else{
					fourierSampleReady = true;
					return;
				}
				fourierSampleIndex++;
			}
		}
	}

	private float[] GetFourierData(float[] sampleData){
		float[] fourierOutput = new float[baseVecs.Length];
		for(int i=0; i<baseVecs.Length; i++){			// i => cycle lengths
			int cycleLength = baseVecs[i].Length;
			Vector2 sum = Vector2.zero;
			for(int j=0; j<sampleData.Length; j++){		// j => data
				sum += baseVecs[i][j % cycleLength] * sampleData[j];
			}
			float mag = sum.magnitude;
			fourierOutput[i] = mag;
		}
		return fourierOutput;
	}

	private void InitializeFourierFrequencies(){
		float minMaxFreqRatio = Mathf.Log((float)fourierMaxFreq / (float)fourierMinFreq) / Mathf.Log(2);
		float ratioBetweenFrequencies = Mathf.Pow(2, 1f/(float)fourierResolutionPerOctave);
		fourierFrequencies = new float[1 + (int)(fourierResolutionPerOctave * minMaxFreqRatio)];
		for(int i=0; i<fourierFrequencies.Length; i+=fourierResolutionPerOctave){
			fourierFrequencies[i] = fourierMinFreq * Mathf.Pow(2, i/fourierResolutionPerOctave);
			for(int j=1; j<fourierResolutionPerOctave; j++){
				if(i+j >= fourierFrequencies.Length) break;
				fourierFrequencies[i+j] = fourierFrequencies[i] * Mathf.Pow(ratioBetweenFrequencies, j);
			}
		}
	}

	private void InitializeVectorCache(){
		baseVecs = new Vector2[fourierFrequencies.Length][];
		float twoPI = 2f * Mathf.PI;
		for(int i=0; i<fourierFrequencies.Length; i++){
			int cycleLength = (int)((float)AudioSettings.outputSampleRate / fourierFrequencies[i]);
			baseVecs[i] = new Vector2[cycleLength];
			for(int j=0; j<cycleLength; j++){
				float pos = twoPI * (float)j / (float)cycleLength;
				baseVecs[i][j] = new Vector2(Mathf.Cos(pos), Mathf.Sin(pos));
			}
		}
	}

	private void UpdateChannelData(float[] data, int channels){
		if(leftChannels == null || rightChannels == null) return;
		int length = data.Length / channels;
		float[] newLeft = new float[length];
		float[] newRight = new float[length];
		int channelOffset;
		if(channels < 2) channelOffset = 0;
		else channelOffset = 1;
		int leftRightIndex = 0;
		for(int i=0; i<data.Length; i+=channels){
			newLeft[leftRightIndex] = data[i];
			newRight[leftRightIndex] = data[i+channelOffset];
			leftRightIndex++;
		}
		leftChannels.Add(newLeft);
		rightChannels.Add(newRight);
	}

	private float[] GetFullArray(List<float[]> arrayList){
		int count = arrayList.Count;
		int length = 0;
		for(int i=0; i<count; i++){
			length += arrayList[i].Length;
		}
		float[] output = new float[length];
		for(int i=0; i<count; i++){
			for(int j=0; j<arrayList[i].Length; j++){
				output[i+j] = arrayList[i][j];
			}
		}
		return output;
	}

}
