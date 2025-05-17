using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class CandleLightFlicker : MonoBehaviour
{
    private Light2D candleLight;

    [Header("Radius Flicker Settings")]
    public float baseInnerRadius = 1.0f;
    public float baseOuterRadius = 3.0f;
    public float innerRadiusVariation = 0.55f;
    public float outerRadiusVariation = 0.54f;

    [Header("Flicker Timing")]
    public float flickerSpeed = 1.97f;      
    public float noiseScale = 1.0f;         

    private float seed;

    void Awake()
    {
        candleLight = GetComponent<Light2D>();
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time * flickerSpeed;

        float innerFlicker = Mathf.Sin(time * 2f) * 0.5f + 0.5f;
        innerFlicker += Mathf.PerlinNoise(seed, time * noiseScale);
        innerFlicker = Mathf.Clamp01(innerFlicker / 2f);

        float outerFlicker = Mathf.Sin(time) * 0.5f + 0.5f;
        outerFlicker += Mathf.PerlinNoise(seed + 10f, time * noiseScale);
        outerFlicker = Mathf.Clamp01(outerFlicker / 2f);

        candleLight.pointLightInnerRadius = baseInnerRadius + innerFlicker * innerRadiusVariation;
        candleLight.pointLightOuterRadius = baseOuterRadius + outerFlicker * outerRadiusVariation;
    }
}
