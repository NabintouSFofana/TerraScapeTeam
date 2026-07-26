# TerraScape — An Interactive 3D Terrain and Environment Sandbox

**CS 4361.0W1 — Computer Graphics — Summer 2026 — Team 1**
Theme 3: Interactive Graphics Program

Anisha Baidya · Loi Nguyen · Syed Naqvi · Nabintou S. Fofana

An interactive 3D editor. You start with flat ground and shape it into hills and valleys
with the mouse, generate random landscapes with fractals, raise the water level to make
lakes, move the sun through a day/night cycle, place trees and rocks, and turn rain and
snow on and off.

---

## How we are building it

Each of us owns one part and adds it to this repo as we go, so the project grows piece by
piece. That way we each know our own code well enough to explain it in the presentation.

| Who | Part | Status |
|---|---|---|
| Nabintou Fofana | Placing objects, fractal trees | done — in the repo |
| Loi Nguyen | Terrain mesh, sculpting brushes, random terrain, colouring | in progress |
| Syed Naqvi | Water, day/night lighting, rain and snow | in progress |
| Anisha Baidya | Camera, on-screen menu, putting it all together | in progress |

Nabintou is sending each person a document on Discord with the details of their part.

---

## Getting set up

1. Install **Unity 6.5**, version `6000.5.4f1`. We all need the same version or the
   project breaks when we swap files.
2. In GitHub Desktop: **File → Clone repository →** pick this one. Put it somewhere
   simple like `C:\Dev\`. **Don't put it in OneDrive** — OneDrive syncs while Unity is
   working and corrupts the project.
3. **Unity Hub → Open →** choose the folder you just cloned.
4. Open `Assets/Scenes/SampleScene` and hit **Play**.

If it worked you can left click the grey ground to grow a tree, scroll over a tree to
turn it, and right click to delete it.

The grey plane is temporary — it's only there so there's something to click on until
Loi's terrain is in. The placer uses a raycast so it works on either one without any
code change.

---

## What's in here so far

```
Assets/Scripts/Objects/
    ObjectPlacer.cs      places, turns and deletes things using a mouse raycast
    PlacedObject.cs      marker so we only delete what the user added
    FractalGrammar.cs    the rules for each fractal (from textbook section 8.2)
    LSystemPlant.cs      draws the fractals with turtle graphics
Assets/Editor/
    StarterSceneBuilder.cs   menu button that sets the scene up
```

There's a **TerraScape** menu at the top of the Unity window with **Build Starter Scene**
in it, in case anyone needs to rebuild the scene from scratch.

---

## Working together

- Everyone works on their own branch: `loi-terrain`, `syed-environment`, `anisha-ui`,
  `nabintou-objects`
- Pull from main before you start each time
- Push your branch and open a pull request, don't commit straight to main
- **Only one person in the scene file at a time.** Say something in Discord before you
  open it. Unity scene files don't merge, they just overwrite each other.
- Keep your scripts in your own folder: `Scripts/Terrain/`, `Scripts/Environment/`,
  `Scripts/Camera/`, `Scripts/Objects/`

---

## Where the course material shows up

| Chapter | Where we use it |
|---|---|
| 3 — Transformations | Moving, turning and scaling the objects you place |
| 4 — Bézier curves | Camera flythrough path |
| 5 — Perspective, 3D data structures | The camera, and how the terrain mesh is stored |
| 6 — Hidden surfaces | Vertex normals and depth sorting on the terrain |
| 7 — Colour and shading | Day/night colour blending, colouring terrain by height and slope |
| 8 — Fractals | The L-system trees, and the diamond-square terrain generator |

The fractal code is based on the `FractalGrammars.java` example from class and the
grammars listed in section 8.2 of Ammeraal & Zhang. The camera path work is based on the
`Bezier.java` example and section 4.6. Both were rewritten in C# for Unity — details are
in the comments at the top of those files.

---

## Dates

- Feature freeze: **Aug 4**
- Presentation and demo: **Mon Aug 10**
- Final report and code: **Wed Aug 12**
