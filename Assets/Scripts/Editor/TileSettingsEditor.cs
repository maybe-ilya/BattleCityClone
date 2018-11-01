using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (TileSettings))]
[TableEditorProperties ("configs")]
[NonTablePropertiesOrderAttribute(NonTablePropsOrder.BeforeTable)]
public class TileSettingsEditor : TableEditor { }