using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterReadGUIScript : MonoBehaviour {

	public Text datalength;
	public Text channels;
	public Text maximum;
	public Text calls;

	private int displayPrecision = 10000;

	public void SetDataLength(int value){
		datalength.text = value.ToString();
	}

	public void SetChannels(int value){
		channels.text = value.ToString();
	}

	public void SetMaximum(float value){

		int intPrecision = (int)(value * displayPrecision);
		int beforeComma = intPrecision / displayPrecision;
		int afterComma = intPrecision % displayPrecision;
		maximum.text = beforeComma.ToString() + "." + afterComma.ToString();
	}

	public void SetCallsPerSecond(float value){
		calls.text = value.ToString();
	}

}
