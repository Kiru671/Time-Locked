
# if UNITY_EDITOR
using UnityEditor;
# endif
using Seagull.Interior_I1.Inspector;
using UnityEngine;

namespace Seagull.Interior_I1.SceneProps {
    public enum Axis {
        x, y, z
    }
    
    public class Rotatable : MonoBehaviour {
        [SerializeField] private float startAngle;
        [SerializeField] private float endAngle;
        [SerializeField] private Axis rotationAxis;
        
        [Range(0f, 1f)] public float rotation;
        
        // Start is called before the first frame update
        private void Start() {
            if (startAngle > endAngle) (endAngle, startAngle) = (startAngle, endAngle);
            updateAngle();
        }

        private float lastRotation = -1;
        private void FixedUpdate() {
            if (lastRotation == -1) {
                lastRotation = rotation;
                return;
            }

            if (lastRotation == rotation) return;
            updateAngle();
            lastRotation = rotation;
        }

        private void OnValidate() {
            updateAngle();
            lastRotation = rotation;
        }

        private void updateAngle() {
            rotation = Mathf.Clamp01(rotation);
            float angle = rotation * (endAngle - startAngle) + startAngle;
            Vector3 axis = rotationAxis switch {
                Axis.x => Vector3.right,
                Axis.y => Vector3.up,
                Axis.z => Vector3.forward,
                _ => Vector3.zero
            };
            transform.localRotation = Quaternion.AngleAxis(angle, axis);
        }
    }
    
# if UNITY_EDITOR
    [CustomEditor(typeof(Rotatable))]
    public class RotatableEditor : AnInspector { }
# endif
}