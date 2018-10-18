using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructTest : MonoBehaviour {

	[System.Serializable]
	struct StructTestStruct {
		public bool someValue;
	}

	[SerializeField] StructTestStruct s;
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.Return)){
			RefStuff(ref s);
		}
		if(Input.GetKeyDown(KeyCode.Space)){
			NotRefStuff(s);
		}
	}

	void RefStuff (ref StructTestStruct sts) {
		sts.someValue = !sts.someValue;
	}

	void NotRefStuff (StructTestStruct sts) {
		sts.someValue = !sts.someValue;
	}
}
