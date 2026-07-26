using UnityEngine;

// Lets the user put objects on the ground with the mouse.
//   Left click                   place a tree or a rock
//   Scroll wheel over an object  turn it
//   Right click on an object     delete it
public class ObjectPlacer : MonoBehaviour
{
    public enum Kind { Tree, Rock, Dragon }

    [Tooltip("What gets placed on the next left click.")]
    public Kind kind = Kind.Tree;

    [Tooltip("Degrees turned per notch of the scroll wheel.")]
    public float rotateSpeed = 15f;

    public Camera cam;

    void Awake()
    {
        // If nobody assigned a camera in the Inspector, just grab the main one.
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) Place();
        else if (Input.GetMouseButtonDown(1)) Delete();
        else
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f) Rotate(scroll);
        }
    }

    // Fire a ray at whatever is under the mouse. Returns false if it hit nothing.
    bool RayFromMouse(out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit);
    }

    void Place()
    {
        if (!RayFromMouse(out RaycastHit hit)) return;

        if (kind == Kind.Rock) PlaceRock(hit.point);
        else PlacePlant(hit.point, kind == Kind.Dragon);
    }

    // A rock is just a squashed sphere. Random size and turn so they don't all look
    // identical - the scaling and rotating here is the transformation material from
    // chapter 3.
    void PlaceRock(Vector3 point)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";

        // Sink it slightly so it looks like it is sitting in the ground, not on it.
        rock.transform.position = point - Vector3.up * 0.2f;
        rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        float size = Random.Range(0.8f, 2.2f);
        rock.transform.localScale = new Vector3(size * 1.4f, size, size * 1.4f);

        MeshRenderer mr = rock.GetComponent<MeshRenderer>();
        if (mr != null) mr.material.color = new Color(0.42f, 0.40f, 0.38f);

        rock.AddComponent<PlacedObject>();
    }

    // Trees and dragons are both drawn by LSystemPlant, just with different rules.
    void PlacePlant(Vector3 point, bool isDragon)
    {
        GameObject go = new GameObject(isDragon ? "Dragon" : "Tree");
        go.transform.position = point;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        LSystemPlant plant = go.AddComponent<LSystemPlant>();
        plant.preset = isDragon ? LSystemPlant.Preset.Dragon : LSystemPlant.Preset.Tree1;
        plant.iterations = isDragon ? 10 : 4;
        plant.initialLength = isDragon ? 1.2f : Random.Range(5f, 8f);
        plant.Build();

        // The plant is drawn as lines
        // around it so the user can still click it to turn or delete it.
        BoxCollider box = go.AddComponent<BoxCollider>();
        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            box.center = mf.sharedMesh.bounds.center;
            box.size = mf.sharedMesh.bounds.size + Vector3.one * 0.3f;
        }

        go.AddComponent<PlacedObject>();
    }

    // Turn whatever object the mouse is currently hovering over.
    void Rotate(float scroll)
    {
        if (!RayFromMouse(out RaycastHit hit)) return;

        PlacedObject placed = hit.collider.GetComponentInParent<PlacedObject>();
        if (placed == null) return;

        placed.transform.Rotate(Vector3.up, scroll * rotateSpeed * 100f, Space.World);
    }

    // Only delete things the user placed. Without the PlacedObject check you could
    // right click the ground and delete the whole terrain by accident.
    void Delete()
    {
        if (!RayFromMouse(out RaycastHit hit)) return;

        PlacedObject placed = hit.collider.GetComponentInParent<PlacedObject>();
        if (placed != null) Destroy(placed.gameObject);
    }

    // For the UI buttons once Anisha's menu is in.
    public void SetKind(int k) => kind = (Kind)k;
}
