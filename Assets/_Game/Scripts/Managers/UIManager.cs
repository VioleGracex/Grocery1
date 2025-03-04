using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public GameObject grabDropButton; // One button for both Grab and Drop
    public GameObject throwButton;
    public Button liftButton;
    public Button lowerButton;
    public TextMeshProUGUI grabDropButtonText; // Text label for the Grab/Drop button
    private PlayerGrabSystem playerGrabSystem;

    public static UIManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        playerGrabSystem = FindObjectOfType<PlayerGrabSystem>();
        SetUIState(false);
    }

    // Set the UI state for holding an item
    public void SetUIState(bool isHoldingItem)
    {
        if (isHoldingItem)
        {
            grabDropButtonText.text = "Drop";
            throwButton.SetActive(true);
           /*  liftButton.gameObject.SetActive(true);
            lowerButton.gameObject.SetActive(true); */
        }
        else
        {
            grabDropButtonText.text = "Grab";
            throwButton.SetActive(false);
            /* liftButton.gameObject.SetActive(false);
            lowerButton.gameObject.SetActive(false); */
        }
    }
}