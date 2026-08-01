using UnityEngine;

// Places and deletes props on the terrain with the mouse (ray casting).
// Left-click  = place a tree (procedural L-system) or a rock, depending on Kind
// Right-click = delete the placed object under the cursor
// Owner: Nabintou (object placement).

public class ObjectPlacer : MonoBehaviour
{
    public enum Kind { Tree, Rock, Dragon }
    public Kind kind = Kind.Tree;
    public Camera cam;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryPlace();
        else if (Input.GetMouseButtonDown(1)) TryDelete();
    }

    void TryPlace()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if (kind == Kind.Rock)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name = "Rock";
            g.transform.position = hit.point - Vector3.up * 0.2f;
            float s = Random.Range(0.8f, 2.2f);
            g.transform.localScale = new Vector3(s * 1.4f, s, s * 1.4f);
            var mr = g.GetComponent<MeshRenderer>();
            if (mr != null) mr.material.color = new Color(0.42f, 0.40f, 0.38f);
            g.AddComponent<PlacedObject>();
        }
        else // Tree or Dragon — grow a fractal string grammar
        {
            bool isDragon = kind == Kind.Dragon;
            var g = new GameObject(isDragon ? "Dragon" : "Tree");
            g.transform.position = hit.point;
            var plant = g.AddComponent<LSystemPlant>();
            plant.preset = isDragon ? LSystemPlant.Preset.Dragon : LSystemPlant.Preset.Tree1;
            plant.iterations = isDragon ? 10 : 4;
            plant.initialLength = isDragon ? 1.2f : Random.Range(5f, 8f);
            plant.Build();

            // Give it a rough collider so it can be clicked to delete.
            var box = g.AddComponent<BoxCollider>();
            var mf = g.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                box.center = mf.sharedMesh.bounds.center;
                box.size = mf.sharedMesh.bounds.size + Vector3.one * 0.3f;
            }
            g.AddComponent<PlacedObject>();
        }
    }

    void TryDelete()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var placed = hit.collider.GetComponentInParent<PlacedObject>();
            if (placed != null) Destroy(placed.gameObject);
        }
    }
}
