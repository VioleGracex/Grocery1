using UnityEngine;

public class ItemGrabbable : Item
{
    private Rigidbody rb;
    private Vector3 lastPosition;
    private float speedThreshold = 10f;
    private float cooldownTime = 0.2f;
    private float lastDamageTime;
    [SerializeField] private bool isInCart = false;
    [SerializeField] private Outline myOutline;
    private Color originalOutlineColor;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        lastDamageTime = -cooldownTime;
        myOutline = GetComponent<Outline>();
        originalOutlineColor = myOutline.OutlineColor;
    }

    private void Update()
    {
        if (!isInCart)
        {
            CheckSpeedAndCollisions();
        }
    }

    private void CheckSpeedAndCollisions()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (speed > speedThreshold && Time.time > lastDamageTime + cooldownTime)
        {
            int damage = Mathf.CeilToInt(weight * speed * 0.1f);
            TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb.velocity.magnitude > speedThreshold && Time.time > lastDamageTime + cooldownTime)
        {
            int damage = Mathf.CeilToInt(weight * rb.velocity.magnitude * 0.1f);
            TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }

    public void OnThrow()
    {
        if (rb.velocity.magnitude > speedThreshold && Time.time > lastDamageTime + cooldownTime)
        {
            int damage = Mathf.CeilToInt(weight * rb.velocity.magnitude * 0.1f);
            TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }

    public void SetInCart(bool inCart)
    {
        isInCart = inCart;
        if (isInCart)
        {
            //Truck.Instance.AddItem(this);
        }
    }

    public void SetOutline(bool isHighlighted)
    {
        myOutline.OutlineColor = isHighlighted ? Color.green : originalOutlineColor;
    }
}