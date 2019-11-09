using UnityEngine;

[ExecuteAlways]
public class CustomMatrixSetter : MonoBehaviour {

    Material mat;
    MeshRenderer mr;
    MaterialPropertyBlock mpb;
    int mvpMatrixID;
    int modelMatrixID;
    int normalMatrixID;
    int inverseModelMatrixID;

    void Awake () {
        mr = GetComponent<MeshRenderer>();
        if(mr == null){
            Debug.Log($"No MeshRenderer on \"{gameObject.name}\"!");
            return;
        }
        mpb = new MaterialPropertyBlock();

        mat = mr.sharedMaterial;
        if(mat == null){
            Debug.Log($"No Material on MeshRenderer of \"{gameObject.name}\"!");
            return;
        }

        mvpMatrixID = Shader.PropertyToID("CustomMVPMatrix");
        modelMatrixID = Shader.PropertyToID("CustomModelMatrix");
        normalMatrixID = Shader.PropertyToID("CustomNormalMatrix");
        inverseModelMatrixID = Shader.PropertyToID("CustomInverseModelMatrix");
    }

    void OnWillRenderObject () {
        if(mat == null){
            Awake();
            return;
        }

        if(mr == null){
            return;
        }

        if(mpb == null){
            mpb = new MaterialPropertyBlock();
        }

        var translationMatrix = Matrix4x4.Translate(transform.position);
        var rotationMatrix = Matrix4x4.Rotate(transform.rotation);
        var scaleMatrix = Matrix4x4.Scale(transform.lossyScale);
        
        var modelMatrix = translationMatrix * rotationMatrix * scaleMatrix; 
        var inverseModelMatrix = modelMatrix.inverse;

        var cam = Camera.current;
        var viewMatrix = cam.worldToCameraMatrix;
        var projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
       
        var mvpMatrix = projectionMatrix * viewMatrix * modelMatrix;        
        var normalMatrix = (viewMatrix * modelMatrix).inverse.transpose;

        mpb.SetMatrix(mvpMatrixID, mvpMatrix);
        mpb.SetMatrix(modelMatrixID, modelMatrix);
        mpb.SetMatrix(normalMatrixID, normalMatrix);
        mpb.SetMatrix(inverseModelMatrixID, inverseModelMatrix);
        mr.SetPropertyBlock(mpb);
    }
	
}
