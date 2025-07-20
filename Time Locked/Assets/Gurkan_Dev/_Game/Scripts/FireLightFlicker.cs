using UnityEngine;

public class FireLightFlicker : MonoBehaviour
{
    public Light targetLight;
    public float minIntensity = 1f;
    public float maxIntensity = 2.5f;
    public float flickerSpeed = 15f;

    private float noiseOffset;

    void Start()
    {
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        float flicker = Mathf.Lerp(minIntensity, maxIntensity, noise);
        targetLight.intensity = flicker;
    }
}
