using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public UltimateJoystick moveJoystick; // Joystick for movement
    public Rigidbody rb;
    public float speed = 5f;
    public float jumpForce = 5f;
    private bool canJump = true;

    public Transform cameraTransform; // Reference to the Main Camera

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        float moveX = moveJoystick.GetHorizontalAxis();
        float moveZ = moveJoystick.GetVerticalAxis();

        // Get the camera's forward and right direction (ignoring vertical tilt)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0; // Remove vertical movement (to avoid tilting issues)
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction based on camera orientation
        Vector3 moveDirection = (forward * moveZ + right * moveX) * speed;

        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
    }

    public void Jump()
    {
        if (canJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            canJump = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        canJump = true;
    }
}
