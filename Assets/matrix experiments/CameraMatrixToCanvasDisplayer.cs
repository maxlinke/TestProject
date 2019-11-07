using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMatrixToCanvasDisplayer : MonoBehaviour {

    [SerializeField] Camera cam;
    [SerializeField] Text textField;

    void Update () {
        textField.text = string.Empty;
        // var mat = cam.worldToCameraMatrix;
        var mat = cam.projectionMatrix;
        for(int i=0; i<4; i++){
            var row = mat.GetRow(i);
            for(int j=0; j<4; j++){
                float val = row[j];
                textField.text += (val >= 0 ? $" {val:F3}\t" : $"{val:F3}\t");
            }
            textField.text += "\n";
        }
    }
	
}
