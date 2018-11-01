using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance {
#pragma warning disable 659,414
	private static GameInstance instance;
	private GameSettings settings;
#pragma warning restore 659,414

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void CreateInstrance() {
		instance = new GameInstance();
	}

	private void Init() {
		FindGameSettings();
		ShowInitializationNotification();
	}

	private void FindGameSettings() {
		settings = Resources.Load<GameSettings>("GameSettings");
	}

	private void ShowInitializationNotification() {
		var message = string.Format("Game Instance initialized at {0}", DateTime.Now.ToLongTimeString());
		Debug.LogWarning(message);
	}
}