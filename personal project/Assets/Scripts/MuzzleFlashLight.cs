using UnityEngine;

/// <summary>
/// Attached to a muzzle flash GameObject with a Light component.
/// Briefly flashes the light when the particle system fires, then fades it out.
/// </summary>
[RequireComponent(typeof(Light))]
[RequireComponent(typeof(ParticleSystem))]
public class MuzzleFlashLight : MonoBehaviour
{
    [Header("Flash Settings")]
    public float peakIntensity = 8f;
    public float fadeSpeed = 20f;

    private Light _light;
    private ParticleSystem _ps;
    private bool _wasPlaying = false;

    void Awake()
    {
        _light = GetComponent<Light>();
        _ps = GetComponent<ParticleSystem>();
        _light.intensity = 0f;
        _light.enabled = false;
    }

    void Update()
    {
        bool isPlaying = _ps.isPlaying && _ps.particleCount > 0;

        // On burst start, snap to peak intensity
        if (isPlaying && !_wasPlaying)
        {
            _light.enabled = true;
            _light.intensity = peakIntensity;
        }

        // Fade out over time
        if (_light.enabled)
        {
            _light.intensity = Mathf.MoveTowards(_light.intensity, 0f, fadeSpeed * Time.deltaTime);
            if (_light.intensity <= 0f)
                _light.enabled = false;
        }

        _wasPlaying = isPlaying;
    }
}
