using System.Collections.Generic;
using UnityEngine;

namespace Seagull.Interior_I1.SceneProps {
    public class GlowLight : MonoBehaviour {
        private MeshRenderer lightBoxRenderer;
        public List<Light> lightSources;
        public Color activeLightSourceSurfaceColor;
        public Color inactiveLightSourceSurfaceColor;
        [ColorUsage(false, true)]
        public Color lightEmissionColor;

        private void Awake() {
            lightBoxRenderer = GetComponent<MeshRenderer>();
            turnOff();
        }

        public virtual void turnOn() {
            lightSources.ForEach(ls => {
                if (ls == null) return;
                ls.enabled = true;
            });
            lightBoxRenderer.material.color = activeLightSourceSurfaceColor;
            lightBoxRenderer.material.SetColor("_EmissionColor", lightEmissionColor);
            lightBoxRenderer.material.EnableKeyword("_EMISSION");
        }
        
        public virtual void turnOff() {
            lightSources.ForEach(ls => {
                if (ls == null) return;
                ls.enabled = false;
            });
            lightBoxRenderer.material.color = inactiveLightSourceSurfaceColor;
            lightBoxRenderer.material.DisableKeyword("_EMISSION");
        }
        
        public virtual void turnOn(Color lightSourceSurfaceColor, Color lightEmissionColor, float lightEmissionIntensity) {
            lightBoxRenderer.material.color = lightSourceSurfaceColor;
            lightBoxRenderer.material.SetColor("_EmissionColor", lightEmissionColor * lightEmissionIntensity);
            lightBoxRenderer.material.EnableKeyword("_EMISSION");
        }

        public virtual void turnOff(Color inactiveLightSourceSurfaceColor) {
            lightBoxRenderer.material.color = inactiveLightSourceSurfaceColor;
            lightBoxRenderer.material.DisableKeyword("_EMISSION");
        }
    }
}