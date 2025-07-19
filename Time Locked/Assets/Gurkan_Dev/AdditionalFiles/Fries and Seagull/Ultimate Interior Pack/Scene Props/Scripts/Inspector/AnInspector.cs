#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Seagull.Interior_I1.Inspector {
    
    public class AnInspector : Editor {
        
        private SerializedObject serializedObj;

        private void OnEnable() {
            serializedObj = new SerializedObject(target);
        }
        
        public override void OnInspectorGUI() {
            Type type = target.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            EditorGUI.BeginChangeCheck();
            
            foreach (var field in fields) {
                SerializedProperty prop = serializedObj.FindProperty(field.Name);
                
                if (field.FieldType == typeof(UnityEvent) || field.FieldType == typeof(Action)) {
                    AButtonAttribute attr = field.GetCustomAttribute<AButtonAttribute>();
                    if (attr != null) {
                        string name = field.Name;
                        if (attr.text != null) name = attr.text;

                        if (field.FieldType == typeof(UnityEvent) && !Application.isPlaying) 
                            name = $"{name} (Require to start the game)";
                        
                        if (GUILayout.Button(name)) {
                            if (field.FieldType == typeof(UnityEvent))
                                ((UnityEvent)field.GetValue(target)).Invoke();
                            else {
                                try {
                                    ((Action)field.GetValue(target)).Invoke();
                                }
                                catch (Exception) {
                                    MethodInfo startMethod = target.GetType().GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                                    if (startMethod != null && startMethod.GetParameters().Length == 0) {
                                        startMethod.Invoke(target, null);
                                        ((Action)field.GetValue(target)).Invoke();
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (field.GetCustomAttribute<IgnoreInInspectorAttribute>() == null)
                    EditorGUILayout.PropertyField(prop, true);
            }
            
            EditorGUI.EndChangeCheck();
            serializedObj.ApplyModifiedProperties();
        }
    }

    
}

#endif