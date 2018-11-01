using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedPair<K, T> {
	[SerializeField]
	public K key;
	[SerializeField]
	public T value;
}

[Serializable]
public class SerializedDictionary<P, K, T> : ISerializationCallbackReceiver where P : SerializedPair<K, T> {
#pragma warning disable 649, 414
	// This wouldn't work
	// [SerializeField]
	// private SerializedPair<K, T>[] oldPairs;
	[SerializeField]
	private P[] pairs = new P[1];

	[NonSerialized]
	private Dictionary<K, T> map;
#pragma warning restore 649, 414

	public void OnAfterDeserialize() {
		if (map != null) {
			map.Clear();
		} else {
			map = new Dictionary<K, T>();
		}

		for (var i = 0; i < pairs.Length; i++) {
			var pair = pairs[i];
			var key = pair.key;
			var value = pair.value;

			map.Add(key, value);
		}
	}

	public void OnBeforeSerialize() { }

	public T this [K key] {
		get {
			T result;
			map.TryGetValue(key, out result);
			return result;
		}
	}
}