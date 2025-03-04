using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform playerBody; // Reference to player (parent of camera)

    [Header("Camera Settings")]
    public float sensitivity = 100f; // Default sensitivity
    public float verticalClamp = 80f; // Limit up/down look range
    public Slider sensitivitySlider; // UI Slider to adjust sensitivity

    [Header("Touch Settings")]
    public float deadZoneWidth = 0.4f; // Percentage of the left screen that will not rotate the camera (0.4 = 40%)
    public bool enableDragging = true; // Allow dragging or not

    private float rotationX = 0f; // Track vertical rotation
    private bool isDragging = false;

    void Start()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(AdjustSensitivity);
            sensitivitySlider.value = sensitivity; // Set initial value
        }
    }

    void Update()
    {
        if (enableDragging)
            HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            // Ignore camera movement if touch is in the left dead zone
            if (touch.position.x < Screen.width * deadZoneWidth)
                return;

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.deltaPosition;
                float mouseX = delta.x * sensitivity * Time.deltaTime;
                float mouseY = delta.y * sensitivity * Time.deltaTime;

                // Rotate around Y-axis (horizontal movement)
                playerBody.Rotate(Vector3.up * mouseX);

                // Rotate around X-axis (vertical movement)
                rotationX -= mouseY;
                rotationX = Mathf.Clamp(rotationX, -verticalClamp, verticalClamp);

                transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    // Function to adjust sensitivity via UI Slider
    public void AdjustSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
    }
}
