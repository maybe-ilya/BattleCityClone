using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeReference))]
public class TypeReferencePropertyDrawer : PropertyDrawer {
    const string BUTTON_TEXT = "Find";
    readonly GUIContent BUTTON_CONTENT = new GUIContent(BUTTON_TEXT);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var style = EditorStyles.label;
        var buttonSize = style.CalcSize(BUTTON_CONTENT);

        position = EditorGUI.PrefixLabel(position, label);

        var labelRect = position;
        var buttonRect = position;
        buttonRect.width = buttonSize.x;
        labelRect.width -= buttonRect.width;
        buttonRect.x += labelRect.width;

        property.Next(true);
        EditorGUI.LabelField(labelRect, property.stringValue);
        if (GUI.Button(buttonRect, BUTTON_CONTENT)) {
            OnButtonClick(position, property);
        }
    }

    private void OnButtonClick(Rect rect, SerializedProperty property) {
        PopupWindow.Show(rect, new TypeReferencePopup(property, fieldInfo, rect));
    }
}

public class TypeReferencePopup : PopupWindowContent {
    private SerializedProperty typeRefProp;
    private FieldInfo info;
    private Rect rect;

    private SearchField searchField;

    private Type[] possibleTypes;
    private List<Type> currentViewTypes;

    const int MAX_LINES = 20;
    private string searchString;
    private Vector2 scroll;

    private bool isClosing;

    public TypeReferencePopup(SerializedProperty prop, FieldInfo info, Rect rect) {
        this.typeRefProp = prop;
        this.info = info;
        this.rect = rect;

        this.searchField = new SearchField();
        searchField.autoSetFocusOnFindCommand = true;
    }

    public override void OnOpen() {
        CachePossibleTypes();
    }

    private void CachePossibleTypes() {
        var searchType = typeof(TypeReferenceBaseAttribute);
        var attrs = info.GetCustomAttributes(searchType, true);

        Type baseType = null;
        if (attrs.Length > 0) {
            baseType = (attrs[0] as TypeReferenceBaseAttribute).BaseType;
        } else {
            baseType = typeof(object);
        }

        var isInterface = baseType.IsInterface;

        possibleTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(type => !type.IsAbstract && isInterface? type.GetInterfaces().Contains(baseType) : type.IsSubclassOf(baseType))
            .ToArray();

        currentViewTypes = new List<Type>();
        currentViewTypes.AddRange(possibleTypes);
    }

    public override void OnClose() {
        Clear();
    }

    private void Clear() {
        typeRefProp = null;
        info = null;
        searchField = null;
        currentViewTypes = null;
        possibleTypes = null;
    }

    public override Vector2 GetWindowSize() {
        return new Vector2() {
            x = rect.width,
                y = EditorGUIUtility.singleLineHeight * Mathf.Clamp(1, MAX_LINES, (!isClosing ? currentViewTypes.Count : 0) + 1)
        };
    }

    public override void OnGUI(Rect rect) {
        if (isClosing) { return; }
        var searchFieldRect = rect;
        searchFieldRect.height = EditorGUIUtility.singleLineHeight;

        var scrollViewRect = rect;
        scrollViewRect.height -= searchFieldRect.height;
        scrollViewRect.y += searchFieldRect.height;

        DrawSearchField(searchFieldRect);
        DrawScrollView(scrollViewRect);
    }

    private void DrawSearchField(Rect rect) {
        EditorGUI.BeginChangeCheck();
        searchString = searchField.OnGUI(rect, searchString);
        if (EditorGUI.EndChangeCheck()) {
            OnSearchChanged();
        }
    }

    private void OnSearchChanged() {
        currentViewTypes.Clear();
        if (!string.IsNullOrEmpty(searchString)) {
            currentViewTypes.AddRange(possibleTypes.Where(type => type.Name.Contains(searchString)));
        } else {
            currentViewTypes.AddRange(possibleTypes);
        }
    }

    private void DrawScrollView(Rect rect) {
        GUILayout.BeginArea(rect);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; !isClosing && i < currentViewTypes.Count; i++) {
            var elem = currentViewTypes[i];
            if (GUILayout.Button(elem.Name)) {
                TrySetType(elem);
                TryToClose();
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void TrySetType(Type newType) {
        typeRefProp.stringValue = newType.AssemblyQualifiedName;
        typeRefProp.serializedObject.ApplyModifiedProperties();
    }

    private void TryToClose() {
        isClosing = true;
        editorWindow.Close();
    }
}