using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class ApplyRetroVolumeSettings : MonoBehaviour
{
    public Volume volume;

    private void OnEnable()
    {
        if (volume == null || volume.profile == null) return;

        VolumeProfile profile = volume.profile;

        // Tonemapping
        if (profile.TryGet(out Tonemapping tonemapping))
            tonemapping.mode.Override(TonemappingMode.None);

        // Bloom
        if (profile.TryGet(out Bloom bloom))
        {
            bloom.threshold.Override(0.75f);
            bloom.intensity.Override(0.6f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(new Color(1f, 0.92f, 0.75f));
        }

        // Chromatic Aberration
        if (profile.TryGet(out ChromaticAberration chromatic))
        {
            chromatic.intensity.Override(0.2f);
        }

        // Vignette
        if (profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.Override(0.25f);
            vignette.smoothness.Override(0.35f);
            vignette.color.Override(Color.black);
            vignette.rounded.Override(true);
        }

        // Color Adjustments
        if (profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.Override(-0.4f);
            colorAdjustments.contrast.Override(-25f);
            colorAdjustments.saturation.Override(-15f);
            colorAdjustments.hueShift.Override(4f);
        }

        // Lift Gamma Gain
        if (profile.TryGet(out LiftGammaGain liftGammaGain))
        {
            liftGammaGain.lift.Override(new Vector4(1.03f, 0.98f, 0.92f, 0f));
            liftGammaGain.gamma.Override(new Vector4(0.95f, 0.96f, 0.88f, 0f));
            liftGammaGain.gain.Override(new Vector4(0.97f, 0.99f, 0.90f, 0f));
        }

        // Shadows Midtones Highlights
        if (profile.TryGet(out ShadowsMidtonesHighlights smh))
        {
            smh.shadows.Override(new Vector4(0.45f, 0.55f, 0.95f, 0f));  
            smh.midtones.Override(new Vector4(0.65f, 0.75f, 0.6f, 0f));   
            smh.highlights.Override(new Vector4(1f, 0.95f, 0.75f, 0f)); 
        }

        // White Balance
        if (profile.TryGet(out WhiteBalance whiteBalance))
        {
            whiteBalance.temperature.Override(20f);
            whiteBalance.tint.Override(6f);
        }

        // Film Grain
        if (profile.TryGet(out FilmGrain grain))
        {
            grain.intensity.Override(0.4f);
            grain.response.Override(0.6f);
            grain.type.Override(FilmGrainLookup.Thin2);
        }

        // Depth of Field
        if (profile.TryGet(out DepthOfField dof))
        {
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(10f);
            dof.gaussianEnd.Override(30f);
            dof.gaussianMaxRadius.Override(2f);
            dof.highQualitySampling.Override(true);
        }
    }
}
