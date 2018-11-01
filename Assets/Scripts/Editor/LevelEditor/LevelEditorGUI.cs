using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public partial class LevelEditor {
#pragma warning disable 649,414
	[SerializeField]
	private GUIStyle toolbarStyle,
	toolbarButtonStyle,
	toolsetStyle,
	toolbarDropDown;

	[SerializeField]
	private Rect toolbarArea,
	baseArea,
	editArea,
	fieldArea,
	toolsetArea;

	[SerializeField]
	private float fieldRatio;

	[SerializeField]
	private Vector2 toolsetScroll;

	[SerializeField]
	private SearchField searchField;
	[SerializeField]
	private int selectedToolIndex;
	[SerializeField]
	private string searchFieldText = string.Empty;
	[SerializeField]
	private Rect fileDropDown;
#pragma warning disable 649,414

	private LevelEditorTool SelectedTool {
		get {
			if (toolset != null) {
				return toolset[selectedToolIndex];
			}
			return null;
		}
	}

	[LevelEditorInitialize]
	private void FindStyles() {
		var skin = EditorGUIExtensions.GetCurrentSkin();
		toolbarStyle = skin.GetStyle("toolbar");
		toolbarButtonStyle = skin.GetStyle("toolbarbutton");
		toolsetStyle = skin.GetStyle("Label");
		toolbarDropDown = skin.GetStyle("toolbarDropDown");
	}

	[LevelEditorInitialize]
	private void GetRatio() {
		fieldRatio = editorSettings.FieldRatio;
	}

	[LevelEditorOnEnable]
	private void GetSearchField() {
		searchField = new SearchField();
		searchField.autoSetFocusOnFindCommand = true;
	}

	private void MarkWindow(Rect windowRect) {
		if (Event.current.type != EventType.Layout) { return; }
		var zeroPoint = Vector2.zero;
		var baseSize = windowRect.size;
		var toolbarHeight = toolbarStyle.fixedHeight;

		toolbarArea = new Rect(zeroPoint, baseSize);
		toolbarArea.height = toolbarHeight;

		baseArea = new Rect(zeroPoint, baseSize);
		baseArea.height -= toolbarHeight;
		baseArea.y += toolbarHeight;

		editArea = new Rect(zeroPoint, baseSize);
		editArea.width = Mathf.Round(editArea.width * fieldRatio);

		if (IsEditing) {
			var size = EditLevel.FieldSize;
			var tileSize = editorSettings.TileSize;

			fieldArea.position = Vector2.zero;
			fieldArea.size = Vector2.Scale(tileSize, size);
			var diff = editArea.size - fieldArea.size;
			fieldArea.x += (diff.x / 2);
			fieldArea.y += (diff.y / 2);

		} else {
			fieldArea = editArea;
		}

		toolsetArea = new Rect(zeroPoint, baseSize);
		toolsetArea.width -= editArea.width;
		toolsetArea.x += editArea.width;
	}

	private void HandleToolbar() {
		EditorGUILayout.BeginHorizontal(toolbarStyle, GUILayout.ExpandWidth(true));
		DrawFileButton();
		DrawLayerDropdown();
		EditorGUILayout.Space();
		DrawEditLevelName();
		GUILayout.FlexibleSpace();
		DrawSearchField();
		EditorGUILayout.EndHorizontal();
	}

	private void DrawFileButton() {
		if (GUILayout.Button("File", toolbarButtonStyle, GUILayout.ExpandWidth(false))) {
			GenericMenu menu = new GenericMenu();
			AddFileMenuFunctions(menu);
			menu.DropDown(fileDropDown);
		}
		if (Event.current.type == EventType.Repaint) {
			fileDropDown = GUILayoutUtility.GetLastRect();
		}
	}

	private void DrawEditLevelName() {
		if (IsEditing) {
			var name = EditLevel.name;
			EditorGUILayout.LabelField(name, GUILayout.ExpandWidth(false));
		}
	}

	private void HandleField() {
		GUI.BeginClip(editArea);
		DrawBackground();

		GUI.BeginClip(fieldArea);

		if (IsEditing) {
			var id = GUIUtility.GetControlID(FocusType.Passive);

			var point = Vector2Int.zero;
			var e = Event.current;

			if (e.isMouse) {
				point = CalculatePoint(e);
			}

			switch (e.GetTypeForControl(id)) {
				case EventType.MouseDown:
					if (GUIUtility.hotControl == 0) {
						HandleMouseDown(point, e);
						GUIUtility.hotControl = id;
					}
					break;

				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id) {
						HandleMouseDrag(point, e);
					}
					break;

				case EventType.MouseUp:
					if (GUIUtility.hotControl == id) {
						HandleMouseUp(point, e);
						GUIUtility.hotControl = 0;
					}
					break;

				case EventType.Repaint:
					DrawField();
					break;
			}
		}

		GUI.EndClip();

		GUI.EndClip();
	}

	private void DrawBackground() {
		DrawTintedTexture(new Rect(Vector2.zero, editArea.size), Color.black);
	}

	private void BeginBaseArea() {
		GUI.BeginClip(baseArea);
	}

	private void EndBaseArea() {
		GUI.EndClip();
	}

	private void HandleToolset() {
		GUILayout.BeginArea(toolsetArea, toolsetStyle);
		toolsetScroll = EditorGUILayout.BeginScrollView(toolsetScroll);

		if (toolset != null) {
			selectedToolIndex = EditorGUILayout.Popup("Current Tool", selectedToolIndex, toolset.Select(x => x.GetName()).ToArray());

			if (SelectedTool is LevelEditorOnGUIHandler) {
				(SelectedTool as LevelEditorOnGUIHandler).OnGUI();
			}
		}
		EditorGUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private Vector2Int CalculatePoint(Event e) {
		var pos = e.mousePosition;
		var tileSize = editorSettings.TileSize;
		var result = new Vector2Int();

		result.x = Mathf.FloorToInt(pos.y / tileSize.x);
		result.y = Mathf.FloorToInt(pos.x / tileSize.y);

		return result;
	}

	private void HandleMouseUp(Vector2Int point, Event e) {
		if (SelectedTool != null && SelectedTool is LevelEditorMouseUpHandler) {
			(SelectedTool as LevelEditorMouseUpHandler).OnMouseUp(point, e);
		}
	}

	private void HandleMouseDrag(Vector2Int point, Event e) {
		if (SelectedTool != null && SelectedTool is LevelEditorMouseDragHandler) {
			(SelectedTool as LevelEditorMouseDragHandler).OnMouseDrag(point, e);
		}
	}

	private void HandleMouseDown(Vector2Int point, Event e) {
		if (SelectedTool != null && SelectedTool is LevelEditorMouseDownHandler) {
			(SelectedTool as LevelEditorMouseDownHandler).OnMouseDown(point, e);
		}
	}

	private void DrawSearchField() {
		searchFieldText = searchField.OnToolbarGUI(searchFieldText, GUILayout.ExpandWidth(true));
		if (searchField.HasFocus()) {
			var e = Event.current;
			switch (e.type) {
				case EventType.Used:
					if (e.keyCode == KeyCode.Return) {
						TryToFindLevel(searchFieldText);
						GUIUtility.keyboardControl = 0;
						searchFieldText = string.Empty;
					}
					break;
			}
		}
	}

	private bool TryToFindLevel(string searchLevelName) {
		var newLevel = AssetFinder.FindAsset<LevelAsset>(searchLevelName);
		if (newLevel != null) {
			AssetDatabase.OpenAsset(newLevel);
			return true;
		}
		return false;
	}
}