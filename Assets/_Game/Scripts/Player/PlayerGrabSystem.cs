using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private bool usePhysics = true; // Toggle physics-based holding

    public float minHoldDistance = 2f; // Minimum distance to hold the item from the camera
    public float maxHoldDistance = 7f;
    public float throwForce = 10f;
    public float liftSpeed = 0.1f;
    public float lowerSpeed = 0.1f;
    public float horizontalMoveSpeed = 5f; // Speed at which the object follows camera left/right
    public float holdHeightAdjustment = 0.5f; // Adjust height based on weight

    [Header("Physics Settings")]
    public float objectWeightFactor = 0.1f; // Affects how much weight influences holding height
    public float grabSmoothing = 10f; // Smooth movement to prevent jittering

    [Header("UI Settings")]
    public Image dotImage;
    public Color defaultDotColor = Color.white;
    public Color highlightDotColor = Color.green;

    private LineRenderer lineRenderer;
    private ItemGrabbable lastHighlightedItem;
    private Interactable lastInteractable;

    void Awake()
    {
        /* lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false; */
    }

    void Update()
    {
        HandleHolding();
        UpdateDotColor();
        /* UpdateLineRenderer(); */
        HighlightAndGrabOrInteractItem();
    }

    // Called by UI Button (Grab/Drop/Interact)
    public void OnGrabDropInteract()
    {
        if (grabbedItem == null)
        {
            TryPickupOrInteract();
        }
        else
        {
            DropItem();
        }
    }

    private void TryPickupOrInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, pickupLayer))
        {
            // Try to interact first
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact();
                return;
            }

            // If not interactable, try to grab
            ItemGrabbable itemGrabbable = hit.collider.GetComponent<ItemGrabbable>();
            if (itemGrabbable != null)
            {
                // Store the original pickup position
                initialPickupPosition = hit.point;

                grabbedItem = itemGrabbable.gameObject;
                grabbedRb = grabbedItem.GetComponent<Rigidbody>();

                if (grabbedRb != null)
                {
                    grabbedRb.velocity = Vector3.zero; // Stop movement
                    grabbedRb.angularVelocity = Vector3.zero; // Stop spinning

                    if (usePhysics)
                    {
                        grabbedRb.useGravity = false; // Disable gravity while holding

                        // Freeze rotation constraints
                        grabbedRb.constraints = RigidbodyConstraints.FreezeRotation;
                    }
                    else
                    {
                        grabbedRb.isKinematic = true; // Make kinematic to avoid physical interactions
                    }

                    FindObjectOfType<UIManager>().SetUIState(true);
                }
            }
        }
    }

    private void HandleHolding()
    {
        if (grabbedItem)
        {
            Vector3 holdPosition = cameraTransform.position + cameraTransform.forward * holdDistance;

            if (usePhysics)
            {
                holdPosition.y += holdHeightAdjustment - grabbedRb.mass * objectWeightFactor;

                // Ensure the item is not too close to the camera
                float distanceToCamera = Vector3.Distance(cameraTransform.position, holdPosition);
                if (distanceToCamera < minHoldDistance)
                {
                    holdPosition = cameraTransform.position + cameraTransform.forward * minHoldDistance;
                }

                // Smoothly move the object to the target position
                grabbedRb.MovePosition(Vector3.Lerp(grabbedItem.transform.position, holdPosition, Time.deltaTime * grabSmoothing));

                // Prevent clipping through objects
                PreventClipping(holdPosition);
            }
            else
            {
                // Directly move the item to hold position without physics
                grabbedItem.transform.position = holdPosition;
            }
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
            if (usePhysics)
            {
                grabbedRb.useGravity = true; // Re-enable gravity
                grabbedRb.constraints = RigidbodyConstraints.None; // Release rotation constraints
            }
            else
            {
                grabbedRb.isKinematic = false; // Disable kinematic mode
            }
        }
        grabbedItem = null;
        grabbedRb = null;
        FindObjectOfType<UIManager>().SetUIState(false);
    }

    public void ThrowItem()
    {
        if (grabbedItem)
        {
            // Check if the item is too close to any surface
            if (IsSurfaceTooClose())
            {
                return; // Do not throw the item if a surface is too close
            }

            grabbedRb.useGravity = true;
            grabbedRb.isKinematic = false;
            grabbedRb.constraints = RigidbodyConstraints.None; // Release rotation constraints
            grabbedRb.AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
            grabbedItem = null;
            grabbedRb = null;
            FindObjectOfType<UIManager>().SetUIState(false);
        }
    }

    private bool IsSurfaceTooClose()
    {
        RaycastHit hit;
        return Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, minHoldDistance, ~pickupLayer);
    }

    private void UpdateDotColor()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, pickupLayer))
        {
            // Check for interactable
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                dotImage.color = highlightDotColor;
                return;
            }

            // Check for grabbable
            ItemGrabbable itemGrabbable = hit.collider.GetComponent<ItemGrabbable>();
            if (itemGrabbable != null)
            {
                dotImage.color = highlightDotColor;
                return;
            }
        }

        dotImage.color = defaultDotColor;
    }

    private void HighlightAndGrabOrInteractItem()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, pickupLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (lastInteractable != null && lastInteractable != interactable)
                {
                    lastInteractable.SetOutline(false);
                }
                interactable.SetOutline(true);
                lastInteractable = interactable;

                return;
            }

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
        else
        {
            if (lastInteractable != null)
            {
                lastInteractable.SetOutline(false);
                lastInteractable = null;
            }

            if (lastHighlightedItem != null)
            {
                lastHighlightedItem.SetOutline(false);
                lastHighlightedItem = null;
            }
        }
    }
}