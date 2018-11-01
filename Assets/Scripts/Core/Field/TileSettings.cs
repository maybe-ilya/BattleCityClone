using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileConfig {
	[SerializeField]
	public int id;
	[SerializeField]
	public string Name;
	[SerializeField]
	public Sprite sprite;
	[SerializeField]
	public bool walkable = false;
	[SerializeField]
	public bool breakable = false;
	[SerializeField]
	public int health = 1;
	[SerializeField]
	public int layer = 0;
	[SerializeField]
	public RuntimeAnimatorController anim;
	[SerializeField]
	public float speedModifier = -1;
}

[CreateAssetMenu]
public class TileSettings : ScriptableObject, ISerializationCallbackReceiver {
#pragma warning disable 649
	[SerializeField]
	private Tile defaultTile;
	[SerializeField]
	private TileConfig[] configs;

	[NonSerialized]
	private Dictionary<int, TileConfig> configMap;
#pragma warning restore 649
	public void OnAfterDeserialize() {
		if (configs == null) { return; }
		if (configMap != null) {
			configMap.Clear();
		} else {
			configMap = new Dictionary<int, TileConfig>();
		}

		for (var i = 0; i < configs.Length; i++) {
			var elem = configs[i];
			configMap.Add(elem.id, elem);
		}
	}

	public void OnBeforeSerialize() { }

	public TileConfig GetConfigById(int id) {
		TileConfig result = null;
		configMap.TryGetValue(id, out result);
		return result;
	}

	public Sprite GetSpriteById(int id) {
		var config = GetConfigById(id);
		return config.sprite;
	}

	public TileConfig[] GetConfigs() {
		return configs;
	}
}