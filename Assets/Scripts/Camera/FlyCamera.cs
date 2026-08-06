using UnityEngine;

// Free-fly camera for exploring the scene.
//   Right mouse (hold) + move  = look around
//   W / A / S / D              = move
//   Q / E                      = down / up
//   Left Shift                 = move faster
// Owner: Anisha (camera controls).

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float fastMultiplier = 3f;
    public float lookSensitivity = 3f;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void Update()
    {
        // Look only while holding the right mouse button, so the mouse is free
        // for sculpting and clicking UI the rest of the time.
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.E)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) dir -= Vector3.up;

        transform.position += dir.normalized * speed * Time.deltaTime;
    }
}