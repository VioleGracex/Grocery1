using UnityEngine;

public class Door : Interactable
{
    private bool isOpen = false;
    [SerializeField] private Outline myOutline;
    private Color originalOutlineColor;
    
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalOutlineColor = myOutline.OutlineColor;
    }

    public override void Interact()
    {
        if (animator != null)
        {
            isOpen = !isOpen;
            animator.SetBool("isOpen", isOpen);
        }
    }
    public override void SetOutline(bool isHighlighted)
    {
        myOutline.OutlineColor = isHighlighted ? Color.green : originalOutlineColor;
    }
}