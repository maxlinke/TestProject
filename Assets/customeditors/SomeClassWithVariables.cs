using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SomeClassWithVariables : MonoBehaviour {

	public bool executeAction;
	[SerializeField] UnityEvent someEvent;
	[Range(-1f, 1f)] [SerializeField] float someFloat;

	[Tooltip("Should the action be exeuted every update?")]
	public bool everyUpdate;
	[Tooltip("How long should the interval between actions be?")]
	public float waitTime;

	int counter;
	float nextExecute;

	//there's no bool here, just a toggle group in the editor
	[SerializeField] int firstInt;
	[SerializeField] int secondInt;
	[SerializeField] int thirdInt;

	[SerializeField] bool aPrivateBool;

	void Update () {
		if(executeAction){
			if(everyUpdate || (Time.time > nextExecute)){
				someEvent.Invoke();
				if(!everyUpdate){
					nextExecute = Time.time + waitTime;
				}
			}
		}
	}

	public void IncrementCounter () {
		counter++;
	}

	public void LogHexCounter () {
		Debug.Log(counter.ToString("X") + " | " + someFloat);
	}

}
