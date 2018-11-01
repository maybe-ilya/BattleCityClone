using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public partial class LevelEditor {
	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class LevelEditorInitializeAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class LevelEditorOnEnableAttribute : Attribute { }

	private void InvokeMethodsWithAttribute(Type attributeType) {
		var searchOptions = BindingFlags.InvokeMethod |
			BindingFlags.Public |
			BindingFlags.NonPublic |
			BindingFlags.Instance;
		var methods = GetType()
			.GetMethods(searchOptions)
			.Where(method => Attribute.IsDefined(method, attributeType));
		foreach (var method in methods) {
			method.Invoke(this, null);
		}
	}
}