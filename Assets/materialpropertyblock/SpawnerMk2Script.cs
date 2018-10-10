using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerMk2Script : MonoBehaviour {

	[Header("Settings")]
	[SerializeField] bool updateMaterials;
	[SerializeField] int fieldSize;
	[SerializeField] bool useMaterialPropertyBlock;
	[SerializeField] TestObjectType testObjectType;

	[Header("Materials")]
	[SerializeField] Material normalMaterial;
	[SerializeField] Material mpbMaterial;

	[Header("Prefabs")]
	[SerializeField] GameObject cubePrefab;
	[SerializeField] GameObject spherePrefab;
	[SerializeField] GameObject quadPrefab;

	MeshRenderer[,] meshRenderers;
	MaterialPropertyBlock mpb;
	float[] xOffsets;
	float[] zOffsets;

	void Start(){
		GameObject prefab = GetPrefab(testObjectType);
		meshRenderers = new MeshRenderer[fieldSize,fieldSize];
		mpb = new MaterialPropertyBlock();
		xOffsets = new float[fieldSize];
		zOffsets = new float[fieldSize];
		for(int x=0; x<fieldSize; x++){
			xOffsets[x] = ((float)x / (float)fieldSize);
			for(int z=0; z<fieldSize; z++){
				zOffsets[z] = ((float)z / (float)fieldSize);
				GameObject obj = Instantiate(prefab, this.transform.position, Quaternion.identity, this.transform) as GameObject;
				obj.transform.position += new Vector3(x, 0f, z);
				MeshRenderer mr = obj.GetComponent<MeshRenderer>();
				if(useMaterialPropertyBlock){
					mr.material = mpbMaterial;
				}else{
					mr.material = normalMaterial;
				}
				meshRenderers[x,z] = mr;
			}
		}
	}

	void Update(){
		if(updateMaterials){
			if(useMaterialPropertyBlock){
				for(int x=0; x<fieldSize; x++){
					for(int z=0; z<fieldSize; z++){
						MeshRenderer mr = meshRenderers[x, z];
						Color col = new Color(xOffsets[x], zOffsets[z], Time.time - Mathf.Floor(Time.time));
						mpb.SetColor("_Color", col);
						mr.SetPropertyBlock(mpb);
					}
				}
			}else{
				for(int x=0; x<fieldSize; x++){
					for(int z=0; z<fieldSize; z++){
						MeshRenderer mr = meshRenderers[x, z];
						Color col = new Color(xOffsets[x], zOffsets[z], Time.time - Mathf.Floor(Time.time));
						mr.material.SetColor("_Color", col);
					}
				}
			}
		}
	}

	GameObject GetPrefab(TestObjectType type){
		switch(type){
		case TestObjectType.SPHERES:
			return spherePrefab;
		case TestObjectType.CUBES:
			return cubePrefab;
		case TestObjectType.QUADS:
			return quadPrefab;
		default:
			return null;
		}
	}

	enum TestObjectType{
		SPHERES,
		CUBES,
		QUADS
	}
}
