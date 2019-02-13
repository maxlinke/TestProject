using UnityEngine;

public static class MyExtensions {

    public static void Stop (this Coroutine coroutine, MonoBehaviour runner) {
        if(coroutine != null){
            runner.StopCoroutine(coroutine);
        }
    }

	public static void SetGOActive (this Component component, bool value) {
		component.gameObject.SetActive(value);
	}

}
