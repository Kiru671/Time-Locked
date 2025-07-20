using System;
using System.Collections.Generic;
using Seagull.Interior_I1.Inspector;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;
using UnityEngine.Events;

namespace Seagull.Interior_I1.SceneProps {
    [Serializable]
    public class String2GlowLight : KiiValuePair<string, GlowLight> {}

    public class LightSourceObject : MonoBehaviour {
        public bool isOn;
        
        public List<String2GlowLight> lights = new();
        private Dictionary<string, GlowLight> lightMap = new();
        
        [AButton("Turn On")] public UnityEvent onTurnOn;
        [AButton("Turn Off")] public UnityEvent onTurnOff;
        
        private void Start() {
            lights.ForEach(light => lightMap[light.key] = light.value);
            onTurnOn.AddListener(turnOnAll);
            onTurnOff.AddListener(turnOffAll);
            
            if (isOn) turnOnAll();
        }

        public void turnOnAll() {
            foreach (var lightMapValue in lightMap.Values) lightMapValue.turnOn();
        }

        public void turnOffAll() {
            foreach (var light in lightMap.Values) light.turnOff();
        }

        public void turnOn(string key) {
            lightMap[key].turnOn();
        }

        public void turnOff(string key) {
            lightMap[key].turnOff();
        }
    }
    
# if UNITY_EDITOR
    [CustomEditor(typeof(LightSourceObject))]
    public class LightSourceObjectInspector : AnInspector { }
# endif
}