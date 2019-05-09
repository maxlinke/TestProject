using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockPickingMinigameWheel : MonoBehaviour {

    public enum Spinability {
        Clockwise,
        Counterclockwise,
        Free
    }

    [SerializeField] SimpleAnimation advanceAnim;
    [SerializeField] Transform markerParent;
    [SerializeField] Image colorImage;
    [SerializeField] Sprite subStepSprite;
    [SerializeField] float distanceOfSubStepSpritesFromEdge;
    [SerializeField] Vector2 mainNotchSize;
    [SerializeField] float subMarkerCircleSize;
    [SerializeField] Vector2 subMarkerRectangleSize;
    [SerializeField] Color mainNotchColor;
    [SerializeField] Color subMarkerColor;

    LockPickingMinigameWheel linkedWheel;
    int linkFactor;
    Spinability spinability;
    Coroutine rotationCoroutine;

    public RectTransform rectTransform { get; private set; }
    public int currentStep { get; private set; }
    public int steps { get; private set; }

    void Awake () {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize (int steps, Spinability spinability = Spinability.Free, bool spawnStepMarkers = true) {
        this.steps = steps;
        currentStep = 0;
        rectTransform.localEulerAngles = new Vector3(0f, 0f, -GetAngleForStep(0));
        linkedWheel = null;
        linkFactor = 0;
        RemoveOldMarkers();
        if(spawnStepMarkers){
            SpawnMarkers();
        }

        void RemoveOldMarkers () {
            for(int i=markerParent.childCount-1; i>=0; i--){
                Destroy(markerParent.GetChild(i));
            }
        }

        void SpawnMarkers () {
            GetNewRectangleMarker(0, mainNotchSize, mainNotchColor);
            for(int i=1; i<steps; i++){
                GetNewCircleMarker(i, subMarkerCircleSize, subMarkerColor);
                GetNewRectangleMarker(i, subMarkerRectangleSize, subMarkerColor);
            }

            RectTransform GetNewMarker (int index, out Vector2 sineCosine) {
                float angle = 360f * ((float)index) / steps;
                float radAngle = Mathf.Deg2Rad * angle;
                float x = Mathf.Sin(radAngle);
                float y = Mathf.Cos(radAngle);
                sineCosine = new Vector2(x, y);
                var newMarkerRT = new GameObject("New Marker", typeof(RectTransform)).GetComponent<RectTransform>();
                newMarkerRT.SetParent(markerParent, false);
                newMarkerRT.pivot = new Vector2(0.5f, 1f);
                newMarkerRT.SetAnchorPoint(0.5f * (new Vector2(x, y) + Vector2.one));
                newMarkerRT.anchoredPosition = Vector2.zero;
                newMarkerRT.localEulerAngles = new Vector3(0f, 0f, -angle);
                return newMarkerRT;
            }

            RectTransform GetNewRectangleMarker (int index, Vector2 sizeDelta, Color color) {
                var newMarker = GetNewMarker(index, out _);
                newMarker.sizeDelta = sizeDelta;
                var newMarkerImage = newMarker.gameObject.AddComponent<Image>();
                newMarkerImage.color = color;
                return newMarker;
            }

            RectTransform GetNewCircleMarker (int index, float size, Color color) {
                var newMarker = GetNewMarker(index, out Vector2 sineCosine);
                newMarker.sizeDelta = Vector2.one * size;
                newMarker.anchoredPosition = sineCosine * distanceOfSubStepSpritesFromEdge * -1;
                var newMarkerImage = newMarker.gameObject.AddComponent<Image>();
                newMarkerImage.sprite = subStepSprite;
                newMarkerImage.color = color;
                return newMarker;
            }

        }
    }

    public void SetLinkedToOther (LockPickingMinigameWheel otherWheel, int linkFactor) {
        this.linkedWheel = otherWheel;
        this.linkFactor = linkFactor;
    }

    public void SetColor (Color color) {
        colorImage.color = color;
    }

    float GetAngleForStep (int step) {
        return 360f * ((float)step / steps);
    }

    //TODO rotate step-by-step instead?
    //TODO sounds
    //TODO volume for those sounds
    //TODO random duration offset for the anim?

    public void RotateSteps (int stepsToRotate, System.Action onDoneRotating = null, bool instantly = false) {
        rotationCoroutine.Stop(this);
        int newStep = currentStep + stepsToRotate;
        if(instantly){
            rectTransform.localEulerAngles = new Vector3(0, 0, -GetAngleForStep(newStep));
        }else{
            rotationCoroutine = StartCoroutine(CoroutineUtils.GenericAnimationCoroutine<float>(
                start: GetAngleForStep(currentStep),
                end: GetAngleForStep(newStep),
                set: lerped => { rectTransform.localEulerAngles = new Vector3(0, 0, -lerped); },
                lerp: Mathf.LerpUnclamped,
                anim: advanceAnim,
                endAction: onDoneRotating
            ));
        }
        currentStep = newStep;
        if(linkedWheel != null){
            linkedWheel.RotateSteps(linkFactor * stepsToRotate, instantly: instantly);
        }
    }

    // int GetStepDeltaForLinkedWheel (int localStepDelta) {
    //     return (linkFactor * localStepDelta * linkedWheel.steps) / this.steps;
    // }
	
}
