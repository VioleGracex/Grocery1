using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    public ShelfProductPool productPool; // Reference to the product pool
    public float width = 10f;
    public float height = 5f;
    public float depth = 2f;
    public float gap = 0.5f; // Gap between items
    public int itemCount; // Number of items to place on the shelf
    public Vector3 positionOffset; // Position offset for item placement
    public float yOffset = 0.1f; // Small offset to prevent clipping
    public bool autoDepthAndWidth = false; // Auto adjust depth and width based on mesh plane

    private List<GameObject> placedItems = new List<GameObject>();
    private List<Color> gridColors = new List<Color>();
    private bool colorsInitialized = false;

    private void Start()
    {
        AdjustDepthAndWidth();
        //PlaceItemsOnShelf();
    }

    public void PlaceItemsOnShelf()
    {
        AdjustDepthAndWidth();
        UpdatePlacedItems();

        // Starting positions for item placement
        float currentWidth = positionOffset.x - width / 2;
        float currentHeight = positionOffset.y;
        float currentDepth = positionOffset.z - depth / 2;
        float maxHeightInRow = 0f;
        float maxDepthInRow = 0f;

        for (int i = 0; i < itemCount; i++)
        {
            GameObject itemPrefab = productPool.itemPrefabs[Random.Range(0, productPool.itemPrefabs.Count)];
            GameObject item = Instantiate(itemPrefab, transform);

            // Save the original scale and set it after instantiating
            Vector3 originalScale = item.transform.localScale;
            item.transform.localScale = new Vector3(originalScale.x / transform.localScale.x, originalScale.y / transform.localScale.y, originalScale.z / transform.localScale.z);

            Vector3 itemSize = GetItemSize(item);

            // Check if the item can be rotated to fit the shelf
            bool canFit = CheckIfItemCanFit(itemSize, ref currentWidth, ref currentDepth, ref currentHeight, ref maxHeightInRow, ref maxDepthInRow);
            if (!canFit)
            {
                item.transform.Rotate(0, 90, 0);
                itemSize = GetItemSize(item);
                canFit = CheckIfItemCanFit(itemSize, ref currentWidth, ref currentDepth, ref currentHeight, ref maxHeightInRow, ref maxDepthInRow);
            }

            if (!canFit)
            {
                Debug.LogWarning("Not enough space on the shelf for more items.");
                DestroyImmediate(item);
                break;
            }

            item.transform.localPosition = new Vector3(currentWidth, currentHeight + yOffset, currentDepth);
            placedItems.Add(item);
            currentWidth += itemSize.x + gap;

            maxHeightInRow = Mathf.Max(maxHeightInRow, itemSize.y);
            maxDepthInRow = Mathf.Max(maxDepthInRow, itemSize.z);

            // Check if need to move to the next row
            if (currentWidth + itemSize.x > width / 2)
            {
                currentWidth = positionOffset.x - width / 2;
                currentDepth += maxDepthInRow + gap;
                maxDepthInRow = 0f;

                // Check if need to move to the next column
                if (currentDepth + itemSize.z > depth / 2)
                {
                    currentDepth = positionOffset.z - depth / 2;
                    currentHeight += maxHeightInRow + gap;
                    maxHeightInRow = 0f;
                }
            }
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

    private bool CheckIfItemCanFit(Vector3 itemSize, ref float currentWidth, ref float currentDepth, ref float currentHeight, ref float maxHeightInRow, ref float maxDepthInRow)
    {
        if (currentWidth + itemSize.x > width / 2)
        {
            currentWidth = positionOffset.x - width / 2;
            currentDepth += maxDepthInRow + gap;
            maxDepthInRow = 0f;
        }

        if (currentDepth + itemSize.z > depth / 2)
        {
            currentDepth = positionOffset.z - depth / 2;
            currentHeight += maxHeightInRow + gap;
            maxHeightInRow = 0f;
        }

        if (currentHeight + itemSize.y > height)
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
            for (float x = 0; x < width; x += gap)
            {
                for (float z = 0; z < depth; z += gap)
                {
                    gridColors.Add(new Color(Random.value, Random.value, Random.value));
                }
            }
            colorsInitialized = true;
        }
    }

    private void OnDrawGizmos()
    {
        AdjustDepthAndWidth();
        Gizmos.color = Color.green;
        Vector3 adjustedPosition = transform.position + positionOffset - new Vector3(width / 2, 0, depth / 2);
        Gizmos.DrawWireCube(adjustedPosition + new Vector3(width / 2, height / 2, depth / 2), new Vector3(width, height, depth));

        // Draw grid lines and top-down view
        Gizmos.color = Color.blue;
        for (float i = 0; i <= width; i += gap)
        {
            Gizmos.DrawLine(adjustedPosition + new Vector3(i, 0, 0), adjustedPosition + new Vector3(i, 0, depth));
        }
        for (float i = 0; i <= depth; i += gap)
        {
            Gizmos.DrawLine(adjustedPosition + new Vector3(0, 0, i), adjustedPosition + new Vector3(width, 0, i));
        }

        // Initialize grid colors if not already done
        InitializeGridColors();

        // Draw top-down view squares
        int colorIndex = 0;
        for (float x = 0; x < width; x += gap)
        {
            for (float z = 0; z < depth; z += gap)
            {
                Gizmos.color = gridColors[colorIndex++];
                Gizmos.DrawCube(adjustedPosition + new Vector3(x + gap / 2, height + 0.1f, z + gap / 2), new Vector3(gap, 0.1f, gap));
            }
        }
    }
}