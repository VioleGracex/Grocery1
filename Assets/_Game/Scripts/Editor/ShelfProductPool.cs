using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShelfProductPool", menuName = "Shelf/Product Pool")]
public class ShelfProductPool : ScriptableObject
{
    public List<GameObject> itemPrefabs; // List of item prefabs to instantiate
}