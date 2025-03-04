using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName;
    public string category;
    public int id;
    public float weight;
    public int maxHP;
    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
