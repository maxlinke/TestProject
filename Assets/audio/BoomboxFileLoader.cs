using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BoomboxFileLoader : MonoBehaviour {

	[SerializeField]
	BoomboxControllerScript controllerScript;

	void Start () {
		
	}
	
	void Update () {
		if(controllerScript.IsInitialized()){
			ReadAndAddFiles();
			this.enabled = false;
		}
	}

	void ReadAndAddFiles(){
		string path = Application.dataPath + "/StreamingAssets/Music";
		FileInfo[] files = new DirectoryInfo(path).GetFiles();
		foreach(FileInfo info in files){
			string fileName = info.FullName;
			if(!IsMetaFile(fileName)){
				if(IsOggFile(fileName)){
					StartCoroutine("FileLoader", info);
				}
			}
		}
	}

	IEnumerator FileLoader(FileInfo info){
		string fullFileName = info.FullName;
		string wwwFileName = "file://" + fullFileName;
		WWW www = new WWW(wwwFileName);
		yield return www;
		AudioClip clip = www.GetAudioClip();
		clip.name = info.Name;
		controllerScript.AddAudioClip(clip);
	}

	bool IsMetaFile(string fileName){
		return fileName.Contains(".meta");
	}

	bool IsOggFile(string fileName){
		return fileName.Contains(".ogg");
	}

}
