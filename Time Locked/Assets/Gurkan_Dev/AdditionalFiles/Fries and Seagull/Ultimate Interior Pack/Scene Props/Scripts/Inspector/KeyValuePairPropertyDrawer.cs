

namespace Seagull.Interior_I1.Inspector {
# if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(KiiValuePair), true)]
    public class KeyValuePairPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var keyProp = property.FindPropertyRelative("key");
            var valueProp = property.FindPropertyRelative("value");
            
            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = (position.width - labelWidth) / 2 - 5;

            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            Rect stringRect = new Rect(position.x + labelWidth, position.y, fieldWidth, position.height);
            Rect glowLightRect = new Rect(position.x + labelWidth + fieldWidth + 10, position.y, fieldWidth, position.height);

            EditorGUI.LabelField(labelRect, label);

            EditorGUI.PropertyField(stringRect, keyProp, GUIContent.none);
            EditorGUI.PropertyField(glowLightRect, valueProp, GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
# endif
}