using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject {
#pragma warning disable 649,414
	[SerializeField]
	private LevelCollectionAsset defaultLevels;
#pragma warning restore 649,414
}