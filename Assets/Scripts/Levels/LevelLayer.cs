using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLayer : ScriptableObject {
#pragma warning disable 649, 414
    [SerializeField, HideInInspector]
    private LevelLayerData data = new LevelLayerData();
#pragma warning restore 649, 414

    public void Resize(Vector2Int newSize) {
        data.Resize(newSize);
    }

    public int this [Vector2Int point] {
        get { return data[point.x, point.y]; }
        set { data[point.x, point.y] = value; }
    }

    public int this [int x, int y] {
        get { return data[x, y]; }
        set { data[x, y] = value; }
    }
}

[Serializable]
public class LevelLayerData : ISerializationCallbackReceiver {
#pragma warning disable 649, 414
    [Serializable]
    public class LevelLayerLine {
        [SerializeField]
        public List<int> values = new List<int>();
    }

    [SerializeField]
    private List<LevelLayerLine> lines = new List<LevelLayerLine>();

    [NonSerialized]
    private int[][] data = new int[0][];
#pragma warning restore 649, 414

    public void Resize(Vector2Int newSize) {
        Array.Resize(ref data, newSize.x);
        for (var i = 0; i < newSize.x; i++) {
            var elem = data[i];
            if (elem == null) {
                elem = new int[0];
            }
            Array.Resize(ref elem, newSize.y);
            data[i] = elem;
        }
    }

    public void OnBeforeSerialize() {
        lines.Clear();
        for (var i = 0; i < data.Length; i++) {
            var line = new LevelLayerLine();
            line.values.AddRange(data[i]);
            lines.Add(line);
        }
    }

    public void OnAfterDeserialize() {
        data = new int[lines.Count][];
        for (var x = 0; x < lines.Count; x++) {
            var elem = lines[x];
            data[x] = elem.values.ToArray();
        }
    }

    public int this [int x, int y] {
        get {
            return data[x][y];
        }
        set {
            data[x][y] = value;
        }
    }
}