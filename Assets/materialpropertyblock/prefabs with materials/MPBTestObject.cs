using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPBTestObject : MonoBehaviour {

	[SerializeField] MeshRenderer mr;
	[SerializeField] bool useMaterialPropertyBlock;

	public float xOffset{
		get{
			return _xOffset;
		}set{
			_xOffset = value;
		}
	}

	public float zOffset{
		get{
			return _zOffset;
		}set{
			_zOffset = value;
		}
	}

	MaterialPropertyBlock mpb;
	float _xOffset;
	float _zOffset;

	float cycleTime = 10f;

	void Start(){
		if(useMaterialPropertyBlock){
			mpb = new MaterialPropertyBlock();
		}
	}
	
	void Update(){
		Color col = GetSinColor();
		if(useMaterialPropertyBlock){
			mr.GetPropertyBlock(mpb);
			mpb.SetColor("_Color", col);
			mr.SetPropertyBlock(mpb);
		}else{
			//mr.material.SetColor("_Color", col);
			mr.material.color = col;
		}
	}

	Color GetConstantColor(){
		float r = xOffset;
		float g = zOffset;
		float b = (Time.time / cycleTime) % 1;
		return new Color(r, g, b);
	}

	Color GetHardEdgeColor(){
		float r = ((Time.time / cycleTime) + xOffset) % 1;
		float g = ((Time.time / cycleTime) + zOffset) % 1;
		float b = (Time.time / cycleTime) % 1;
		return new Color(r, g, b);
	}

	Color GetSinColor(){
		float r = Mathf.Sin(2f * Mathf.PI * ((Time.time / cycleTime) + xOffset));
		float g = Mathf.Sin(2f * Mathf.PI * ((Time.time / cycleTime) + zOffset));
		float b = Mathf.Sin(2f * Mathf.PI * (Time.time / cycleTime));
		r += 1;
		g += 1;
		b += 1;
		r /= 2f;
		g /= 2f;
		b /= 2f;
		return new Color(r, g, b);
	}
}
