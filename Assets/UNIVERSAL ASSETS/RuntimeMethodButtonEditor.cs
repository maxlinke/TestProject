using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

public abstract class RuntimeMethodButtonEditor : Editor {

    MethodInfo[] methods;

    public override void OnInspectorGUI () {
        DrawInspector();
        DrawButtons();
    }

    protected virtual void DrawInspector () {
        DrawDefaultInspector();
    }

    private void DrawButtons () {
        if(EditorApplication.isPlaying){
            if(methods == null){
                FindAppropriateMethods();
            }
            foreach(var method in methods){
                if(GUILayout.Button(method.Name)){
                    method.Invoke(target, new object[method.GetParameters().Length]);
                }
            }
        }

        void FindAppropriateMethods () {
            var methodList = new List<MethodInfo>();
            foreach(var method in target.GetType().GetMethods()){
                foreach(var attribute in method.GetCustomAttributes()){
                    if(attribute is RuntimeMethodButtonAttribute){
                        methodList.Add(method);
                    }
                }
            }
            methods = methodList.ToArray();
        }
    }
	
}
