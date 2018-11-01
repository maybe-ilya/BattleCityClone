using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (LevelCollectionAsset))]
[TableEditorProperties("collection.pairs")]
public class LevelCollectionAssetEditor : TableEditor { }