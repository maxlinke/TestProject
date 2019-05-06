using UnityEngine;

[System.Serializable]
public class SimpleAnimation {

    [SerializeField] AnimationCurve m_animCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] float m_animDuration = 1;

    public AnimationCurve Curve => m_animCurve;
    public float Duration => m_animDuration;
	
}
