
# if UNITY_EDITOR
using UnityEditor;
# endif
using System;
using Seagull.Interior_I1.Inspector;
using UnityEngine;

namespace Seagull.Interior_I1.SceneProps {
    public class Shiftable : MonoBehaviour {
        private Vector3 defaultPos;
        [SerializeField] public Vector3 startPos;
        [SerializeField] public Vector3 endPos;
        
        [Range(0f, 1f)] public float shift;

        [AButton("Init Positions")] [IgnoreInInspector]
        public Action init;
        private void Reset() {
            init = () => {
                startPos = defaultPos;
                endPos = defaultPos;
                OnValidate();
            };
        }

        // Start is called before the first frame update
        private void Start() {
            updatePos();
        }

        private float lastShift = -1;
        private void FixedUpdate() {
            if (lastShift == -1) {
                lastShift = shift;
                return;
            }

            if (lastShift == shift) return;
            updatePos();
            lastShift = shift;
        }

        private bool isFirst = false;
        private void OnValidate() {
            if (!isFirst) {
                defaultPos = transform.localPosition;
                isFirst = true;
            }
            updatePos();
            lastShift = shift;
        }

        private void updatePos() {
            shift = Mathf.Clamp01(shift);
            Vector3 pos = shift * (endPos - startPos) + startPos;
            transform.localPosition = pos;
        }
    }
    
# if UNITY_EDITOR
    [CustomEditor(typeof(Shiftable))]
    public class ShiftableInspector : AnInspector { }
# endif
}