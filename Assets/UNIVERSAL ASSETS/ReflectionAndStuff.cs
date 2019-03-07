using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class ReflectionAndStuff : MonoBehaviour {

	int counter;

	void Start () {
		
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.A)){
			DoSomething(() => { Debug.Log("hello"); } );
		}else if(Input.GetKeyDown(KeyCode.S)){
			DoSomethingElse(SomethingElseAction);
		}else if(Input.GetKeyDown(KeyCode.D)){
			DoYetAnotherThing(YetAnotherAction);
		}else if(Input.GetKeyDown(KeyCode.F)){
			SomeOtherClass soc = new SomeOtherClass(Random.Range(-10, 10), Random.value > 0.5f, "ASDF");
			foreach(var fieldInfo in soc.GetType().GetFields()){
				var fieldValue = fieldInfo.GetValue(soc);
				if(fieldValue is int){
					Debug.Log("int " + fieldInfo.Name + ": " + ((int)fieldValue).ToString());
				}else if(fieldValue is bool){
					Debug.Log("bool " + fieldInfo.Name + ": " +((bool)fieldValue).ToString());
				}else if(fieldValue is string){
					Debug.Log("string " + fieldInfo.Name + ": " + ((string)fieldValue)); 
				}else{
					var fieldType = fieldValue.GetType();
					Debug.Log(fieldType.ToString() + " " + fieldInfo.Name + ": " + fieldValue.ToString());
				}
			}
		}
	}

	void DoSomething (Action action) {
		action.Invoke();
	}

	void DoSomethingElse (Action<string> action) {
		action.Invoke("dosomethingelse");
	}

	void DoYetAnotherThing (Action<string, int> action) {
		action.Invoke("yetanotherthing", counter);
		counter++;
	}

	void SomethingElseAction (string input) {
		Debug.Log(input);
	}

	void YetAnotherAction (string input, int number) {
		Debug.Log(input + " " + number.ToString());
	}

	class SomeOtherClass {

		public int intField;
		public bool boolField;
		public string stringField;

		public SomeOtherClass (int intVar = 0, bool boolVar = false, string stringVar = "") {
			this.intField = intVar;
			this.boolField = boolVar;
			this.stringField = stringVar;
		}

	}
}
