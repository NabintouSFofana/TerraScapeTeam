using UnityEngine;

// Rain and snow
// Owner: Syed (environment).

public class WeatherSystem : MonoBehaviour
{
    public enum Mode { None, Rain, Snow }

    const float baseRainRate = 900f;
    const float baseSnowRate = 250f;

    [Tooltip("Weather strength: 0.2 = light drizzle/flurries, 1 = normal, 2.5 = storm.")]
    [Range(0.2f, 2.5f)]
    public float intensity = 1f;

    ParticleSystem rain;
    ParticleSystem snow;

    // Create the two particle systems, parented above the follow target.
    public void Init(Transform follow)
    {
        rain = Create("Rain", follow, true);
        snow = Create("Snow", follow, false);
        SetMode(Mode.None);
    }

    ParticleSystem Create(string name, Transform follow, bool isRain)
    {
        var go = new GameObject(name);
        if (follow != null)
        {
            go.transform.SetParent(follow, false);
            go.transform.localPosition = new Vector3(0f, 25f, 0f);
            go.transform.localRotation = Quaternion.identity;
        }

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop();

        var main = ps.main;
        main.startSpeed     = isRain ? 28f : 2.5f;
        main.startSize      = isRain ? 0.12f : 0.16f;
        main.startLifetime  = isRain ? 1.5f : 7f;
        main.startColor     = isRain ? new Color(0.7f, 0.8f, 1f, 0.6f) : new Color(1f, 1f, 1f, 0.9f);
        main.gravityModifier = isRain ? 1f : 0.12f;
        main.maxParticles   = 3000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = isRain ? 900f : 250f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(140f, 1f, 140f);

        // A plain unlit material so the drops/flakes are visible without extra assets.
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var shader = Shader.Find("Sprites/Default");
        if (shader != null) renderer.material = new Material(shader);

        return ps;
    }

    // Called by the weather buttons.
    public void SetMode(Mode m)
    {
        if (rain != null) { if (m == Mode.Rain) rain.Play(); else rain.Stop(); }
        if (snow != null) { if (m == Mode.Snow) snow.Play(); else snow.Stop(); }
        SetIntensity(intensity);
    }

    // Called by the intensity slider: scales how much rain/snow falls.
    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp(value, 0.2f, 2.5f);
        if (rain != null)
        {
            var e = rain.emission;
            e.rateOverTime = baseRainRate * intensity;
        }
        if (snow != null)
        {
            var e = snow.emission;
            e.rateOverTime = baseSnowRate * intensity;
        }
    }
}