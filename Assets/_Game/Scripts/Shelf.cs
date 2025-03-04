using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    public ShelfProductPool productPool; // Reference to the product pool
    public float width = 10f;
    public float height = 5f;
    public float depth = 2f;
    public float gap = 0.5f; // Gap between cells
    public int itemCount; // Number of items to place on the shelf
    public Vector3 positionOffset; // Position offset for item placement
    public float yOffset = 0.1f; // Small offset to prevent clipping
    public bool autoDepthAndWidth = false; // Auto adjust depth and width based on mesh plane
    public Vector2 cellSize = new Vector2(1f, 1f); // Cell size for the grid (width, depth)

    public bool showGizmos = true; // Toggle for showing Gizmos
    public bool showGrid = true; // Toggle for showing grid

    private List<GameObject> placedItems = new List<GameObject>();
    private List<Color> gridColors = new List<Color>();
    private bool colorsInitialized = false;

    private void Start()
    {
        //AdjustDepthAndWidth();
    }

    public void PlaceItemsOnShelf()
    {
        try
        {
            AdjustDepthAndWidth();
            UpdatePlacedItems();

            float startX = positionOffset.x - width / 2 + cellSize.x / 2;
            float startY = positionOffset.y;
            float startZ = positionOffset.z - depth / 2 + cellSize.y / 2;

            float currentX = startX;
            float currentY = startY;
            float currentZ = startZ;

            for (int i = 0; i < itemCount; i++)
            {
                GameObject itemPrefab = productPool.itemPrefabs[Random.Range(0, productPool.itemPrefabs.Count)];
                GameObject item = Instantiate(itemPrefab, transform);

                Vector3 originalScale = item.transform.localScale;
                item.transform.localScale = new Vector3(originalScale.x / transform.localScale.x, originalScale.y / transform.localScale.y, originalScale.z / transform.localScale.z);

                Vector3 itemSize = GetItemSize(item);

                bool canFit = CheckIfItemCanFit(itemSize, currentX, currentZ, currentY);
                if (!canFit)
                {
                    Debug.LogWarning("Not enough space on the shelf for more items.");
                    DestroyImmediate(item);
                    break;
                }

                item.transform.localPosition = new Vector3(currentX, currentY + yOffset, currentZ);
                placedItems.Add(item);

                currentX += cellSize.x + Mathf.Max(gap, 0);
                if (currentX + cellSize.x / 2 > startX + width)
                {
                    currentX = startX;
                    currentZ += cellSize.y + Mathf.Max(gap, 0);
                    if (currentZ + cellSize.y / 2 > startZ + depth)
                    {
                        currentZ = startZ;
                        currentY += itemSize.y + Mathf.Max(gap, 0);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error placing items on shelf: " + ex.Message);
        }
    }

    public void AdjustDepthAndWidth()
    {
        if (autoDepthAndWidth)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    Bounds bounds = mesh.bounds;
                    depth = bounds.size.z * transform.localScale.z;
                    width = bounds.size.x * transform.localScale.x;
                }
            }
        }
    }

    private void UpdatePlacedItems()
    {
        placedItems.Clear();
        foreach (Transform child in transform)
        {
            placedItems.Add(child.gameObject);
        }
    }

    private bool CheckIfItemCanFit(Vector3 itemSize, float currentX, float currentZ, float currentY)
    {
        if (currentX + itemSize.x / 2 > width / 2 || currentZ + itemSize.z / 2 > depth / 2 || currentY + itemSize.y > height)
        {
            return false;
        }
        return true;
    }

    private Vector3 GetItemSize(GameObject item)
    {
        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null)
        {
            return itemCollider.bounds.size;
        }
        else
        {
            Debug.LogWarning("Item does not have a collider. Using default size.");
            return Vector3.one;
        }
    }

    private void InitializeGridColors()
    {
        if (!colorsInitialized)
        {
            gridColors.Clear();
            for (float x = 0; x < width; x += cellSize.x + Mathf.Max(gap, 0))
            {
                for (float z = 0; z < depth; z += cellSize.y + Mathf.Max(gap, 0))
                {
                    gridColors.Add(new Color(Random.value, Random.value, Random.value));
                }
            }
            colorsInitialized = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        AdjustDepthAndWidth();
        Gizmos.color = Color.green;
        Vector3 adjustedPosition = transform.position + positionOffset - new Vector3(width / 2, 0, depth / 2);
        Gizmos.DrawWireCube(adjustedPosition + new Vector3(width / 2, height / 2, depth / 2), new Vector3(width, height, depth));

        if (showGrid)
        {
            Vector3 gridPosition = transform.position + positionOffset - new Vector3(width / 2, 0, depth / 2);
            Gizmos.color = Color.blue;
            for (float i = 0; i <= width; i += cellSize.x + Mathf.Max(gap, 0))
            {
                Gizmos.DrawLine(gridPosition + new Vector3(i, 0, 0), gridPosition + new Vector3(i, 0, depth));
            }
            for (float i = 0; i <= depth; i += cellSize.y + Mathf.Max(gap, 0))
            {
                Gizmos.DrawLine(gridPosition + new Vector3(0, 0, i), gridPosition + new Vector3(width, 0, i));
            }

            InitializeGridColors();

            int colorIndex = 0;
            for (float x = 0; x < width; x += cellSize.x + Mathf.Max(gap, 0))
            {
                for (float z = 0; z < depth; z += cellSize.y + Mathf.Max(gap, 0))
                {
                    Gizmos.color = gridColors[colorIndex++];
                    Gizmos.DrawCube(gridPosition + new Vector3(x + cellSize.x / 2, yOffset + 0.1f, z + cellSize.y / 2), new Vector3(cellSize.x, 0.1f, cellSize.y));
                }
            }
        }
    }

    public void ClearChildren()
    {
        try
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error clearing children: " + ex.Message);
        }
    }
}