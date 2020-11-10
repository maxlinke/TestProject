using UnityEngine;

public class SuperSimpleXYZMove : MonoBehaviour {

    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float maxAccel = 10f;

    Vector3 velocity;

    void Start () {
        velocity = Vector3.zero;
    }

    void Update () {
        Vector3 input = Vector3.zero;
        input += Dir(KeyCode.W, Vector3.forward);
        input -= Dir(KeyCode.S, Vector3.forward);
        input += Dir(KeyCode.D, Vector3.right);
        input -= Dir(KeyCode.A, Vector3.right);
        input += Dir(KeyCode.E, Vector3.up);
        input -= Dir(KeyCode.Q, Vector3.up);
        if(input.sqrMagnitude > 1f){
            input = input.normalized;
        }
        Vector3 accel = Vector3.zero;
        if(input.sqrMagnitude > 0.01f){
            accel = input * maxAccel;
        }else{
            accel = -velocity.normalized * Mathf.Min(maxAccel, velocity.magnitude / Time.deltaTime);
        }
        velocity += accel * Time.deltaTime;
        if(velocity.sqrMagnitude > (maxSpeed * maxSpeed)){
            velocity = velocity.normalized * maxSpeed;
        }
        transform.position += velocity * Time.deltaTime;

        Vector3 Dir (KeyCode keyCode, Vector3 direction) {
            return Input.GetKey(keyCode) ? direction : Vector3.zero;
        }
    }


	
}
