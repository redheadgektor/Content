#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using StringComparison = System.StringComparison;
using UnityEditorInternal;

public partial class Content
{
    static readonly HashSet<Type> whiteListTypes = new HashSet<Type>
    {
        typeof(AudioClip),
        typeof(AnimationClip),
        typeof(Shader),
        typeof(GameObject),
        typeof(ComputeShader),
        typeof(Texture),
        typeof(Font),
        typeof(Material),
        typeof(PhysicMaterial),
        typeof(TextAsset),
        typeof(ScriptableObject),
        typeof(SceneAsset),
        typeof(Object)
    };

    static readonly HashSet<Type> blackListTypes = new HashSet<Type>
    {
        typeof(Content),
        typeof(MonoScript),
        typeof(DefaultAsset),
        typeof(AssemblyDefinitionAsset),
        typeof(AssemblyDefinitionReferenceAsset),
    };

    public static bool IsSupportedType(Type type)
    {
        return whiteListTypes.Contains(type)
            || whiteListTypes.Contains(type.BaseType)
                && !blackListTypes.Contains(type)
                && !blackListTypes.Contains(type.BaseType);
    }

    public static bool IsResource(string path)
    {
        return path.Contains("/resources/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/editor resources/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/editor resources", StringComparison.OrdinalIgnoreCase)
            || path.Contains("editor resources/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/package resources/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/package resources", StringComparison.OrdinalIgnoreCase)
            || path.Contains("package resources/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/resources", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsEditor(string path)
    {
        return path.Contains("/editor/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/editor", StringComparison.OrdinalIgnoreCase);
    }
}
#endif
