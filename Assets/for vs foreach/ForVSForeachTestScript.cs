using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ForVSForeachTestScript : MonoBehaviour {

	private List<FVFETestObject> objects;
	private int mode, iterations;

	void Start () {
		objects = new List<FVFETestObject>();
		objects.Add(new FVFETestObject("paul"));
		objects.Add(new FVFETestObject("mary"));
		objects.Add(new FVFETestObject("peter"));
		objects.Add(null);
		objects.Add(new FVFETestObject(null));
	}
	
	void Update () {
		if(mode == 0){
			for(int i=0; i<iterations; i++){
				int count = objects.Count;
				for(int j=0; j<count; j++){
					//absolutely nothing
				}
			}
		}else if(mode == 1){
			for(int i=0; i<iterations; i++){
				foreach(FVFETestObject o in objects){
					//absolutely nothing
				}
			}
		}else{
			throw new ArgumentException("unsupported mode-integer. only 0 or 1 allowed");
		}
	}

	public void SetModeToFor(){
		mode = 0;
	}

	public void SetModeToForEach(){
		mode = 1;
	}

	public void SetIterations(string stringValue){
		int value;
		if(int.TryParse(stringValue, out value)){
			iterations = value;
		}
	}

}

class FVFETestObject{
	public string name;
	public int id;
	public FVFETestObject(string name){
		this.name = name;
		this.id = UnityEngine.Random.Range(0, int.MaxValue);
	}
}
