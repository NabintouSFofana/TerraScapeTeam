using UnityEngine;

// Owner: Nabintou
public class TerraScapeApp : MonoBehaviour
{
    public int terrainResolution = 100;
    public float terrainSize = 100f;

    TerrainMesh terrain;
    TerrainSculptor sculptor;
    FractalTerrain fractal;
    TerrainColorizer colorizer;
    WaterPlane water;
    DayNightCycle dayNight;
    WeatherSystem weather;
    ObjectPlacer placer;
    Camera cam;

    enum Mode { Raise, Lower, Smooth, PlaceTree, PlaceRock, PlaceDragon }
    Mode mode = Mode.Raise;

    float brushSize = 6f, brushStrength = 15f, waterLevel = 3f, timeOfDay = 12f;
    float weatherIntensity = 1f;
    Rect panelRect = new Rect(10, 10, 240, 560);
    string lastError = null;

    void Awake()
    {
        Debug.Log("[TerraScape] Awake START — building scene...");
        BuildScene();
        Debug.Log("[TerraScape] Awake DONE. terrain=" + (terrain != null) +
                  " water=" + (water != null) + " sun=" + (dayNight != null) +
                  " weather=" + (weather != null) + " placer=" + (placer != null));
    }

    // Run one build stage, catching and reporting any error so the rest still runs.
    void Stage(string name, System.Action action)
    {
        try { action(); Debug.Log("[TerraScape] OK: " + name); }
        catch (System.Exception e)
        {
            lastError = name + " -> " + e.Message;
            Debug.LogError("[TerraScape] FAILED at " + name + ": " + e);
        }
    }

    void BuildScene()
    {
        Stage("Camera", () =>
        {
            cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGO.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(terrainSize * 0.5f, terrainSize * 0.6f, -terrainSize * 0.25f);
            cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            if (cam.GetComponent<FlyCamera>() == null) cam.gameObject.AddComponent<FlyCamera>();
        });

        Stage("Terrain", () =>
        {
            var terrainGO = new GameObject("Terrain");
            terrainGO.SetActive(false);
            terrain = terrainGO.AddComponent<TerrainMesh>();
            terrain.resolution = terrainResolution;
            terrain.size = terrainSize;
            sculptor = terrainGO.AddComponent<TerrainSculptor>();
            fractal = terrainGO.AddComponent<FractalTerrain>();
            colorizer = terrainGO.AddComponent<TerrainColorizer>();
            terrainGO.SetActive(true);
            if (cam != null) sculptor.cam = cam;

            var vcShader = Shader.Find("TerraScape/VertexColorLit");
            if (vcShader != null)
                terrainGO.GetComponent<MeshRenderer>().material = new Material(vcShader);
            else
                Debug.LogWarning("[TerraScape] VertexColorLit shader not found — terrain will look plain.");

            fractal.Generate();
            colorizer.Recolor(waterLevel);
        });

        Stage("Water", () =>
        {
            var waterGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterGO.name = "Water";
            var wcol = waterGO.GetComponent<Collider>();
            if (wcol != null) Destroy(wcol);
            waterGO.transform.position = new Vector3(terrainSize * 0.5f, waterLevel, terrainSize * 0.5f);
            waterGO.transform.localScale = new Vector3(terrainSize / 10f * 2f, 1f, terrainSize / 10f * 2f);
            var wShader = Shader.Find("TerraScape/TransparentColor");
            if (wShader != null)
            {
                var wm = new Material(wShader) { color = new Color(0.2f, 0.5f, 0.85f, 0.6f) };
                waterGO.GetComponent<MeshRenderer>().material = wm;
            }
            water = waterGO.AddComponent<WaterPlane>();
            water.level = waterLevel;
        });

        Stage("Sun", () =>
        {
            Light sun = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                var sunGO = new GameObject("Directional Light");
                sun = sunGO.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            dayNight = sun.gameObject.AddComponent<DayNightCycle>();
            dayNight.sun = sun;
            dayNight.cam = cam;
            dayNight.SetTime(timeOfDay);
        });

        Stage("Weather", () =>
        {
            weather = new GameObject("Weather").AddComponent<WeatherSystem>();
            weather.Init(cam != null ? cam.transform : null);
        });

        Stage("Placer", () =>
        {
            placer = new GameObject("Placer").AddComponent<ObjectPlacer>();
            placer.cam = cam;
            placer.enabled = false;
        });

        if (sculptor != null) ApplyMode();
    }

    void Update()
    {
        if (terrain == null || sculptor == null || placer == null) return;

        Vector2 guiMouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        bool overPanel = panelRect.Contains(guiMouse);

        bool sculptMode = mode == Mode.Raise || mode == Mode.Lower || mode == Mode.Smooth;
        bool placeMode = mode == Mode.PlaceTree || mode == Mode.PlaceRock || mode == Mode.PlaceDragon;

        sculptor.enabled = sculptMode && !overPanel;
        placer.enabled = placeMode && !overPanel;

        sculptor.brushRadius = brushSize;
        sculptor.strength = brushStrength;

        if (sculptMode && Input.GetMouseButtonUp(0) && !overPanel && colorizer != null)
            colorizer.Recolor(waterLevel);
    }

    void ApplyMode()
    {
        switch (mode)
        {
            case Mode.Raise:  sculptor.tool = TerrainSculptor.Tool.Raise;  break;
            case Mode.Lower:  sculptor.tool = TerrainSculptor.Tool.Lower;  break;
            case Mode.Smooth: sculptor.tool = TerrainSculptor.Tool.Smooth; break;
            case Mode.PlaceTree: if (placer != null) placer.kind = ObjectPlacer.Kind.Tree; break;
            case Mode.PlaceRock: if (placer != null) placer.kind = ObjectPlacer.Kind.Rock; break;
            case Mode.PlaceDragon: if (placer != null) placer.kind = ObjectPlacer.Kind.Dragon; break;
        }
    }

    void OnGUI()
    {
        GUILayout.Window(0, panelRect, DrawPanel, "TerraScape");
    }

    void DrawPanel(int id)
    {
        if (lastError != null)
        {
            GUILayout.Label("BUILD ERROR:\n" + lastError);
            return;
        }

        GUILayout.Label("Tool");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Raise"))  { mode = Mode.Raise;  ApplyMode(); }
        if (GUILayout.Button("Lower"))  { mode = Mode.Lower;  ApplyMode(); }
        if (GUILayout.Button("Smooth")) { mode = Mode.Smooth; ApplyMode(); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Place Tree")) { mode = Mode.PlaceTree; ApplyMode(); }
        if (GUILayout.Button("Place Rock")) { mode = Mode.PlaceRock; ApplyMode(); }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Place Dragon (L-system)")) { mode = Mode.PlaceDragon; ApplyMode(); }
        GUILayout.Label("Current tool: " + mode);

        GUILayout.Space(8);
        GUILayout.Label("Brush size: " + brushSize.ToString("0.0"));
        brushSize = GUILayout.HorizontalSlider(brushSize, 2f, 20f);
        GUILayout.Label("Brush strength: " + brushStrength.ToString("0"));
        brushStrength = GUILayout.HorizontalSlider(brushStrength, 2f, 40f);

        GUILayout.Space(8);
        GUILayout.Label("Water level: " + waterLevel.ToString("0.0"));
        float newWater = GUILayout.HorizontalSlider(waterLevel, -10f, 25f);
        if (!Mathf.Approximately(newWater, waterLevel))
        {
            waterLevel = newWater;
            if (water != null) water.SetLevel(waterLevel);
            if (colorizer != null) colorizer.Recolor(waterLevel);
        }

        GUILayout.Label("Time of day: " + timeOfDay.ToString("0.0"));
        float newTime = GUILayout.HorizontalSlider(timeOfDay, 0f, 24f);
        if (!Mathf.Approximately(newTime, timeOfDay))
        {
            timeOfDay = newTime;
            if (dayNight != null) dayNight.SetTime(timeOfDay);
        }

        GUILayout.Space(8);
        if (GUILayout.Button("Generate Random Terrain"))
        {
            if (fractal != null) fractal.Generate();
            if (colorizer != null) colorizer.Recolor(waterLevel);
        }

        GUILayout.Space(8);
        GUILayout.Label("Weather");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("None") && weather != null) weather.SetMode(WeatherSystem.Mode.None);
        if (GUILayout.Button("Rain") && weather != null) weather.SetMode(WeatherSystem.Mode.Rain);
        if (GUILayout.Button("Snow") && weather != null) weather.SetMode(WeatherSystem.Mode.Snow);
        GUILayout.EndHorizontal();
        GUILayout.Label("Weather intensity: " + weatherIntensity.ToString("0.0"));
        float newIntensity = GUILayout.HorizontalSlider(weatherIntensity, 0.2f, 2.5f);
        if (!Mathf.Approximately(newIntensity, weatherIntensity))
        {
            weatherIntensity = newIntensity;
            if (weather != null) weather.SetIntensity(weatherIntensity);
        }

        GUILayout.Space(8);
        GUILayout.Label("Fly: W A S D + hold right-mouse\nLeft-click: use tool\nRight-click (place): delete");
    }
}
