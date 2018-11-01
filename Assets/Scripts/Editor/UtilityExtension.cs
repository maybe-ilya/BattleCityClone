using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public static class SerializedPropertyExtension {
	public static Type GetPropertyType(this SerializedProperty property) {
		var searchType = property.serializedObject.targetObject.GetType();
		var pathSplit = property.propertyPath.Split('.');
		var searchOptions = BindingFlags.GetField |
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance;

		for (var i = 0; i < pathSplit.Length; i++) {
			var currentProperty = pathSplit[i];
			FieldInfo field = null;

			var iterationFinished = false;
			while (!iterationFinished) {
				field = searchType.GetField(currentProperty, searchOptions);
				if (field != null) {
					iterationFinished = true;
				} else {
					searchType = searchType.BaseType;
				}

				if (searchType == null) {
					iterationFinished = true;
				}
			}

			if (field == null) { break; }
			searchType = field.FieldType;
		}

		Type result;
		if (searchType == null) {
			result = null;
		} else if (searchType.IsArray) {
			result = searchType.GetElementType();
		} else if (searchType.IsGenericType) {
			result = searchType.GetGenericArguments() [0];
		} else {
			result = searchType;
		}
		return result;
	}
}

public static class GenericMenuExtension {
	public static void AddItem(this GenericMenu menu, string label, Action action) {
		menu.AddItem(new GUIContent(label), false, new GenericMenu.MenuFunction(action));
	}
}

public static class AssetFinder {
	public static T FindAsset<T>(string name = null) where T : UnityObject {
		return FindAsset(typeof(T), name) as T;
	}

	private static UnityObject FindAsset(Type type, string name) {
		var filter = string.Format("t:{0} {1}", type.Name, name);
		var guids = AssetDatabase.FindAssets(filter);

		if (guids.Length != 1) {
			return null;
		}

		var path = AssetDatabase.GUIDToAssetPath(guids[0]);
		return AssetDatabase.LoadAssetAtPath(path, type);
	}
}

public static class EditorGUIExtensions {
	public static GUISkin GetCurrentSkin() {
		var skinType = EditorGUIUtility.isProSkin ?
			EditorSkin.Scene :
			EditorSkin.Inspector;
		return EditorGUIUtility.GetBuiltinSkin(skinType);
	}
}