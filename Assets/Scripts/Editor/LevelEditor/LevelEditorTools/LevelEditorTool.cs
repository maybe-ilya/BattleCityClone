using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface LevelEditorMouseUpHandler {
	void OnMouseUp(Vector2Int point, Event e);
}

public interface LevelEditorMouseDownHandler {
	void OnMouseDown(Vector2Int point, Event e);
}

public interface LevelEditorMouseDragHandler {
	void OnMouseDrag(Vector2Int point, Event e);
}

public interface LevelEditorOnGUIHandler {
	void OnGUI();
}

public class LevelEditorTool {
#pragma warning disable 649,414
	protected LevelEditor editor;
	protected LevelAsset editLevel;
	protected string name;
#pragma warning restore 649, 414
	public LevelEditorTool(LevelEditor editor) {
		this.editor = editor;
		this.name = "tool";
	}

	public string GetName() {
		return name;
	}

	public void SetEditLevel(LevelAsset editLevel) {
		this.editLevel = editLevel;
	}
}