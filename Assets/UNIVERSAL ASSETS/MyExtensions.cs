using UnityEngine;
using System.Collections;

public static class MyExtensions {

    public static void Stop (this Coroutine coroutine, MonoBehaviour runner) {
        if(coroutine != null){
            runner.StopCoroutine(coroutine);
        }
    }

	public static Coroutine InvokeAfter (this MonoBehaviour runner, System.Action action, float delay) {
		return runner.StartCoroutine(InvokeAfter(action, delay, runner));
	}

	private static IEnumerator InvokeAfter (System.Action action, float delay, MonoBehaviour runner) {
		yield return new WaitForSeconds(delay);
		if(runner != null){
			action.Invoke();
		}
	}

	public static void SetGOActive (this Component component, bool value) {
		component.gameObject.SetActive(value);
	}

	public static float EvaluateNullsafe (this AnimationCurve curve, float t) {
		if(curve != null){
			return curve.Evaluate(t);
		}else{
			return t;
		}
	}

	public static void SetAnchoredXPos (this RectTransform rectTransform, float x) {
		rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
	}

	public static void SetAnchoredYPos (this RectTransform rectTransform, float y) {
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
	}

	public static void InvokeNullsafe (this System.Action action) {
		if(action != null) action.Invoke();
	}

	public static void InvokeNullsafe<T0> (this System.Action<T0> action, T0 t0) {
		if(action != null) action.Invoke(t0);
	}

	public static void InvokeNullsafe<T0, T1> (this System.Action<T0, T1> action, T0 t0, T1 t1) {
		if(action != null) action.Invoke(t0, t1);
	}

	public static void InvokeNullsafe<T0, T1, T2> (this System.Action<T0, T1, T2> action, T0 t0, T1 t1, T2 t2) {
		if(action != null) action.Invoke(t0, t1, t2);
	}

	public static void InvokeNullsafe<T0, T1, T2, T3> (this System.Action<T0, T1, T2, T3> action, T0 t0, T1 t1, T2 t2, T3 t3) {
		if(action != null) action.Invoke(t0, t1, t2, t3);
	}

}
