using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TileTool : LevelEditorTool, LevelEditorOnGUIHandler, LevelEditorMouseDownHandler, LevelEditorMouseDragHandler {
#pragma warning disable 649,414
	private TileSettings tileSettings;

	// private int selectedTileIndex;
	private int selectedTileId = 1;

	private TileConfig SelectedTile {
		get {
			if (selectedTileId > 0) {
				return tileSettings.GetConfigById(selectedTileId);
			}
			return null;
		}
	}
#pragma warning restore 649,414

	public TileTool(LevelEditor editor) : base(editor) {
		name = "Tile";
		tileSettings = AssetFinder.FindAsset<TileSettings>();
	}

	public void OnGUI() {
		var configs = tileSettings.GetConfigs();

		var rect = GUILayoutUtility.GetRect(0, int.MaxValue, 0, int.MaxValue, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

		var size = 32.0f;
		for (var i = 0; i < configs.Length; i++) {
			var area = rect;
			area.height = size;
			area.y += i * size;

			var sprite = area;
			sprite.width = size;

			var button = area;
			button.width -= size;
			button.x += size;

			var elem = configs[i];
			editor.DrawSprite(sprite, elem.sprite);

			if (GUI.Button(button, elem.Name)) {
				selectedTileId = configs[i].id;
			}
		}
	}

	public void OnMouseDown(Vector2Int point, Event e) {
		ProcessMouseEvent(point, e);
	}

	public void OnMouseDrag(Vector2Int point, Event e) {
		ProcessMouseEvent(point, e);
	}

	private void ProcessMouseEvent(Vector2Int point, Event e) {
		var level = editor.EditLevel;
		var tile = SelectedTile;
		if (level.Contains(point) && tile != null) {
			var layer = tile.layer;

			level[layer][point.x, point.y] = e.shift ? 0 : tile.id;

			editor.ModifyLevel();
		}
		e.Use();
	}
}