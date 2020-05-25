using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FrameRateSetter : MonoBehaviour {

    [SerializeField] int targetFrameRate = 60;
    [SerializeField] bool vSync = false;

    [RuntimeMethodButton]
    public void SetFrameRate () {
        Application.targetFrameRate = Mathf.Max(1, targetFrameRate);
        targetFrameRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = (vSync ? 1 : 0);
    }

    [RuntimeMethodButton]
    public void UnlockFrameRate () {
        Application.targetFrameRate = -1;
        targetFrameRate = Application.targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(FrameRateSetter))]
public class FrameRateSetterEditor : RuntimeMethodButtonEditor {}
#endif