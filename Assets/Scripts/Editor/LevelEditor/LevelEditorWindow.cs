using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

public class LevelEditorWindow : EditorWindow, IHasCustomMenu {
	const string MENU_ITEM_PATH = "Tools/Level Editor %#l",
		CHANGE_LEVEL_EDITOR_CODE = "Change Level Editor",
		LEVEL_EDITOR_TITLE = "Level Editor";
#pragma warning disable 649,414
	[SerializeField]
	private LevelEditor editor;
#pragma warning restore 649,414

	void Awake() {
		titleContent = new GUIContent(LEVEL_EDITOR_TITLE);
		editor = LevelEditor.Get();
	}

	void OnEnable() {
		editor.OnEnable();
	}

	void OnGUI() {
		editor.OnGUI(position);
	}

	void OnDestroy() {
		editor.OnDestroy();
	}

	private void Edit(LevelAsset level) {
		editor.Edit(level);
	}

	[MenuItem(MENU_ITEM_PATH)]
	public static LevelEditorWindow GetLevelEditor() {
		var instance = GetWindow<LevelEditorWindow>();
		return instance;
	}

	[OnOpenAsset]
	static bool TryToOpenLevelAsset(int instanceID, int line) {
		var asset = EditorUtility.InstanceIDToObject(instanceID);
		if (asset is LevelAsset) {
			GetLevelEditor().Edit(asset as LevelAsset);
			return true;
		}
		return false;
	}

	public void AddItemsToMenu(GenericMenu menu) {
		menu.AddItem(CHANGE_LEVEL_EDITOR_CODE, OpenLevelEditorCode);
		menu.AddSeparator(string.Empty);
		editor.AddItemsToWindowMenu(menu);
	}

	private void OpenLevelEditorCode() {
		AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(this));
	}
}