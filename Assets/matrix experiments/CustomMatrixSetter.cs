using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class CustomMatrixSetter : MonoBehaviour {

    Material mat;
    int mvpMatrixID;
    int modelMatrixID;
    int normalMatrixID;

    [SerializeField] Vector4 translation;

    void Awake () {
        var mr = GetComponent<MeshRenderer>();
        if(mr == null){
            Debug.Log($"No MeshRenderer on \"{gameObject.name}\"!");
            return;
        }
        mat = mr.sharedMaterial;
        if(mat == null){
            Debug.Log($"No Material on MeshRenderer of \"{gameObject.name}\"!");
            return;
        }
        mvpMatrixID = Shader.PropertyToID("CustomMVPMatrix");
        modelMatrixID = Shader.PropertyToID("CustomModelMatrix");
        normalMatrixID = Shader.PropertyToID("CustomNormalMatrix");
    }

    //the render matrix' z-axis is flipped according to openGL

    void OnWillRenderObject () {
        if(mat == null){
            Awake();
            return;
        }

        var cam = Camera.current;

        // var viewMatrix = cam.worldToCameraMatrix;
        // var projectionMatrix = cam.projectionMatrix;
        // var flipMatrix = new Matrix4x4(
        //     new Vector4(1, 0, 0, 0),
        //     new Vector4(0, -1, 0, 0),
        //     new Vector4(0, 0, 1, 0),
        //     new Vector4(0, 0, 0, 1));
        // var zFlipMatrix = new Matrix4x4(
        //     new Vector4(1, 0, 0, 0),
        //     new Vector4(0, 1, 0, 0),
        //     new Vector4(0, 0, -1, 0),
        //     new Vector4(0, 0, 0, 1));
        // var cameraMatrix = flipMatrix * projectionMatrix * viewMatrix;

        var viewMatrix = cam.worldToCameraMatrix;
        var projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
        var cameraMatrix = projectionMatrix * viewMatrix;
        
        var translationMatrix = Matrix4x4.Translate(transform.position);
        var rotationMatrix = Matrix4x4.Rotate(transform.rotation);
        var scaleMatrix = Matrix4x4.Scale(transform.lossyScale);

        Matrix4x4 modelMatrix = translationMatrix * rotationMatrix * scaleMatrix; 
        // modelMatrix.SetColumn(3, translation);

        //the z-buffer is flipped for some reason...

        Matrix4x4 mvpMatrix = cameraMatrix * modelMatrix;
        Matrix4x4 normalMatrix = Matrix4x4.identity;

        mat.SetMatrix(mvpMatrixID, mvpMatrix);
        mat.SetMatrix(modelMatrixID, modelMatrix);
        mat.SetMatrix(normalMatrixID, normalMatrix);
    }
	
}
