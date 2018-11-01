using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class LevelEditorSettings : ScriptableObject {
#pragma warning disable 649,414
    [SerializeField]
    private Vector2Int defaultFieldSize;
    [SerializeField]
    private Vector2 tileSize;

    [SerializeField, Range(0.0f, 1.0f)]
    private float fieldRatio;
    [SerializeField]
    private int defaultLayerCount;
    [SerializeField]
    private Material spritesMaterial, linesMaterial;

    [SerializeField, TypeReferenceBase(typeof(LevelEditorTool))]
    private TypeReference[] toolsetEditors;
    [SerializeField]
    private bool done = false;
#pragma warning restore 649,414

    public Vector2Int DefaultFieldSize { get { return defaultFieldSize; } }
    public Vector2 TileSize { get { return tileSize; } }

    public float FieldRatio { get { return fieldRatio; } }
    public int DefaultLayerCount { get { return defaultLayerCount; } }

    public Material SpritesMaterial { get { return spritesMaterial; } }
    public Material LinesMaterial { get { return linesMaterial; } }

    public TypeReference[] GetToolsetEditorReferences() {
        return toolsetEditors;
    }
}