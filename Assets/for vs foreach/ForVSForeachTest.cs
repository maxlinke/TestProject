using System.Collections.Generic;
using UnityEngine;

public class ForVSForeachTest : MonoBehaviour {

    public enum Mode {
        ForEach,
        ForWithCountCall,
        ForWithoutCountCall
    }

    class FVFETestObject{
        public readonly int number;
        public FVFETestObject(int number){
            this.number = number;
        }
    }

    [SerializeField] Mode mode = Mode.ForEach;
    [SerializeField, Range(0, 24)] int expIterations = 10;

	[System.NonSerialized] public int output = 0;

    List<FVFETestObject> objects;

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
        int iterations = 2 << expIterations;
        switch(mode){
            case Mode.ForEach:
                for(int i=0; i<iterations; i++){
                    foreach(FVFETestObject o in objects){
                        output += o.number;
                    }
                }
                break;
            case Mode.ForWithCountCall:
                for(int i=0; i<iterations; i++){
                    for(int j=0; j<objects.Count; j++){
                        output += objects[j].number;
                    }
                }
                break;
            case Mode.ForWithoutCountCall:
                for(int i=0; i<iterations; i++){
                    int count = objects.Count;
                    for(int j=0; j<count; j++){
                        output += objects[j].number;
                    }
                }
                break;
            default:
                Debug.LogError("unsupported mode-integer. only 0 or 1 allowed");
                break;
        }
	}

}
