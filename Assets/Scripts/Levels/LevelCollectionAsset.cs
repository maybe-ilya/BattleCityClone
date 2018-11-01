using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IndexedLevel : SerializedPair<int, LevelAsset> { }

[Serializable]
public class LevelCollection : SerializedDictionary<IndexedLevel, int, LevelAsset> { }

[CreateAssetMenu (fileName = "NewLevelCollection")]
public class LevelCollectionAsset : ScriptableObject {
#pragma warning disable 649,414
	[SerializeField]
	private LevelCollection collection = new LevelCollection ();
#pragma warning restore 649,414
}