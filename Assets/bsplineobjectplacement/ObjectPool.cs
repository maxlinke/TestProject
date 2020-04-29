using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    public abstract class ObjectPool : ScriptableObject {

        protected bool initiated = false;
        public bool Initiated => initiated;
        
        public abstract int ObjectCount { get; }

        public void Initiate () {
            if(initiated){
                Debug.LogError("Already initiated! Aborting...");
                return;
            }
            Init();
            this.initiated = true;
        }

        public void Terminate () {
            if(!initiated){
                Debug.LogError("Not even initiated! Aborting...");
                return;
            }
            DeInit();
            this.initiated = false;
        }

        public SplineObject Next (System.Random rng = null) {
            if(!initiated){
                Debug.LogError("Not initiated! Aborting...");
                return null;
            }
            return GetNext(rng);
        }

        protected virtual void Init () { }

        protected virtual void DeInit () { }    // because "Term" sounds dumb

        protected abstract SplineObject GetNext (System.Random rng);
    }

}