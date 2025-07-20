using UnityEngine;

namespace Seagull.Interior_I1.Inspector {
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class MaterialHider : MonoBehaviour {
        public bool hided = true;

        private void OnValidate() {
            foreach (var material in gameObject.GetComponent<Renderer>().sharedMaterials) {
                if (material == null) continue;
                if (hided) material.hideFlags |= HideFlags.HideInInspector;
                else material.hideFlags &= ~HideFlags.HideInInspector;
            }
        }

        public void update(bool isHided) {
            if (!hided) return;
            
            foreach (var material in gameObject.GetComponent<Renderer>().sharedMaterials) {
                if (material == null) continue;
                if (isHided) material.hideFlags |= HideFlags.HideInInspector;
                else material.hideFlags &= ~HideFlags.HideInInspector;
            }
        }
    }
}