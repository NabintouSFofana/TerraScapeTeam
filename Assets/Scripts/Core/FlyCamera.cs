using UnityEngine;

// Basic fly camera controls
public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float lookSpeed = 2f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            pitch = Mathf.Clamp(pitch, -80f, 80f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement =
            transform.right * horizontal +
            transform.forward * vertical;

        if (Input.GetKey(KeyCode.E))
            movement += Vector3.up;

        if (Input.GetKey(KeyCode.Q))
            movement += Vector3.down;

        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}