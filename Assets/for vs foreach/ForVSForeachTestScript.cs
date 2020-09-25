using System.Collections.Generic;
using UnityEngine;

public class ForVSForeachTestScript : MonoBehaviour {

	private List<FVFETestObject> objects;
	private int mode, iterations;

    public int output = 0;

	void Start () {
		objects = new List<FVFETestObject>();
		objects.Add(new FVFETestObject(20));
		objects.Add(new FVFETestObject(-24));
		objects.Add(new FVFETestObject(4));
		objects.Add(new FVFETestObject(-75));
		objects.Add(new FVFETestObject(10));
		objects.Add(new FVFETestObject(25));
		objects.Add(new FVFETestObject(40));
		objects.Add(new FVFETestObject(-19));
		objects.Add(new FVFETestObject(20));
	}
	
	void Update () {
		if(mode == 0){
			for(int i=0; i<iterations; i++){
				int count = objects.Count;
				for(int j=0; j<count; j++){
					output += objects[j].number;
				}
			}
		}else if(mode == 1){
			for(int i=0; i<iterations; i++){
				foreach(FVFETestObject o in objects){
					output += o.number;
				}
			}
		}else{
			Debug.LogError("unsupported mode-integer. only 0 or 1 allowed");
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
    public readonly int number;
	public FVFETestObject(int number){
		this.number = number;
	}
}
