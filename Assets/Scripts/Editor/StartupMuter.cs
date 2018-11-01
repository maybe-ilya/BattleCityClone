using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class StartupMuteData {
	[SerializeField]
	public bool muteOnStartup = false;
}

public static class StartupMuter {
	const string MUTE_DATA_PATH = "STARTUP_MUTE_DATA",
		PREFERENCE_PATH = "StartUp Muter",
		MUTE_ON_STARTUP_FIELD_LABEL = "Mute on Startup";

	[InitializeOnLoadMethod]
	static void TryToMute() {
		if (EditorApplication.isPlaying) { return; }
		var data = GetData();
		EditorUtility.audioMasterMute = data.muteOnStartup;
	}

	[PreferenceItem(PREFERENCE_PATH)]
	static void DrawMuterPreferences() {
		var data = GetData();
		EditorGUI.BeginChangeCheck();
		data.muteOnStartup = EditorGUILayout.Toggle(MUTE_ON_STARTUP_FIELD_LABEL, data.muteOnStartup);
		if (EditorGUI.EndChangeCheck()) {
			SaveData(data);
		}
	}

	static StartupMuteData GetData() {
		var json = EditorPrefs.GetString(MUTE_DATA_PATH);
		StartupMuteData result = null;
		if (!string.IsNullOrEmpty(json)) {
			result = JsonUtility.FromJson<StartupMuteData>(json);
		} else {
			result = new StartupMuteData();
			SaveData(result);
		}
		return result;
	}

	static void SaveData(StartupMuteData data) {
		var json = JsonUtility.ToJson(data, true);
		EditorPrefs.SetString(MUTE_DATA_PATH, json);
	}
}