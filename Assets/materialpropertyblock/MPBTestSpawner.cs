using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPBTestSpawner : MonoBehaviour {

	[Header("Settings")]
	[SerializeField] int fieldSize;
	[SerializeField] bool useMaterialPropertyBlock;
	[SerializeField] TestObjectType testObjectType;

	[Header("Prefabs")]
	[SerializeField] GameObject normalSpherePrefab;
	[SerializeField] GameObject mpbSpherePrefab;
	[SerializeField] GameObject normalCubePrefab;
	[SerializeField] GameObject mpbCubePrefab;

	void Start(){
		GameObject normalPrefab;
		GameObject mpbPrefab;
		switch(testObjectType){
		case TestObjectType.SPHERES:
			normalPrefab = normalSpherePrefab;
			mpbPrefab = mpbSpherePrefab;
			break;
		case TestObjectType.CUBES:
			normalPrefab = normalCubePrefab;
			mpbPrefab = mpbCubePrefab;
			break;
		default:
			normalPrefab = null;
			mpbPrefab = null;
			break;
		}
		for(int x=0; x<fieldSize; x++){
			float xOff = ((float)x / (float)fieldSize);
			for(int z=0; z<fieldSize; z++){
				float zOff = ((float)z / (float)fieldSize);
				GameObject obj;
				if(useMaterialPropertyBlock){
					obj = Instantiate(mpbPrefab, this.transform.position, Quaternion.identity, this.transform) as GameObject;
				}else{
					obj = Instantiate(normalPrefab, this.transform.position, Quaternion.identity, this.transform) as GameObject;
				}
				obj.transform.position += new Vector3(x, 0f, z);
				MPBTestObject objScript = obj.GetComponent<MPBTestObject>();
				objScript.xOffset = xOff;
				objScript.zOffset = zOff;
			}
		}
	}
	
	void Update(){
		
	}

	enum TestObjectType{
		SPHERES,
		CUBES
	}
}
