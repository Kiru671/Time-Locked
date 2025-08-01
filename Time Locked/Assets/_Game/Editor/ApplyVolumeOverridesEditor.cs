using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ApplyDualVolumeOverridesEditor : EditorWindow
{
    VolumeProfile retroProfile;
    VolumeProfile modernProfile;

    [MenuItem("Tools/Apply Volume Settings to House_1 & House_2")]
    public static void ShowWindow()
    {
        GetWindow<ApplyDualVolumeOverridesEditor>("Volume Profile Override");
    }

    void OnGUI()
    {
        GUILayout.Label("Assign Volume Profiles", EditorStyles.boldLabel);
        retroProfile = (VolumeProfile)EditorGUILayout.ObjectField("House_1 (Retro)", retroProfile, typeof(VolumeProfile), false);
        modernProfile = (VolumeProfile)EditorGUILayout.ObjectField("House_2 (Modern)", modernProfile, typeof(VolumeProfile), false);

        if (GUILayout.Button("Apply Both"))
        {
            if (retroProfile != null && modernProfile != null)
            {
                ApplyRetro(retroProfile);
                ApplyModern(modernProfile);

                EditorUtility.SetDirty(retroProfile);
                EditorUtility.SetDirty(modernProfile);
                AssetDatabase.SaveAssets();

                Debug.Log("✅ Retro & Modern Volume settings applied.");
            }
            else
            {
                Debug.LogError("At least one profile is missing.");
            }
        }
    }

    void ApplyRetro(VolumeProfile profile)
    {
        if (profile.TryGet(out Tonemapping tm)) tm.mode.Override(TonemappingMode.None);

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.threshold.Override(0.75f);
            bloom.intensity.Override(0.6f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(new Color(1f, 0.92f, 0.75f));
        }

        if (profile.TryGet(out ChromaticAberration ca)) ca.intensity.Override(0.2f);

        if (profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.Override(0.25f);
            vignette.smoothness.Override(0.35f);
            vignette.color.Override(Color.black);
            vignette.rounded.Override(true);
        }

        if (profile.TryGet(out ColorAdjustments color))
        {
            color.postExposure.Override(-0.4f);
            color.contrast.Override(-25f);
            color.saturation.Override(-15f);
            color.hueShift.Override(4f);
        }

        if (profile.TryGet(out LiftGammaGain lgg))
        {
            lgg.lift.Override(new Vector4(1.03f, 0.98f, 0.92f, 0f));
            lgg.gamma.Override(new Vector4(0.95f, 0.96f, 0.88f, 0f));
            lgg.gain.Override(new Vector4(0.97f, 0.99f, 0.90f, 0f));
        }

        if (profile.TryGet(out ShadowsMidtonesHighlights smh))
        {
            smh.shadows.Override(new Vector4(0.45f, 0.55f, 0.95f, 0f));
            smh.midtones.Override(new Vector4(0.65f, 0.75f, 0.6f, 0f));
            smh.highlights.Override(new Vector4(1f, 0.95f, 0.75f, 0f));
        }

        if (profile.TryGet(out WhiteBalance wb))
        {
            wb.temperature.Override(20f);
            wb.tint.Override(6f);
        }

        if (profile.TryGet(out FilmGrain grain))
        {
            grain.intensity.Override(0.4f);
            grain.response.Override(0.6f);
            grain.type.Override(FilmGrainLookup.Thin2);
        }

        if (profile.TryGet(out DepthOfField dof))
        {
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(10f);
            dof.gaussianEnd.Override(30f);
            dof.gaussianMaxRadius.Override(2f);
            dof.highQualitySampling.Override(true);
        }
    }

    void ApplyModern(VolumeProfile profile)
    {
        if (profile.TryGet(out Tonemapping tm)) tm.mode.Override(TonemappingMode.ACES);

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.85f);
            bloom.tint.Override(Color.white);
        }

        if (profile.TryGet(out ChromaticAberration ca)) ca.intensity.Override(0.05f);

        if (profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.Override(0.12f);
            vignette.smoothness.Override(0.4f);
            vignette.color.Override(Color.black);
            vignette.rounded.Override(true);
        }

        if (profile.TryGet(out ColorAdjustments color))
        {
            color.postExposure.Override(0.2f);
            color.contrast.Override(30f);
            color.saturation.Override(25f);
            color.hueShift.Override(2f);
        }

        if (profile.TryGet(out LiftGammaGain lgg))
        {
            lgg.lift.Override(new Vector4(0.97f, 0.97f, 0.97f, 0f));
            lgg.gamma.Override(new Vector4(1.00f, 1.00f, 1.00f, 0f));
            lgg.gain.Override(new Vector4(1.1f, 1.1f, 1.1f, 0f));
        }

        if (profile.TryGet(out ShadowsMidtonesHighlights smh))
        {
            smh.shadows.Override(new Vector4(1.0f, 1.0f, 1.0f, 0f));
            smh.midtones.Override(new Vector4(1.1f, 1.05f, 1.1f, 0f));
            smh.highlights.Override(new Vector4(1.2f, 1.1f, 1.0f, 0f));
        }

        if (profile.TryGet(out WhiteBalance wb))
        {
            wb.temperature.Override(3f);
            wb.tint.Override(1f);
        }

        if (profile.TryGet(out FilmGrain grain))
        {
            grain.intensity.Override(0.05f);
            grain.response.Override(0.5f);
            grain.type.Override(FilmGrainLookup.Medium1);
        }

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
