using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class TableEditor : UnityEditor.Editor {
    private string[] propertyNames;
    private int currentPropertyIndex;
    private NonTablePropsOrder otherPropsOrder;
    private bool useDefaultMargins;
    private SerializedPropertyTable table;
    private GUIContent[] toolbarContent;
    private GUIContent addButton, removeButton;

    private const string scriptPropertyName = "m_Script";

    protected SerializedProperty SelectedProperty {
        get { return serializedObject.FindProperty (propertyNames[currentPropertyIndex]); }
    }

    #region Initialization
    private void OnEnable () {
        GetPropertyNames ();
        GetOtherPropertiesOrder ();
        GetDefaultMarginUsage ();
        GetMiscellaneous ();
        table = new SerializedPropertyTable (SelectedProperty);
        table.SetCheckSerializedObjectState (false);
    }

    private void GetPropertyNames () {
        var thisType = GetType ();
        var attrType = typeof (TableEditorPropertiesAttribute);
        var attr = thisType.GetCustomAttributes (attrType, true);

        if (attr == null || attr.Length <= 0) {
            string message = string.Format ("Please use {0} class with {1} attribute",
                thisType.Name, attrType.Name);
            throw new Exception (message);
        }

        var propertyNames = (attr[0] as TableEditorPropertiesAttribute).GetProperties ();

        if (propertyNames == null || propertyNames.Length <= 0) {
            string message = string.Format ("Please check Property Names for {0} attribute of {1} class",
                attrType.Name, thisType.Name);
            throw new Exception (message);
        }

        this.propertyNames = propertyNames;
        currentPropertyIndex = 0;

        toolbarContent = propertyNames
            .Select (x => new GUIContent (ObjectNames.NicifyVariableName (x)))
            .ToArray ();
    }

    private void GetOtherPropertiesOrder () {
        Type thisType = GetType ();
        otherPropsOrder = NonTablePropsOrder.Never;
        var props = thisType.GetCustomAttributes (typeof (NonTablePropertiesOrderAttribute), true);

        if (props != null && props.Length > 0) {
            var attr = props[0] as NonTablePropertiesOrderAttribute;
            otherPropsOrder = attr.Order;
        }
    }

    private void GetDefaultMarginUsage () {
        Type thisType = GetType ();
        useDefaultMargins = false;

        var props = thisType.GetCustomAttributes (typeof (TableEditorDefaultMarginsAttribute), true);

        if (props != null && props.Length > 0) {
            var attr = props[0] as TableEditorDefaultMarginsAttribute;
            useDefaultMargins = attr.UseDefaultMargins;
        }
    }

    private void GetMiscellaneous () {
        addButton = EditorGUIUtility.IconContent ("Toolbar Plus");
        addButton.tooltip = "Add new element/duplicate selected elements";

        removeButton = EditorGUIUtility.IconContent ("Toolbar Minus");
        removeButton.tooltip = "Remove last/selected elements";
    }
    #endregion

    protected void DrawScriptReference () {
        EditorGUI.BeginDisabledGroup (true);
        EditorGUILayout.PropertyField (serializedObject.FindProperty (scriptPropertyName));
        EditorGUI.EndDisabledGroup ();
    }

    protected void DrawToolbar () {
        EditorGUILayout.BeginHorizontal ();
        DrawProperties ();
        DrawButtons ();
        EditorGUILayout.EndHorizontal ();
    }

    private GUILayoutOption ExpandWidth { get { return GUILayout.ExpandWidth (true); } }
    private GUILayoutOption DontExpantWidth { get { return GUILayout.ExpandWidth (false); } }

    private void DrawProperties () {
        if (propertyNames.Length > 1) {
            int temp = currentPropertyIndex;
            EditorGUI.BeginChangeCheck ();
            temp = EditorGUILayout.Popup (temp, toolbarContent, EditorStyles.toolbarPopup);
            if (EditorGUI.EndChangeCheck () && temp != currentPropertyIndex) {
                ChangeProperty (temp);
            }
        } else {
            GUILayout.Label (toolbarContent[0], EditorStyles.toolbarButton);
        }
    }

    private void DrawButtons () {
        var style = EditorStyles.toolbarButton;
        var option = DontExpantWidth;

        if (GUILayout.Button (addButton, style, option)) {
            table.AddNewElemens ();
        }
        if (GUILayout.Button (removeButton, style, option)) {
            table.RemoveElements ();
        }
    }

    private void ChangeProperty (int newProp) {
        currentPropertyIndex = newProp;
        var propertyName = propertyNames[currentPropertyIndex];
        var property = serializedObject.FindProperty (propertyName);
        table.SetProperty (property);
    }

    protected void DrawTable () {
        table.OnGUILayout ();
    }

    protected void DrawPropertyTable () {
        DrawToolbar ();
        DrawTable ();
    }

    public sealed override bool UseDefaultMargins () { return useDefaultMargins; }

    public sealed override void OnInspectorGUI () {
        DrawScriptReference ();
        BeforeTable ();
        DrawPropertyTable ();
        AfterTable ();
        serializedObject.ApplyModifiedProperties ();
        serializedObject.Update ();
    }

    private void BeforeTable () {
        if (otherPropsOrder == NonTablePropsOrder.BeforeTable) {
            DrawOtherProperties ();
        }
        BeforeTableDrawing ();
    }

    private void AfterTable () {
        if (otherPropsOrder == NonTablePropsOrder.AfterTable) {
            DrawOtherProperties ();
        }
        AfterTableDrawing ();
    }

    protected virtual void BeforeTableDrawing () { }

    protected virtual void AfterTableDrawing () { }

    protected void DrawOtherProperties () {
        var prop = serializedObject.GetIterator ();
        prop.Next (true);

        while (prop.NextVisible (false)) {
            var name = prop.name;
            if (scriptPropertyName.Equals (name) || propertyNames.Contains (name)) { continue; }
            EditorGUILayout.PropertyField (prop, true);
        }
    }
}

[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class TableEditorPropertiesAttribute : Attribute {
    private string[] propertyNames;

    public TableEditorPropertiesAttribute (params string[] names) {
        propertyNames = names;
    }

    public string[] GetProperties () { return propertyNames; }
}

[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class NonTablePropertiesOrderAttribute : Attribute {
    private NonTablePropsOrder order;

    public NonTablePropertiesOrderAttribute (NonTablePropsOrder order) {
        this.order = order;
    }

    public NonTablePropsOrder Order { get { return order; } }
}

public enum NonTablePropsOrder {
    Never = 0,
    BeforeTable = 1,
    AfterTable = 2,
}

[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class TableEditorDefaultMarginsAttribute : Attribute {
    private bool useDefaultMargins;

    public bool UseDefaultMargins { get { return useDefaultMargins; } }

    public TableEditorDefaultMarginsAttribute (bool useDefaultMargins = false) {
        this.useDefaultMargins = useDefaultMargins;
    }
}