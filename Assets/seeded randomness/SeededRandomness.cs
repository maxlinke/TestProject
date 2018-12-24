using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeededRandomness : MonoBehaviour {

	[SerializeField] bool seedFromString;
	[SerializeField] string stringSeed;
	[SerializeField] int seed;

	[SerializeField] KeyCode keySysRng;
	[SerializeField] KeyCode keyUnityRng;

	System.Random sysRng;

	void Start () {
		if(seedFromString && (stringSeed.Length > 0)){
			seed = stringSeed.GetHashCode();
		}
		sysRng = new System.Random(seed);
	}
	
	void Update () {
		if(Input.GetKeyDown(keySysRng)){
			Debug.Log("sys : " + ((float)sysRng.NextDouble()).ToString());
		}
		if(Input.GetKeyDown(keyUnityRng)){
			Debug.Log("unity : " + UnityEngine.Random.value.ToString());
		}
	}
}
