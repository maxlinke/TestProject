using UnityEngine;

public static class MyExtensions {

    public static void Stop (this Coroutine coroutine, MonoBehaviour runner) {
        if(coroutine != null){
            runner.StopCoroutine(coroutine);
        }
    }

}
