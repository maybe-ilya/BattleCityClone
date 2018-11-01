using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

[Serializable]
public partial class LevelEditor {
	const string CREATE_LEVEL_TITLE = "Create Title",
		OEPN_LEVEL_TITLE = "Open Title",
		LEVEL_EXTENSION = "asset",
		ASSETS_FOLDER = "Assets";
	readonly string DEFAULT_DIRECTORY = string.Empty;
#pragma warning disable 649,414
	[SerializeField]
	private LevelAsset editLevel;

	[SerializeField]
	private LevelEditorSettings editorSettings;
	[SerializeField]
	private GameSettings gameSettings;
	[SerializeField]
	private TileSettings tileSettings;

	private LevelEditorTool[] toolset;
#pragma warning restore 649,414

	public static LevelEditor Get() {
		var newEditor = new LevelEditor();
		newEditor.Initalize();
		return newEditor;
	}

	private void Initalize() {
		InvokeMethodsWithAttribute(typeof(LevelEditorInitializeAttribute));
	}

	public void OnEnable() {
		InvokeMethodsWithAttribute(typeof(LevelEditorOnEnableAttribute));
	}

	public void OnDestroy() {
		if (IsEditing) {
			SaveLevel();
		}
	}

	public void Edit(LevelAsset newLevel) {
		if (CanEdit(newLevel)) {
			if (IsEditing) {
				SaveLevel();
			}
			StartEdit(newLevel);
		}
	}

	public bool IsEditing { get { return editLevel != null; } }
	public LevelAsset EditLevel { get { return editLevel; } }

	public bool CanEdit(LevelAsset asset) {
		return asset != null;
	}

	public void StartEdit(LevelAsset newAsset) {
		editLevel = newAsset;
	}

	public void ModifyLevel() {
		EditorUtility.SetDirty(editLevel);
	}

	private void SaveLevel() {
		ModifyLevel();
		AssetDatabase.SaveAssets();
	}

	[LevelEditorInitialize]
	private void GatherDependencies() {
		editorSettings = AssetFinder.FindAsset<LevelEditorSettings>();
		gameSettings = AssetFinder.FindAsset<GameSettings>();
		tileSettings = AssetFinder.FindAsset<TileSettings>();
	}

	public void OnGUI(Rect windowRect) {
		MarkWindow(windowRect);
		HandleToolbar();
		BeginBaseArea();
		HandleField();
		HandleToolset();
		EndBaseArea();
	}

	private void AddFileMenuFunctions(GenericMenu menu) {
		menu.AddItem("Create", TryToCreateLevel);
		menu.AddItem("Open", TryToOpenLevel);
		if (IsEditing) {
			menu.AddItem("Save", SaveLevel);
		}
		menu.AddItem("Close", TryToClose);
	}

	private void TryToCreateLevel() {
		var path = EditorUtility.SaveFilePanel(CREATE_LEVEL_TITLE, DEFAULT_DIRECTORY, "New Level", LEVEL_EXTENSION);
		if (IsValidPath(path)) {
			path = GetRelativePath(path);

			var newLevel = ScriptableObject.CreateInstance<LevelAsset>();
			AssetDatabase.CreateAsset(newLevel, path);

			for (var i = 0; i < editorSettings.DefaultLayerCount; i++) {
				var newLayer = ScriptableObject.CreateInstance<LevelLayer>();
				newLayer.name = string.Format("Layer {0}", i + 1);
				newLevel.AddLayer(newLayer);
				AssetDatabase.AddObjectToAsset(newLayer, path);
			}
			newLevel.SetFieldSize(editorSettings.DefaultFieldSize);

			EditorUtility.SetDirty(newLevel);
			AssetDatabase.SaveAssets();
			AssetDatabase.OpenAsset(newLevel);
		}
	}

	private bool IsValidPath(string path) {
		return !string.IsNullOrEmpty(path) && path.Contains(Application.dataPath);
	}

	private string GetRelativePath(string path) {
		var basePath = Application.dataPath.Replace(ASSETS_FOLDER, string.Empty);
		path = path.Replace(basePath, string.Empty);
		return path;
	}

	private void TryToOpenLevel() {
		var path = EditorUtility.OpenFilePanel(CREATE_LEVEL_TITLE, DEFAULT_DIRECTORY, LEVEL_EXTENSION);
		if (IsValidPath(path)) {
			path = GetRelativePath(path);
			var asset = AssetDatabase.LoadAssetAtPath<LevelAsset>(path);
			if (asset != null) {
				AssetDatabase.OpenAsset(asset);
			}
		}
	}

	private void TryToClose() {
		EditorWindow.focusedWindow.Close();
	}

	public void AddItemsToWindowMenu(GenericMenu menu) {
		menu.AddItem("View Editor Settings", PingToEditorSettings);
		if (IsEditing) {
			menu.AddItem("Find Edit Level", PingEditLevel);
		}
	}

	private void PingToEditorSettings() {
		PingAndSelect(editorSettings);
	}

	private void PingEditLevel() {
		PingAndSelect(EditLevel);
	}

	private void PingAndSelect(UnityObject obj) {
		if (obj == null) { return; }
		EditorGUIUtility.PingObject(obj);
		Selection.activeObject = obj;
	}

	[LevelEditorOnEnable]
	private void GetToolset() {
		var references = editorSettings.GetToolsetEditorReferences();
		var count = references.Length;
		toolset = new LevelEditorTool[count];
		for (var i = 0; i < count; i++) {
			toolset[i] = references[i].GetObject<LevelEditorTool>(this);
		}
	}
}