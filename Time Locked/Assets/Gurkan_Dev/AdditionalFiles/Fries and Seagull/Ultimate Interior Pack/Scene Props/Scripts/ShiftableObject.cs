using System;
using System.Collections.Generic;
using Seagull.Interior_I1.Inspector;

# if UNITY_EDITOR
# endif

using UnityEngine;

namespace Seagull.Interior_I1.SceneProps {
    [Serializable]
    public class String2Shiftable : KiiValuePair<string, Shiftable> {}
    
    public class ShiftableObject : MonoBehaviour {
        public List<String2Shiftable> shiftables = new();
        private Dictionary<string, Shiftable> shiftableMap = new();
        
        private void Awake() {
            shiftables.ForEach(rot => shiftableMap[rot.key] = rot.value);
        }

        public void shift(string id, float rotation01) {
            rotation01 = Mathf.Clamp01(rotation01);
            shiftableMap[id].shift = rotation01;
        }
        
        public void shift(float rotation01) {
            rotation01 = Mathf.Clamp01(rotation01);
            foreach (var rot in shiftableMap.Values) 
                rot.shift = rotation01;
        }
    }
}