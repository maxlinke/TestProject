using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableTerrain : MonoBehaviour {

	//TODO take impact into account (convert to weight and convert that to deformationdepth)
	//don't deform additively and don't un-deform (new height must be lower than old one otherwise ignore...)
	//also try a bigger (more resolution, same size) heightmap and make a function to lower an entire area...
	//i should also make the terrain completely non-static since all this could change

	//ORRR!!!
	//have a basemesh that is used for collision and used in modified form for rendering
	//the rendering variant is just extruded upwards (maybe have a control texture (or even vertex colours))
	//and with each collision stay of the collision mesh the rendering mesh is modified...
	//i can get the triangle index, the bary coords and everything else so that's good...

	[SerializeField] Terrain terrain;
	[SerializeField] float maxDeformationDepth;
	[Tooltip("In kg")] [SerializeField] float maxDeformationWeight;

	TerrainData terrainData;
	float[,] origHeights;

	void Start () {
		if(terrain == null){
			throw new UnityException("Terrain is not referenced!");
		}else{
			terrainData = terrain.terrainData;
            // vvv terraindata.heightmapwidth and -height were replaced by resolution. no idea if that's correct or what...
			origHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);		//from 0 to 1
//			Debug.Log("height: " + terrainData.heightmapHeight);			//65
//			Debug.Log("width: " + terrainData.heightmapWidth);				//65
//			Debug.Log("resolution: " + terrainData.heightmapResolution);	//65
//			Debug.Log("scale: " + terrainData.heightmapScale);				//(0.8, 10, 0.8)
//			Debug.Log("sx : " + terrainData.heightmapScale.x);				//0.8 is actually 0.78125, which is 50/64
		}
	}

	void Reset () {
		this.terrain = GetComponent<Terrain>();
	}

	void OnDestroy () {
		if(terrainData != null){
			terrainData.SetHeights(0, 0, origHeights);
		}
	}

	void OnCollisionEnter (Collision collision) {
		if(terrainData != null){
			for(int i=0; i<collision.contacts.Length; i++){
				int x, y;
				WorldPointToHeightmapCoords(collision.contacts[i].point, out x, out y);
				x = Mathf.Clamp(x, 0, terrainData.heightmapResolution - 1);
				y = Mathf.Clamp(y, 0, terrainData.heightmapResolution - 1);
				terrainData.SetHeights(
					x, 
					y, 
					new float[,] {{origHeights[y, x] - (maxDeformationDepth / terrainData.heightmapScale.y)}}
//					new float[,] {{origHeights[x, y]}}		// <-- the wrong way around!!!
				);
			}
		}
	}

	//could be useful to find out if desired deformation depth is even possible (e.g. if the terrain is at 0 height at some point there won't be any deformation...)
	void GetMinMaxOfHeightmap (float[,] heightmap, out float min, out float max) {
		min = Mathf.Infinity;
		max = Mathf.NegativeInfinity;
		for(int i=0; i<heightmap.GetLength(0); i++){
			for(int j=0; j<heightmap.GetLength(1); j++){
				float height = heightmap[i,j];
				if(height > max) max = height;
				if(height < min) min = height;
			}
		}
	}

	void WorldPointToHeightmapCoords (Vector3 worldPoint, out int xPos, out int yPos) {
		Vector3 point = worldPoint - terrain.transform.position;		//no transformpoint because rotation and scale are ignored by terrains
		Vector3 hmScale = terrainData.heightmapScale;
		Vector3 hmPoint = new Vector3(point.x / hmScale.x, point.y / hmScale.y, point.z / hmScale.z);
		xPos = Mathf.FloorToInt(hmPoint.x + 0.5f);
		yPos = Mathf.FloorToInt(hmPoint.z + 0.5f);
	}

}
