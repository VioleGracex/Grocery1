using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Shelf))]
public class ShelfEditor : Editor
{
    private bool autoDepthAndWidthPrevValue;

    public override void OnInspectorGUI()
    {
        Shelf shelf = (Shelf)target;

        EditorGUILayout.LabelField("Shelf Area", EditorStyles.boldLabel);
        shelf.width = EditorGUILayout.FloatField("Width", shelf.width);
        shelf.height = EditorGUILayout.FloatField("Height", shelf.height);
        shelf.depth = EditorGUILayout.FloatField("Depth", shelf.depth);
        shelf.gap = EditorGUILayout.FloatField("Gap", shelf.gap);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Position Offset", EditorStyles.boldLabel);
        shelf.positionOffset = EditorGUILayout.Vector3Field("Offset", shelf.positionOffset);
        shelf.yOffset = EditorGUILayout.FloatField("Y Offset", shelf.yOffset);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        shelf.cellSize = EditorGUILayout.Vector2Field("Cell Size", shelf.cellSize);

        EditorGUILayout.Space();
        shelf.autoDepthAndWidth = EditorGUILayout.Toggle("Auto Depth and Width", shelf.autoDepthAndWidth);

        if (shelf.autoDepthAndWidth != autoDepthAndWidthPrevValue)
        {
            shelf.AdjustDepthAndWidth();
            autoDepthAndWidthPrevValue = shelf.autoDepthAndWidth;
            SceneView.RepaintAll();
            EditorUtility.SetDirty(shelf);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
        shelf.itemCount = EditorGUILayout.IntField("Item Count", shelf.itemCount);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visualization Settings", EditorStyles.boldLabel);
        shelf.showGizmos = EditorGUILayout.Toggle("Show Gizmos", shelf.showGizmos);
        shelf.showGrid = EditorGUILayout.Toggle("Show Grid", shelf.showGrid);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Product Pool", EditorStyles.boldLabel);
        SerializedProperty productPool = serializedObject.FindProperty("productPool");
        EditorGUILayout.PropertyField(productPool);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Place Items"))
        {
            shelf.PlaceItemsOnShelf();
        }

        if (GUILayout.Button("Clear Children"))
        {
            ClearChildren(shelf);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(shelf);
        }
    }

    private void ClearChildren(Shelf shelf)
    {
        for (int i = shelf.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(shelf.transform.GetChild(i).gameObject);
        }
    }
}