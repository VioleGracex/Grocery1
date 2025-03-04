using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class PlayerGrabSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform cameraTransform;
    public float pickupRange = 3f;
    public LayerMask pickupLayer;
    public float minPickupDistance = 1.5f; // Minimum distance for picking up items

    [Header("Holding Settings")]
    private GameObject grabbedItem;
    private Rigidbody grabbedRb;
    private Vector3 initialPickupPosition; // Store original pickup position
    [SerializeField] private float holdDistance = 5f; // Fixed holding distance

    public float minHoldDistance = 2f; // Minimum distance to hold the item from the camera
    public float maxHoldDistance = 7f;
    public float throwForce = 10f;
    public float liftSpeed = 0.1f;
    public float lowerSpeed = 0.1f;
    public float horizontalMoveSpeed = 5f; // Speed at which the object follows camera left/right
    public float holdHeightAdjustment = 0.5f; // Adjust height based on weight

    private bool isLifting = false;
    private bool isLowering = false;

    [Header("Physics Settings")]
    public float objectWeightFactor = 0.1f; // Affects how much weight influences holding height
    public float grabSmoothing = 10f; // Smooth movement to prevent jittering

    [Header("UI Settings")]
    public Image dotImage;
    public Color defaultDotColor = Color.white;
    public Color highlightDotColor = Color.green;

    private LineRenderer lineRenderer;
    private ItemGrabbable lastHighlightedItem;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        HandleHolding();
        UpdateDotColor();
        UpdateLineRenderer();
        HighlightItem();
    }

    // Called by UI Button (Grab/Drop)
    public void ToggleGrabDrop()
    {
        if (grabbedItem == null)
        {
            TryPickup();
        }
        else
        {
            DropItem();
        }
    }

    private void TryPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, pickupLayer))
        {
            // Store the original pickup position
            initialPickupPosition = hit.point;

            grabbedItem = hit.collider.gameObject;
            grabbedRb = grabbedItem.GetComponent<Rigidbody>();

            if (grabbedRb != null)
            {
                grabbedRb.velocity = Vector3.zero; // Stop movement
                grabbedRb.angularVelocity = Vector3.zero; // Stop spinning
                grabbedRb.useGravity = false; // Disable gravity while holding

                // Freeze rotation constraints
                grabbedRb.constraints = RigidbodyConstraints.FreezeRotation;

                FindObjectOfType<UIManager>().SetUIState(true);
            }
        }
    }

    private void HandleHolding()
    {
        if (grabbedItem)
        {
            Vector3 holdPosition = cameraTransform.position + cameraTransform.forward * holdDistance;
            holdPosition.y += holdHeightAdjustment - grabbedRb.mass * objectWeightFactor;

            // Ensure the item is not too close to the camera
            float distanceToCamera = Vector3.Distance(cameraTransform.position, holdPosition);
            if (distanceToCamera < minHoldDistance)
            {
                holdPosition = cameraTransform.position + cameraTransform.forward * minHoldDistance;
            }

            if (isLifting)
            {
                holdPosition.y += liftSpeed;
            }
            else if (isLowering)
            {
                holdPosition.y -= lowerSpeed;
            }

            // Smoothly move the object to the target position
            grabbedRb.MovePosition(Vector3.Lerp(grabbedItem.transform.position, holdPosition, Time.deltaTime * grabSmoothing));

            // Prevent clipping through objects
            PreventClipping(holdPosition);
        }
    }

    private void PreventClipping(Vector3 targetPosition)
    {
        RaycastHit hit;
        if (Physics.Linecast(cameraTransform.position, targetPosition, out hit, ~pickupLayer))
        {
            // Adjust position to avoid clipping
            Vector3 adjustedPosition = hit.point - (cameraTransform.forward * 0.2f);
            float distanceToCamera = Vector3.Distance(cameraTransform.position, adjustedPosition);
            if (distanceToCamera < minHoldDistance)
            {
                adjustedPosition = cameraTransform.position + cameraTransform.forward * minHoldDistance;
            }
            grabbedItem.transform.position = adjustedPosition;
        }
    }

    private void DropItem()
    {
        if (grabbedRb != null)
        {
            grabbedRb.useGravity = true; // Re-enable gravity
            grabbedRb.constraints = RigidbodyConstraints.None; // Release rotation constraints
        }
        grabbedItem = null;
        grabbedRb = null;
        FindObjectOfType<UIManager>().SetUIState(false);
    }

    public void ThrowItem()
    {
        if (grabbedItem)
        {
            grabbedRb.useGravity = true;
            grabbedRb.constraints = RigidbodyConstraints.None; // Release rotation constraints
            grabbedRb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
            grabbedItem = null;
            grabbedRb = null;
            FindObjectOfType<UIManager>().SetUIState(false);
        }
    }

    private void UpdateDotColor()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, pickupLayer))
        {
            dotImage.color = highlightDotColor;
        }
        else
        {
            dotImage.color = defaultDotColor;
        }
    }

    private void UpdateLineRenderer()
    {
        if (cameraTransform != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, cameraTransform.position);
            lineRenderer.SetPosition(1, cameraTransform.position + cameraTransform.forward * pickupRange);
        }
    }

    private void OnDrawGizmos()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * pickupRange);
        }
    }

    private void HighlightItem()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, pickupLayer))
        {
            ItemGrabbable itemGrabbable = hit.collider.GetComponent<ItemGrabbable>();
            if (itemGrabbable != null)
            {
                if (lastHighlightedItem != null && lastHighlightedItem != itemGrabbable)
                {
                    lastHighlightedItem.SetOutline(false);
                }
                itemGrabbable.SetOutline(true);
                lastHighlightedItem = itemGrabbable;
            }
        }
        else if (lastHighlightedItem != null)
        {
            lastHighlightedItem.SetOutline(false);
            lastHighlightedItem = null;
        }
    }

    public void LiftItem()
    {
        isLifting = true;
        isLowering = false;
    }

    public void LowerItem()
    {
        isLowering = true;
        isLifting = false;
    }

    public void StopLiftingLowering()
    {
        isLifting = false;
        isLowering = false;
    }
}