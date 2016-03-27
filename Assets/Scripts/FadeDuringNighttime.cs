using UnityEngine;

/// Fades a light when it is rotated upside down so objects aren't still illuminated.
[RequireComponent(typeof(Light))]
public class FadeDuringNighttime : MonoBehaviour {
    private readonly Quaternion MiddayAngle = Quaternion.Euler(90, 0, 0); // straight down

    public float TwilightStartAngle = 85f;
    public float TwilightStopAngle = 100f;

    private Light _light;

    void Start() {
        _light = GetComponent<Light>();
    }

    void Update() {
        // Above TwilightStartAngle, set intensity to 1. Below TwilightStopAngle, set to 0. Lerp in between.
        float angle = Quaternion.Angle(MiddayAngle, transform.rotation);
        _light.intensity = 1 - Mathf.Clamp01((angle - TwilightStartAngle) / (TwilightStopAngle - TwilightStartAngle));
    }
}
