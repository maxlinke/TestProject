using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class BuildSafeUndo {

    // it would have been sooooo nice to use these...

    // private static void DoIfInEditor (System.Action action) {
    //     #if UNITY_EDITOR
    //     action();
    //     #endif
    // }

    // private static T ReturnIfInEditor<T> (System.Func<T> func, T defaultReturn) {
    //     #if UNITY_EDITOR
    //     return func();
    //     #else
    //     return defaultReturn;
    //     #endif
    // } 

    public static void AddComponent (GameObject gameObject, System.Type type) {
        #if UNITY_EDITOR
        Undo.AddComponent(gameObject, type);
        #endif
    }

    public static void ClearAll () {
        #if UNITY_EDITOR
        Undo.ClearAll();
        #endif
    }

    public static void ClearUndo (Object identifier) {
        #if UNITY_EDITOR
        Undo.ClearUndo(identifier);
        #endif
    }

    public static void CollapseUndoOperations (int groupIndex) {
        #if UNITY_EDITOR
        Undo.CollapseUndoOperations(groupIndex);
        #endif
    }

    public static void DestroyObjectImmediate (Object objectToUndo) {
        #if UNITY_EDITOR
        Undo.DestroyObjectImmediate(objectToUndo);
        #endif
    }

    public static void FlushUndoRecordObjects () {
        #if UNITY_EDITOR
        Undo.FlushUndoRecordObjects();
        #endif
    }

    public static int GetCurrentGroup () {
        #if UNITY_EDITOR
        return Undo.GetCurrentGroup();
        #else
        return 0;
        #endif
    }

    public static string GetCurrentGroupName () {
        #if UNITY_EDITOR
        return Undo.GetCurrentGroupName();
        #else
        return string.Empty;
        #endif
    }

    public static void IncrementCurrentGroup () {
        #if UNITY_EDITOR
        Undo.IncrementCurrentGroup();
        #endif
    }

    public static void MoveGameObjectToScene (GameObject go, UnityEngine.SceneManagement.Scene scene, string name) {
        #if UNITY_EDITOR
        MoveGameObjectToScene(go, scene, name);
        #endif
    }

    public static void PerformRedo() {
        #if UNITY_EDITOR
        Undo.PerformRedo();
        #endif
    }

    public static void PerformUndo () {
        #if UNITY_EDITOR
        Undo.PerformUndo();
        #endif
    }

    public static void RecordObject (Object objectToUndo, string name) {
        #if UNITY_EDITOR
        Undo.RecordObject(objectToUndo, name);
        #endif
    }

    public static void RecordObjects (Object[] objectsToUndo, string name) {
        #if UNITY_EDITOR
        Undo.RecordObjects(objectsToUndo, name);
        #endif
    }

    public static void RegisterCompleteObjectUndo (Object objectToUndo, string name) {
        #if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(objectToUndo, name);
        #endif
    }

    public static void RegisterCreatedObjectUndo (Object objectToUndo, string name) {
        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(objectToUndo, name);
        #endif
    }

    public static void RegisterFullObjectHierarchyUndo (Object objectToUndo, string name) {
        #if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(objectToUndo, name);
        #endif
    }

    public static void RevertAllDownToGroup (int group) {
        #if UNITY_EDITOR
        Undo.RevertAllDownToGroup(group);
        #endif
    }

    public static void RevertAllInCurrentGroup () {
        #if UNITY_EDITOR
        Undo.RevertAllInCurrentGroup();
        #endif
    }

    public static void SetCurrentGroupName (string name) {
        #if UNITY_EDITOR
        Undo.SetCurrentGroupName(name);
        #endif
    }

    public static void SetTransformParent (Transform transform, Transform newParent, string name) {
        #if UNITY_EDITOR
        Undo.SetTransformParent(transform, newParent, name);
        #endif
    }
	
}
