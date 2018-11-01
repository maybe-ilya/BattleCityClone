using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class SerializedPropertyTable : TreeView {
    private static float INDEX_COLUMN_WIDTH = 35;
    private const int MIN_ROWS = 10;

    private SerializedProperty property;
    private string[] childNames;
    private GUIContent emptyLabel;
    private int minRowCount;
    private bool hasVisibleChildren;
    private bool checkSerializedObjectState;
    private int previousArraySize;
    private bool dragNDropComplete;
    private int previousExpantions;

    private Action onContextClicked = () => { };

    public event Action OnContextClicked {
        add { onContextClicked += value; }
        remove { onContextClicked -= value; }
    }

    private SerializedObject serializedObject { get { return property.serializedObject; } }

#region Initialization
    public SerializedPropertyTable (int minRows = 0) : base (new TreeViewState ()) {
        this.minRowCount = minRows > 0 ? minRows : MIN_ROWS;
        SetCheckSerializedObjectState (false);
    }

    public SerializedPropertyTable (SerializedProperty property, int minRows = 0) : this (minRows) {
        if (property == null) { throw new Exception ("NO"); }
        SetProperty (property);
    }

    private void InitTable () {
        state.Clear ();
        showAlternatingRowBackgrounds = true;
        var type = property.GetPropertyType ();
        hasVisibleChildren = HasVisibleChildren (type);
        emptyLabel = new GUIContent (string.Empty);
        if (hasVisibleChildren) {
            var fields = GetVisibleChildrenFields (property.GetPropertyType ());
            childNames = fields.Select (x => x.Name).ToArray ();
            multiColumnHeader = new MultiColumnHeader (GetHeaderState (fields));
        } else {
            multiColumnHeader = new MultiColumnHeader (GetHeaderState ());
        }
        multiColumnHeader.ResizeToFit ();
        previousArraySize = property.arraySize;
        previousExpantions = GetPropertyExpantions ();
        Reload ();
    }

    private static GUIStyle HeaderStyle {
        get {
            var skin = EditorGUIUtility.GetBuiltinSkin (EditorSkin.Inspector);
            var label = skin.FindStyle ("label");
            var result = new GUIStyle (label);
            result.padding = new RectOffset (4, 4, 0, 0);
            return result;
        }
    }

    private static MultiColumnHeaderState GetHeaderState (FieldInfo[] fields) {
        List<MultiColumnHeaderState.Column> columns = new List<MultiColumnHeaderState.Column> ();
        columns.Add (new MultiColumnHeaderState.Column () {
            headerContent = new GUIContent ("#"),
                minWidth = INDEX_COLUMN_WIDTH,
                width = INDEX_COLUMN_WIDTH,
                maxWidth = INDEX_COLUMN_WIDTH,
                autoResize = true,
                allowToggleVisibility = false,
                canSort = false,
        });

        var style = HeaderStyle;

        for (int i = 0; i < fields.Length; i++) {
            var elem = fields[i];
            GUIContent content = new GUIContent (GetNiceText (elem.Name));

            var tooltipAttr = elem.GetCustomAttributes (typeof (TooltipAttribute), true);
            if (tooltipAttr.Length > 0) {
                content.tooltip = (tooltipAttr[0] as TooltipAttribute).tooltip;
            }

            float width = style.CalcSize (content).x;

            columns.Add (new MultiColumnHeaderState.Column () {
                headerContent = content,
                    minWidth = width,
                    width = width,
                    autoResize = true,
                    allowToggleVisibility = true,
                    canSort = false,
            });
        }

        return new MultiColumnHeaderState (columns.ToArray ());
    }

    private static MultiColumnHeaderState GetHeaderState () {
        List<MultiColumnHeaderState.Column> columns = new List<MultiColumnHeaderState.Column> ();
        columns.Add (new MultiColumnHeaderState.Column () {
            headerContent = new GUIContent ("#"),
                minWidth = INDEX_COLUMN_WIDTH,
                width = INDEX_COLUMN_WIDTH,
                maxWidth = INDEX_COLUMN_WIDTH,
                autoResize = true,
                allowToggleVisibility = false,
                canSort = false,
        });
        columns.Add (new MultiColumnHeaderState.Column () {
            headerContent = new GUIContent ("Value"),
                autoResize = true,
                allowToggleVisibility = false,
                canSort = false,
        });
        return new MultiColumnHeaderState (columns.ToArray ());
    }

    private static string GetNiceText (string text) {
        return ObjectNames.NicifyVariableName (text);
    }

    private static Type GetPropertyType (SerializedProperty property) {
        var searchType = property.serializedObject.targetObject.GetType ();
        var pathSplit = property.propertyPath.Split ('.');
        var searchOptions = BindingFlags.GetField |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        for (int i = 0; i < pathSplit.Length; i++) {
            var field = searchType.GetField (pathSplit[i], searchOptions);
            searchType = field.FieldType;
        }

        if (searchType.IsArray) {
            return searchType.GetElementType ();
        } else {
            return searchType.GetGenericArguments () [0];
        }
    }

    private FieldInfo[] GetVisibleChildrenFields (Type type) {
        var mask = BindingFlags.GetField |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        var serializeFieldAttr = typeof (SerializeField);
        var hideInInspectorAttr = typeof (HideInInspector);

        var fields = type
            .GetFields (mask)
            .Where (field =>
                (field.IsPublic ||
                    Attribute.IsDefined (field, serializeFieldAttr)) &&
                !Attribute.IsDefined (field, hideInInspectorAttr))
            .ToArray ();

        return fields;
    }

    private bool HasVisibleChildren (Type type) {
        var fields = GetVisibleChildrenFields (type);
        return fields != null && fields.Length > 0;
    }
#endregion

    public void SetProperty (SerializedProperty newProperty) {
        this.property = newProperty;
        InitTable ();
    }

    protected override TreeViewItem BuildRoot () {
        return new TreeViewItem (-1, -1);
    }

    protected override IList<TreeViewItem> BuildRows (TreeViewItem root) {
        var newResult = new List<TreeViewItem> ();

        var size = property.arraySize;
        var subChildCount = hasVisibleChildren ? childNames.Length : 0;

        for (int i = 0; i < size; i++) {
            var baseProp = property.GetArrayElementAtIndex (i);
            var childProps = new SerializedProperty[subChildCount];

            for (int x = 0; x < subChildCount; x++) {
                childProps[x] = baseProp.FindPropertyRelative (childNames[x]);
            }

            var newElem = new SerializedPropertyTableItem (baseProp, childProps, i);
            newResult.Add (newElem);
        }

        root.children = newResult;
        return newResult;
    }

    protected override float GetCustomRowHeight (int row, TreeViewItem item) {
        var serItem = item as SerializedPropertyTableItem;

        float height = 0;

        if (hasVisibleChildren) {
            for (int i = 0; i < serItem.ChildCount; i++) {
                var prop = serItem.Child[i];
                var temp = EditorGUI.GetPropertyHeight (prop, emptyLabel, prop.isExpanded);
                if (temp > height) {
                    height = temp;
                }
            }
        } else {
            var prop = serItem.Base;
            height = EditorGUI.GetPropertyHeight (prop, emptyLabel, prop.isExpanded);
        }

        return height;
    }

    protected override void RowGUI (RowGUIArgs args) {
        int index = args.row;

        var item = args.item as SerializedPropertyTableItem;

        int size = args.GetNumVisibleColumns ();
        for (int i = 0; i < size; i++) {
            Rect rect = args.GetCellRect (i);
            int x = args.GetColumn (i);

            if (x == 0) {
                EditorGUI.LabelField (rect, (index + 1).ToString ());
            } else {
                if (hasVisibleChildren) {
                    DrawProperty (rect, item.Child[x - 1]);
                } else {
                    DrawProperty (rect, item.Base);
                }
            }
        }
    }

    private void DrawIndexedProperty (Rect rect, SerializedProperty property, int index, bool isSelected = false) {
        var arrayElement = property.FindPropertyRelative (childNames[index]);
        DrawProperty (rect, arrayElement);
    }

    private void DrawProperty (Rect rect, SerializedProperty property) {
        bool includeChildren = property.hasVisibleChildren, isExpanded = property.isExpanded;
        var label = includeChildren ? new GUIContent (property.displayName) : emptyLabel;

        if (includeChildren && isExpanded) {
            EditorGUIUtility.labelWidth = rect.width / 2;
        }

        EditorGUI.PropertyField (rect, property, label, property.isExpanded);
        EditorGUIUtility.labelWidth = 0;
    }

    private float GetMinimalHeight () {
        int size = Mathf.Clamp (property.arraySize, 1, minRowCount);
        return multiColumnHeader.height + size * rowHeight;
    }

    private bool NeedReload () {
        var currentExpantions = GetPropertyExpantions ();
        var currentArraySize = property.arraySize;
        var needReload = dragNDropComplete ||
            previousArraySize != currentArraySize ||
            currentExpantions != previousExpantions;
        previousArraySize = currentArraySize;
        dragNDropComplete = false;
        previousExpantions = currentExpantions;
        return needReload;
    }

    private int GetPropertyExpantions () {
        int result = 0;

        var copy = property.Copy ();
        while (copy.Next (true)) {
            result += copy.isExpanded ? 1 : 0;
        }

        return result;
    }

    protected override void BeforeRowsGUI () {
        base.BeforeRowsGUI ();
        if (property == null) { throw new NullReferenceException ("Property not initialized"); }
        CheckReload ();
    }

    protected override void AfterRowsGUI () {
        base.AfterRowsGUI ();
        ApplyChanges ();
        CheckUpdates ();
    }

    private void CheckReload () {
        if (NeedReload ()) { Reload (); }
    }

    private void CheckUpdates () {
        if (checkSerializedObjectState) {
            serializedObject.Update ();
        }
    }

    private void ApplyChanges () {
        if (checkSerializedObjectState) {
            serializedObject.ApplyModifiedProperties ();
        }
    }

    private Rect GetLayoutRect (bool expandHeight) {
        return GUILayoutUtility.GetRect (0, 0,
            GetMinimalHeight (),
            totalHeight,
            GUILayout.ExpandWidth (true),
            GUILayout.ExpandHeight (expandHeight));
    }

    public void OnGUILayout (bool expandHeight = false) {
        OnGUI (GetLayoutRect (expandHeight));
    }

    protected override void CommandEventHandling () {
        base.CommandEventHandling ();
        var e = Event.current;

        if (e.type == EventType.ValidateCommand) {
            HandleValidateCommandEvents (e);
        }
    }

    private void HandleValidateCommandEvents (Event e) {
        switch (e.commandName) {
            case "SoftDelete":
            case "Delete":
                DeleteElements (GetSelection ());
                UseEvent (e);
                break;

            case "Duplicate":
                DuplicateElements (GetSelection ());
                UseEvent (e);
                break;

            case "Copy":
                break;

            case "Cut":
                break;

            case "Paste":
                break;
        }
    }

    private void UseEvent (Event e) {
        e.Use ();
        GUIUtility.ExitGUI ();
    }

    public void RemoveElements () {
        DeleteElements (GetSelection ());
    }

    private void DeleteElements (IList<int> selection) {
        int count = selection != null ? selection.Count : 0;

        if (count <= 0) {
            if (property.arraySize > 0) {
                property.arraySize--;
            }
        } else {
            selection = selection.OrderBy (x => x).ToList ();
            for (int i = selection.Count - 1; i >= 0; i--) {
                var index = selection[i];
                property.DeleteArrayElementAtIndex (index);
            }
        }

        ClearSelection ();
    }

    public void AddNewElemens () {
        DuplicateElements (GetSelection ());
    }

    private void DuplicateElements (IList<int> selection) {
        int count = selection != null ? selection.Count : 0;

        if (count <= 0) {
            property.arraySize++;
            SetSelection (new int[] { property.arraySize - 1 });
        } else {
            selection = selection.OrderBy (x => x).ToList ();
            List<int> newSelection = new List<int> ();
            for (int i = selection.Count - 1; i >= 0; i--) {
                var index = selection[i];
                newSelection.Add (i + index + 1);
                property.InsertArrayElementAtIndex (index);
            }

            SetSelection (newSelection);
        }
    }

    private void ClearSelection () {
        SetSelection (new int[0]);
    }

#region Drag 'n' Drop
    private List<int> movingRows = new List<int> ();

    protected override bool CanStartDrag (CanStartDragArgs args) {
        var size = property.arraySize;
        return size > 1;
    }

    protected override void SetupDragAndDrop (SetupDragAndDropArgs args) {
        DragAndDrop.PrepareStartDrag ();
        movingRows.Clear ();
        movingRows.AddRange (args.draggedItemIDs);
        movingRows.Sort ();
        DragAndDrop.objectReferences = new UnityEngine.Object[] { property.serializedObject.targetObject };
        DragAndDrop.StartDrag (property.propertyPath);
    }

    protected override DragAndDropVisualMode HandleDragAndDrop (DragAndDropArgs args) {
        if (args.performDrop) {
            int target = -1;

            switch (args.dragAndDropPosition) {
                case DragAndDropPosition.BetweenItems:
                    target = args.insertAtIndex;
                    break;

                case DragAndDropPosition.UponItem:
                    target = args.parentItem.id;
                    break;
            }
            int count = movingRows.Count;

            if (target > 0 && count > 0) {
                for (int i = 0; i < count; i++) {
                    int index = movingRows[i];
                    property.MoveArrayElement (index, target);
                }

                ClearSelection ();
                movingRows.Clear ();
                DragAndDrop.AcceptDrag ();
                dragNDropComplete = true;
            }
        }
        return DragAndDropVisualMode.Move;
    }
#endregion

    protected override void ContextClicked () {
        base.ContextClicked ();
        if (onContextClicked != null) {
            onContextClicked ();
        }
    }

    public void SetCheckSerializedObjectState (bool newValue) {
        checkSerializedObjectState = newValue;
    }
}

public static class TreeViewStateExtension {
    public static void Clear (this TreeViewState state) {
        state.searchString = string.Empty;
        state.selectedIDs.Clear ();
        state.expandedIDs.Clear ();
        state.lastClickedID = -1;
        state.scrollPos = Vector2.zero;
    }
}

public class SerializedPropertyTableItem : TreeViewItem {
#pragma warning disable 649
    private SerializedProperty baseProperty;
    private SerializedProperty[] childProperties;
#pragma warning restore 649

    public SerializedProperty Base { get { return baseProperty; } }
    public SerializedProperty[] Child { get { return childProperties; } }
    public int ChildCount { get { return childProperties.Length; } }

    public SerializedPropertyTableItem (SerializedProperty baseProperty,
        SerializedProperty[] childProperties,
        int id, int depth = 0) : base (id, depth) {
        this.baseProperty = baseProperty;
        this.childProperties = childProperties;
    }
}