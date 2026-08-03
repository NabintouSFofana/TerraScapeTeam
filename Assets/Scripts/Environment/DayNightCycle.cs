using UnityEngine;

// Day/night
// brightness, plus the sky/ambient color
// Owner: Syed (environment).

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public Camera cam;             // its background is used as the sky color
    [Range(0f, 24f)] public float timeOfDay = 12f;

    void Start()
    {
        if (sun == null) sun = GetComponent<Light>();
        SetTime(timeOfDay);
    }

    // Called by the time-of-day slider.
    public void SetTime(float t)
    {
        timeOfDay = t;

        // Sun angle: 6:00 = sunrise (horizon), 12:00 = overhead, 18:00 = sunset.
        float xAngle = (t / 24f) * 360f - 90f;
        if (sun != null)
            sun.transform.rotation = Quaternion.Euler(xAngle, 30f, 0f);

        float elevation = Mathf.Sin(xAngle * Mathf.Deg2Rad);   // >0 daytime
        float day = Mathf.Clamp01(elevation);

        if (sun != null)
        {
            sun.intensity = Mathf.Lerp(0.05f, 1.15f, day);
            // Warm/orange near the horizon, white-ish at midday.
            sun.color = Color.Lerp(new Color(1f, 0.5f, 0.3f), new Color(1f, 0.96f, 0.85f), day);
        }

        Color skyDay   = new Color(0.45f, 0.65f, 0.90f);
        Color skyNight = new Color(0.03f, 0.04f, 0.09f);
        if (cam != null) cam.backgroundColor = Color.Lerp(skyNight, skyDay, day);

        RenderSettings.ambientLight =
            Color.Lerp(new Color(0.05f, 0.06f, 0.10f), new Color(0.5f, 0.5f, 0.55f), day);
    }
}