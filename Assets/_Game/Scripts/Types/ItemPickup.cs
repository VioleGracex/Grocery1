using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Transform holdPosition;
    private GameObject grabbedItem;
    private Rigidbody grabbedRb;

    public float throwForce = 10f;
    public float minHoldDistance = 1f;
    public float maxHoldDistance = 5f;
    private float currentHoldDistance = 2f;

    private void Update()
    {
        if (grabbedItem)
        {
            grabbedItem.transform.position = holdPosition.position + holdPosition.forward * currentHoldDistance;

            if (Input.GetKey(KeyCode.Q)) // Move item closer
                currentHoldDistance = Mathf.Max(minHoldDistance, currentHoldDistance - 0.1f);
            if (Input.GetKey(KeyCode.E)) // Move item further
                currentHoldDistance = Mathf.Min(maxHoldDistance, currentHoldDistance + 0.1f);
            
            if (Input.GetMouseButtonDown(1)) // Right click to throw
            {
                ThrowItem();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") && !grabbedItem)
        {
            grabbedItem = other.gameObject;
            grabbedRb = grabbedItem.GetComponent<Rigidbody>();
            grabbedRb.isKinematic = true;
        }
    }

    private void ThrowItem()
    {
        grabbedRb.isKinematic = false;
        grabbedRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        grabbedItem = null;
        grabbedRb = null;
    }
}
