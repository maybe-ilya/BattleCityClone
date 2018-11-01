using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAsset : ScriptableObject {
#pragma warning disable 649,414
    [SerializeField]
    private Vector2Int fieldSize;
    [SerializeField]
    private List<LevelLayer> layers = new List<LevelLayer>();
#pragma warning restore 649,414

    public Vector2Int FieldSize { get { return fieldSize; } }

    public void SetFieldSize(Vector2Int newFieldSize) {
        this.fieldSize = newFieldSize;
        for (var i = 0; i < layers.Count; i++) {
            layers[i].Resize(newFieldSize);
        }
    }

    public void AddLayer(LevelLayer layer) {
        layers.Add(layer);
    }

    public bool Contains(Vector2Int point) {
        for (var i = 0; i < 2; i++) {
            var value = point[i];
            if (value < 0 || value >= fieldSize[i]) {
                return false;
            }
        }
        return true;
    }

    public LevelLayer this [int i] {
        get { return layers[i]; }
    }

    public int LayerCount { get { return layers.Count; } }
}