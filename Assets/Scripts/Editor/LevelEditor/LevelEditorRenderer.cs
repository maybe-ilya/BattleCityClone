using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class LevelEditor {
	readonly Rect DEFAULT_UV = new Rect(0, 0, 1, 1);
#pragma warning disable 649,414
	[SerializeField]
	private Material textureMaterial, linesMaterial;

	[SerializeField]
	private int layerMask = int.MaxValue;

	[SerializeField]
	private Rect layerDropDownRect;
#pragma warning restore 649,414

	[LevelEditorInitialize]
	private void GetMaterials() {
		textureMaterial = editorSettings.SpritesMaterial;
		linesMaterial = editorSettings.LinesMaterial;
	}

	public void DrawSprite(Rect spriteArea, Sprite sprite) {
		var text = sprite.texture;
		var rect = sprite.rect;
		rect.x /= text.width;
		rect.y /= text.height;
		rect.width /= text.width;
		rect.height /= text.height;

		Graphics.DrawTexture(spriteArea, text, rect, 0, 0, 0, 0, textureMaterial, 0);
	}

	public void DrawTintedTexture(Rect rect, Color tint) {
		DrawTexture(rect, Texture2D.whiteTexture, tint, textureMaterial);
	}

	public void DrawTexture(Rect rect, Texture texture, Color tint, Material material) {
		Graphics.DrawTexture(rect, texture, DEFAULT_UV, 0, 0, 0, 0, tint, material, 0);
	}

	private void DrawGrid() {
		var size = EditLevel.FieldSize;
		var tileSize = editorSettings.TileSize;
		var width = size.x * tileSize.x;
		var height = size.y * tileSize.y;

		linesMaterial.SetPass(0);
		GL.PushMatrix();
		GL.LoadPixelMatrix();
		GL.Begin(GL.LINES);
		GL.Color(Color.gray);

		for (var i = 0; i <= size.x; i++) {
			var origin = Vector3.zero + Vector3.right * tileSize.x * i;

			GL.Vertex(origin);
			origin += Vector3.up * height;
			GL.Vertex(origin);
		}

		for (var i = 0; i <= size.y; i++) {
			var origin = Vector3.zero + Vector3.up * tileSize.y * i;
			GL.Vertex(origin);

			origin += Vector3.right * width;
			GL.Vertex(origin);
		}

		GL.End();
		GL.PopMatrix();
	}

	private void DrawField() {
		DrawTiles();
		DrawGrid();
	}

	private void DrawTiles() {
		var level = EditLevel;
		var size = level.FieldSize;
		var tileSize = editorSettings.TileSize;

		for (var i = 0; i < level.LayerCount; i++) {
			if ((layerMask & (1 << i)) == 0) { continue; }

			for (var x = 0; x < size.x; x++) {
				for (var y = 0; y < size.y; y++) {
					var id = level[i][x, y];
					if (id > 0) {
						var rect = new Rect(Vector2.zero, tileSize);
						rect.x += y * tileSize.x;
						rect.y += x * tileSize.y;

						var tile = tileSettings.GetConfigById(id);

						DrawSprite(rect, tile.sprite);
					}
				}
			}
		}
	}

	private void DrawLayerDropdown() {
		if (!IsEditing) { return; }
		if (GUILayout.Button("Layers", toolbarDropDown)) {
			ShowLayerDropDown();
		}
		if (Event.current.type == EventType.Repaint) {
			layerDropDownRect = GUILayoutUtility.GetLastRect();
		}
	}

	private void ShowLayerDropDown() {
		var popup = new LayerPopup(layerMask, EditLevel.LayerCount, EditorWindow.focusedWindow);
		popup.OnMaskChange += ChangeMask;
		PopupWindow.Show(layerDropDownRect, popup);
	}

	private void ChangeMask(int index, bool value) {
		if (value) {
			layerMask |= 1 << index;
		} else {
			layerMask &= ~(1 << index);
		}
	}
}

public class LayerPopup : PopupWindowContent {
	public delegate void MaskChanged(int changeIndex, bool value);

	public MaskChanged OnMaskChange = null;

	private ReorderableList list;
	private bool[] maskArray;
	private EditorWindow window;

	public LayerPopup(int initialMask, int count, EditorWindow window) {
		this.window = window;
		initMask(initialMask, count);
		InitList();
	}

	private void initMask(int mask, int count) {
		maskArray = new bool[count];
		for (int i = 0; i < count; i++) {
			maskArray[i] = (1 << i & mask) != 0;
		}
	}

	private void InitList() {
		list = new ReorderableList(maskArray, typeof(bool), false, true, false, false);
		list.drawHeaderCallback += Header;
		list.drawElementCallback += Element;
	}

	public override Vector2 GetWindowSize() {
		var result = Vector2.zero;
		result.x = 180.0f;
		result.y = maskArray.Length * list.elementHeight + list.headerHeight;
		return result;
	}

	public override void OnGUI(Rect rect) {
		list.DoList(rect);
	}

	private void Header(Rect rect) {
		EditorGUI.LabelField(rect, string.Format("Layers Number: {0}", maskArray.Length));
	}

	private void Element(Rect rect, int index, bool isActive, bool isFocused) {
		EditorGUI.BeginChangeCheck();
		maskArray[index] = EditorGUI.Toggle(rect, string.Format("Layer {0}", index + 1), maskArray[index]);
		if (EditorGUI.EndChangeCheck() && OnMaskChange != null) {
			OnMaskChange(index, maskArray[index]);
			window.Repaint();
		}
	}
}