using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mesmer {

    public class LockPickingMinigame : MonoBehaviour {

        [Header("Components")]
        [SerializeField] LockPickingMinigameWheel wheelPrefab;
        [SerializeField] RectTransform interactableWheelParent;

        [Header("Settings")]
        [SerializeField, Range(1, 10)] int randomRotationsAtInit;
        [SerializeField, Range(1, 10)] int minStepsPerWheel;
        [SerializeField, Range(1, 10)] int maxStepsPerWheel;
        [SerializeField] SimpleAnimation wheelAdvanceAnim;

        LockPickingMinigameWheel wheel1;
        LockPickingMinigameWheel wheel2;
        LockPickingMinigameWheel passiveWheel;

        void Start () {
            Reset();
        }

        void Reset () {
            for(int i=interactableWheelParent.childCount-1; i>=0; i--){
                Destroy(interactableWheelParent.GetChild(i).gameObject);
            }

            passiveWheel = MakeWheel(500);
            passiveWheel.SetColor(0.8f * Color.white + 0.2f * Color.black);
            wheel2 = MakeWheel(400);
            wheel1 = MakeWheel(300);
            wheel1.SetLinkedToOther(passiveWheel, 2);
            wheel2.SetLinkedToOther(passiveWheel, 3);
            
            for(int i=0; i<randomRotationsAtInit; i++){
                wheel1.RotateSteps(Random.Range(1, wheel1.steps), instantly: true);
                wheel2.RotateSteps(Random.Range(1, wheel2.steps), instantly: true);
            }

            LockPickingMinigameWheel MakeWheel (float size) {
                var newWheel = Instantiate(wheelPrefab, interactableWheelParent);
                int newWheelStepCount = Random.Range(minStepsPerWheel, maxStepsPerWheel);
                newWheel.Initialize(newWheelStepCount);
                newWheel.rectTransform.sizeDelta = Vector3.one * size;
                return newWheel;
            }
        }

        void Update () {
            if(Input.GetKeyDown(KeyCode.R)){
                Reset();
            }
            if(Input.GetKeyDown(KeyCode.A)){
                wheel1.RotateSteps(-1);
            }else if(Input.GetKeyDown(KeyCode.D)){
                wheel1.RotateSteps(1);
            }
            if(Input.GetKeyDown(KeyCode.Q)){
                wheel2.RotateSteps(-1);
            }else if(Input.GetKeyDown(KeyCode.E)){
                wheel2.RotateSteps(1);
            }
        }
    
    }

}
