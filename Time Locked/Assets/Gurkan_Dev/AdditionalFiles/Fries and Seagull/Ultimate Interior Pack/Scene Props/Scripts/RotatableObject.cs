using System;
using System.Collections.Generic;
using Seagull.Interior_I1.Inspector;

# if UNITY_EDITOR
# endif

using UnityEngine;

namespace Seagull.Interior_I1.SceneProps {
    [Serializable]
    public class String2Rotatable : KiiValuePair<string, Rotatable> {}
    
    public class RotatableObject : MonoBehaviour {
        public List<String2Rotatable> rotatables = new();
        private Dictionary<string, Rotatable> rotatableMap = new();
        
        private void Awake() {
            rotatables.ForEach(rot => rotatableMap[rot.key] = rot.value);
        }

        public void rotate(string id, float rotation01) {
            rotation01 = Mathf.Clamp01(rotation01);
            rotatableMap[id].rotation = rotation01;
        }
        
        public void rotate(float rotation01) {
            rotation01 = Mathf.Clamp01(rotation01);
            foreach (var rot in rotatableMap.Values) 
                rot.rotation = rotation01;
        }
    }
}