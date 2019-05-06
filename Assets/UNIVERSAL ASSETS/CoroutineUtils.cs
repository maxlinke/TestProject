using System;
using System.Collections;
using UnityEngine;

public static class CoroutineUtils {

    public static IEnumerator GenericAnimationCoroutine<T> (T start, T end, Action<T> set, Func<T, T, float, T> lerp, float duration, AnimationCurve animCurve = null, Action endAction = null, params CoroutineInstructions[] instructions) {
        var instruction = CoroutineInstructions.Combine(instructions);
        if(instruction.delay > 0f){
            yield return new WaitForSeconds(instruction.delay);
        }
        if(duration > 0){
            float t = 0f;
            while(t < 1f){ 
                set(lerp(start, end, GetProperEval(GetProperT(t))));
                t += Time.deltaTime / duration;
                yield return null;
            }
        }
        set(lerp(start, end, GetProperEval(GetProperT(1))));
        endAction?.Invoke();

        float GetProperT (float inputT) {
            return instruction.invertEvalInput ? (1f - inputT) : inputT;
        }

        float GetProperEval (float inputT) {
            return instruction.invertEvalOutput ? (1f - animCurve.EvaluateNullsafe(inputT)) : animCurve.EvaluateNullsafe(inputT);
        }
    }

    public static IEnumerator GenericAnimationCoroutine<T> (T start, T end, Action<T> set, Func<T, T, float, T> lerp, SimpleAnimation anim, Action endAction = null, params CoroutineInstructions[] instructions) {
        return GenericAnimationCoroutine<T>(start, end, set, lerp, anim.Duration, anim.Curve, endAction, instructions);
    }

    public static IEnumerator LocalScaleAnimationCoroutine (Transform targetTransform, Vector3 targetLocalScale, float duration, AnimationCurve animCurve = null, Action endAction = null, params CoroutineInstructions[] instructions) {
        return GenericAnimationCoroutine<Vector3>(
            start: targetTransform.localScale,
            end: targetLocalScale,
            set: lerped => { targetTransform.localScale = lerped; },
            lerp: Vector3.LerpUnclamped,
            duration: duration,
            animCurve: animCurve,
            endAction: endAction,
            instructions: instructions
        );
    }

    public static IEnumerator LocalScaleAnimationCoroutine (Transform targetTransform, Vector3 targetLocalScale, SimpleAnimation anim, Action endAction = null, params CoroutineInstructions[] instructions) {
        return LocalScaleAnimationCoroutine(targetTransform, targetLocalScale, anim.Duration, anim.Curve, endAction, instructions);
    }

    public static IEnumerator AnchoredPositionAnimationCoroutine (RectTransform targetTransform, Vector2 targetPos, float duration, AnimationCurve animCurve = null, Action endAction = null, params CoroutineInstructions[] instructions) {
        return GenericAnimationCoroutine<Vector2>(
            start: targetTransform.anchoredPosition,
            end: targetPos,
            set: lerped => { targetTransform.anchoredPosition = lerped; },
            lerp: Vector2.LerpUnclamped,
            duration: duration,
            animCurve: animCurve,
            endAction: endAction,
            instructions: instructions
        );
    }

    public static IEnumerator AnchoredPositionAnimationCoroutine (RectTransform targetTransform, Vector2 targetPos, SimpleAnimation anim, Action endAction = null, params CoroutineInstructions[] instructions) {
        return AnchoredPositionAnimationCoroutine(targetTransform, targetPos, anim.Duration, anim.Curve, endAction, instructions);
    }

    public static IEnumerator RectTransformPivotAnimationCoroutine (RectTransform targetTransform, Vector2 targetPivot, float duration, AnimationCurve animCurve = null, Action endAction = null, params CoroutineInstructions[] instructions) {
        return GenericAnimationCoroutine<Vector2>(
            start: targetTransform.pivot,
            end: targetPivot,
            set: lerped => { targetTransform.pivot = lerped; },
            lerp: Vector2.LerpUnclamped,
            duration: duration,
            animCurve: animCurve,
            endAction: endAction,
            instructions: instructions
        );
    }

    public static IEnumerator RectTransformPivotAnimationCoroutine (RectTransform targetTransform, Vector2 targetPivot, SimpleAnimation anim, Action endAction = null, params CoroutineInstructions[] instructions) {
        return RectTransformPivotAnimationCoroutine(targetTransform, targetPivot, anim.Duration, anim.Curve, endAction, instructions);
    }

}
