using System;
using UnityEngine;

[Serializable]
public class TypeReference : ISerializationCallbackReceiver
{
#pragma warning disable 649, 414
    [SerializeField, HideInInspector] private string typePath = string.Empty;
    [NonSerialized] private Type type = null;
#pragma warning restore 649, 414

    public bool IsEmpty { get { return string.IsNullOrEmpty(typePath); } }

    public override bool Equals(object obj)
    {
        if (obj == null) { return IsEmpty; }
        if (!(obj is TypeReference)) { return false; }
        var other = obj as TypeReference;

        return typePath.Equals(other.typePath);
    }

    public override int GetHashCode()
    {
        return typePath.GetHashCode();
    }

    public void OnAfterDeserialize()
    {
        if (!IsEmpty) { ExtractType(); }
    }

    private void ExtractType()
    {
        type = Type.GetType(typePath);
        if (type == null)
        {
            Clear();
        }
    }

    void Clear()
    {
        typePath = string.Empty;
    }

    public void OnBeforeSerialize() { }

    public object GetObject(params object[] args)
    {
        return Activator.CreateInstance(type, args);
    }

    public T GetObject<T>(params object[] args) where T : class
    {
        return GetObject(args) as T;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TypeReferenceBaseAttribute : Attribute
{
    private Type baseType;

    public Type BaseType { get { return baseType; } }

    public TypeReferenceBaseAttribute(Type baseType)
    {
        this.baseType = baseType;
    }
}