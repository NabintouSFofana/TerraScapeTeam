using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Puts a "TerraScape" menu at the top of the Unity window with a button that sets the
// scene up. I wrote this because dragging objects in and typing the camera coordinates
// by hand every time got old fast, and this way everyone's scene starts identical.
//
// Note this is an editor script, not a game script - it runs while you're working in
// Unity, and the objects it makes are normal scene objects you save with Ctrl+S. It has
// to sit in a folder called "Editor" or Unity complains when you build.
//
// You can run it more than once, it just skips whatever is already in the scene.
public class StarterSceneBuilder
{
    [MenuItem("TerraScape/Build Starter Scene")]
    public static void BuildStarterScene()
    {
        // Ground - just a plane for now so there is something to click on.
        // Loi's terrain will take over from this. The placer raycasts either way.
        GameObject ground = GameObject.Find("TempGround");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "TempGround";
            Undo.RegisterCreatedObjectUndo(ground, "Build Starter Scene");
        }
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10f, 1f, 10f);   // 100 x 100 units

        // Camera, set back and looking down so you can see the whole plane.
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            Undo.RegisterCreatedObjectUndo(camGO, "Build Starter Scene");
        }
        cam.transform.position = new Vector3(0f, 25f, -35f);
        cam.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

        // A light, otherwise everything renders black.
        Light sun = null;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) { sun = l; break; }
        if (sun == null)
        {
            var sunGO = new GameObject("Directional Light");
            sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sunGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Undo.RegisterCreatedObjectUndo(sunGO, "Build Starter Scene");
        }

        // The placer itself.
        GameObject placer = GameObject.Find("Placer");
        if (placer == null)
        {
            placer = new GameObject("Placer");
            Undo.RegisterCreatedObjectUndo(placer, "Build Starter Scene");
        }
        if (placer.GetComponent<ObjectPlacer>() == null)
            placer.AddComponent<ObjectPlacer>();

        // Tell Unity the scene changed, otherwise Ctrl+S does nothing.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[TerraScape] Starter scene built: TempGround + Main Camera + " +
                  "Directional Light + Placer. Press Ctrl+S to save, then Play to test.");
    }
}
