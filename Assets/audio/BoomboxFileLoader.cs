using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class BoomboxFileLoader : MonoBehaviour {

	[SerializeField] BoomboxControllerScript controllerScript;
	
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
			if(!IsFileType(fileName, ".meta")){
				if(TryGetAudioType(fileName, out var audioType)){
					StartCoroutine(FileLoader(info, audioType));
				}
			}
		}
	}

	IEnumerator FileLoader(FileInfo info, AudioType audioType){
		string fullFileName = info.FullName;
		string wwwFileName = "file://" + fullFileName;
        var www = UnityWebRequestMultimedia.GetAudioClip(wwwFileName, audioType);
        yield return www.SendWebRequest();
        if(www.isNetworkError){
            Debug.LogError(www.error);
            yield break;
        }
        var clip = DownloadHandlerAudioClip.GetContent(www);
        if(clip != null){
            clip.name = info.Name;
            controllerScript.AddAudioClip(clip);
        }
	}

    bool IsFileType (string fullFileName, string fileExtension) {
        try{
            for(int i=0; i<fileExtension.Length; i++){
                var fileChar = fullFileName[fullFileName.Length - 1 - i];
                var extensionChar = fileExtension[fileExtension.Length - 1 - i];
                if(fileChar != extensionChar){
                    return false;
                }
            }
        }catch(System.Exception e){
            Debug.LogError(e);
            return false;
        }
        return true;
    }

    bool TryGetAudioType (string fileName, out AudioType outputAudioType) {
        outputAudioType = default;
        if(IsFileType(fileName, ".ogg")){
            outputAudioType = AudioType.OGGVORBIS;
        }else if(IsFileType(fileName, ".wav")){
            outputAudioType = AudioType.WAV;
        }else if(IsFileType(fileName, ".mp3")){
            outputAudioType = AudioType.MPEG;
        }else{
            return false;
        }
        return true;
    }

}
