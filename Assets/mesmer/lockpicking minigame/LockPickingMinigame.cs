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
        [SerializeField, Range(1, 4)] int numberOfWheels;
        [SerializeField, Range(1, 10)] int minStepsPerWheel;
        [SerializeField, Range(1, 10)] int maxStepsPerWheel;
        [SerializeField] SimpleAnimation wheelAdvanceAnim;

        Coroutine resetCoroutine;
        List<LockPickingMinigameWheel> activeWheels;
        LockPickingMinigameWheel passiveWheel;

        readonly int[] hackyPrimes = {2, 3, 5, 7, 11, 13, 17, 19, 23}; 
        bool acceptInput;
        int currentlyActiveWheelIndex;

        void Start () {
            activeWheels = new List<LockPickingMinigameWheel>();
            resetCoroutine.Stop(this);
            resetCoroutine = StartCoroutine(Reset());
        }

        IEnumerator Reset () {
            acceptInput = false;

            for(int i=interactableWheelParent.childCount-1; i>=0; i--){
                Destroy(interactableWheelParent.GetChild(i).gameObject);
            }
            activeWheels.Clear();

            int minSize = 3;
            float sizeMultiplier = 100;
            int size = minSize + numberOfWheels;

            passiveWheel = MakeWheel(size * sizeMultiplier);
            passiveWheel.SetColor(0.8f * Color.white + 0.2f * Color.black);
            size--;

            for(int i=0; i<numberOfWheels; i++){
                var wheel = MakeWheel(sizeMultiplier * size);
                wheel.SetLinkedToOther(passiveWheel, hackyPrimes[i]);
                wheel.SetColor(Color.white);
                activeWheels.Add(wheel);
                size--;
            }

            yield return new WaitForSeconds(0.5f);
            
            bool[] wheelsDone = new bool[activeWheels.Count];
            for(int i=0; i<activeWheels.Count; i++){
                wheelsDone[i] = false;
                int direction = 1 - (Random.Range(0, 2) * 2);
                int wholeTurns = Random.Range(1, 4);
                int fraction = Random.Range(0, activeWheels[i].steps);
                int indexDupe = i; 
                activeWheels[i].RotateSteps(direction * (wholeTurns + fraction), () => { 
                    wheelsDone[indexDupe] = true; 
                });
            }

            bool finished = false;
            while(!finished){
                yield return null;
                finished = true;
                for(int i=0; i<wheelsDone.Length; i++){
                    if(!wheelsDone[i]){
                        finished = false;
                        break;
                    }
                }
            }

            acceptInput = true;
            SetActiveWheel(activeWheels.Count - 1);

            LockPickingMinigameWheel MakeWheel (float sizeDeltaScale) {
                var newWheel = Instantiate(wheelPrefab, interactableWheelParent);
                int newWheelStepCount = Random.Range(minStepsPerWheel, maxStepsPerWheel);
                newWheel.Initialize(newWheelStepCount);
                newWheel.rectTransform.sizeDelta = Vector3.one * sizeDeltaScale;
                return newWheel;
            }
        }

        void Update () {
            if(acceptInput){
                if(Input.GetKeyDown(KeyCode.R)){
                    resetCoroutine.Stop(this);
                    resetCoroutine = StartCoroutine(Reset());
                }else{
                    if(Input.GetKeyDown(KeyCode.W)){
                        SetActiveWheelViaOffset(-1);
                    }else if(Input.GetKeyDown(KeyCode.S)){
                        SetActiveWheelViaOffset(1);
                    }
                    if(Input.GetKeyDown(KeyCode.A)){
                        activeWheels[currentlyActiveWheelIndex].RotateSteps(-1);
                    }else if(Input.GetKeyDown(KeyCode.D)){
                        activeWheels[currentlyActiveWheelIndex].RotateSteps(1);
                    }
                }
            }
        }

        void SetActiveWheel (int index) {
            for(int i=0; i<activeWheels.Count; i++){
                if(i==index){
                    activeWheels[i].SetColor(0.8f * Color.yellow + 0.2f * Color.red);
                }else{
                    activeWheels[i].SetColor(Color.white);
                }
            }
            currentlyActiveWheelIndex = index;
        }

        void SetActiveWheelViaOffset (int offset) {
            int newIndex = currentlyActiveWheelIndex + offset;
            while(newIndex < 0){
                newIndex += activeWheels.Count;
            }
            while(newIndex >= activeWheels.Count){
                newIndex -= activeWheels.Count;
            }
            SetActiveWheel(newIndex);
        }
    
    }

}
