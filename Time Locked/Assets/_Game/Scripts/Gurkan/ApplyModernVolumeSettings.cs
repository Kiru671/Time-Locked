using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class ApplyModernVolumeSettings : MonoBehaviour
{
    public Volume volume;

    private void OnEnable()
    {
        if (volume == null || volume.profile == null) return;

        VolumeProfile profile = volume.profile;

        // Tonemapping
        if (profile.TryGet(out Tonemapping tonemapping))
            tonemapping.mode.Override(TonemappingMode.ACES);

        // Bloom
        if (profile.TryGet(out Bloom bloom))
        {
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.85f);
            bloom.tint.Override(Color.white);
        }

        // Chromatic Aberration
        if (profile.TryGet(out ChromaticAberration chromatic))
        {
            chromatic.intensity.Override(0.05f);
        }

        // Vignette
        if (profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.Override(0.12f);
            vignette.smoothness.Override(0.4f);
            vignette.color.Override(Color.black);
            vignette.rounded.Override(true);
        }

        // Color Adjustments
        if (profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.Override(0.2f);
            colorAdjustments.contrast.Override(30f);
            colorAdjustments.saturation.Override(25f);
            colorAdjustments.hueShift.Override(2f);
        }

        // Lift Gamma Gain
        if (profile.TryGet(out LiftGammaGain liftGammaGain))
        {
            liftGammaGain.lift.Override(new Vector4(0.97f, 0.97f, 0.97f, 0f));
            liftGammaGain.gamma.Override(new Vector4(1.00f, 1.00f, 1.00f, 0f));
            liftGammaGain.gain.Override(new Vector4(1.1f, 1.1f, 1.1f, 0f));
        }

        // Shadows Midtones Highlights
        if (profile.TryGet(out ShadowsMidtonesHighlights smh))
        {
            smh.shadows.Override(new Vector4(1.0f, 1.0f, 1.0f, 0f));
            smh.midtones.Override(new Vector4(1.1f, 1.05f, 1.1f, 0f));
            smh.highlights.Override(new Vector4(1.2f, 1.1f, 1.0f, 0f));
        }

        // White Balance
        if (profile.TryGet(out WhiteBalance whiteBalance))
        {
            whiteBalance.temperature.Override(3f);
            whiteBalance.tint.Override(1f);
        }

        // Film Grain 
        if (profile.TryGet(out FilmGrain grain))
        {
            grain.intensity.Override(0.05f);
            grain.response.Override(0.5f);
            grain.type.Override(FilmGrainLookup.Medium1);
        }

        // Depth of Field
        if (profile.TryGet(out DepthOfField dof))
        {
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(5f);
            dof.gaussianEnd.Override(30f);
            dof.gaussianMaxRadius.Override(1f);
            dof.highQualitySampling.Override(true);
        }
    }
}
