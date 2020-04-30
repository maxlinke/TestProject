using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    public abstract class ObjectPool : ScriptableObject {

        public bool Initiated { get; protected set; }
        public abstract int ObjectCount { get; }

        public void Initiate () {
            if(Initiated){
                Debug.LogError("Already initiated! Aborting...");
                return;
            }
            Init();
            this.Initiated = true;
        }

        public void Terminate () {
            if(!Initiated){
                Debug.LogError("Not even initiated! Aborting...");
                return;
            }
            DeInit();
            this.Initiated = false;
        }

        public SplineObject Next (Quaternion localRotation, System.Random rng = null) {
            if(!Initiated){
                Debug.LogError("Not initiated! Aborting...");
                return null;
            }
            return GetNext(localRotation, rng);
        }

        protected virtual void Init () { }

        protected virtual void DeInit () { }    // because "Term" sounds dumb

        protected abstract SplineObject GetNext (Quaternion localRotation, System.Random rng);
    }

}