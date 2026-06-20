#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ScriptableObject Icon Map",
    menuName = "Editor/ScriptableObject Icon Map"
)]
public class ScriptableObjectIconMap : ScriptableObject
{
    public List<Entry> entries = new();

    [Serializable]
    public class Entry
    {
        [Tooltip("Drag in the C# script that defines your ScriptableObject type.")]
        public MonoScript scriptableObjectScript;

        [Tooltip("Icon to show for assets of this ScriptableObject type.")]
        public Texture2D icon;

        [Tooltip("If true, derived ScriptableObject classes will also use this icon.")]
        public bool includeDerivedTypes = true;

        public Type GetTargetType()
        {
            if (scriptableObjectScript == null)
                return null;

            Type type = scriptableObjectScript.GetClass();

            if (type == null)
                return null;

            if (!typeof(ScriptableObject).IsAssignableFrom(type))
                return null;

            return type;
        }
    }
}

[InitializeOnLoad]
public static class ScriptableObjectProjectIcons
{
    private const string ConfigSearchFilter = "t:ScriptableObjectIconMap";

    private static ScriptableObjectIconMap _config;
    private static readonly Dictionary<Type, Texture2D> _exactTypeIcons = new();
    private static readonly List<(Type type, Texture2D icon)> _derivedTypeIcons = new();

    static ScriptableObjectProjectIcons()
    {
        ReloadConfig();

        EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

        EditorApplication.projectChanged -= ReloadConfig;
        EditorApplication.projectChanged += ReloadConfig;
    }

    private static void ReloadConfig()
    {
        _config = FindConfig();

        _exactTypeIcons.Clear();
        _derivedTypeIcons.Clear();

        if (_config == null)
            return;

        foreach (var entry in _config.entries)
        {
            if (entry == null || entry.icon == null)
                continue;

            Type targetType = entry.GetTargetType();

            if (targetType == null)
                continue;

            if (entry.includeDerivedTypes)
            {
                _derivedTypeIcons.Add((targetType, entry.icon));
            }
            else
            {
                _exactTypeIcons[targetType] = entry.icon;
            }
        }

        EditorApplication.RepaintProjectWindow();
    }

    private static ScriptableObjectIconMap FindConfig()
    {
        string[] guids = AssetDatabase.FindAssets(ConfigSearchFilter);

        if (guids == null || guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<ScriptableObjectIconMap>(path);
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (string.IsNullOrEmpty(path))
            return;

        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

        if (asset == null)
            return;

        Texture2D icon = GetIconForType(asset.GetType());

        if (icon == null)
            return;

        Rect iconRect = GetIconRect(selectionRect);

        if (iconRect.width <= 0 || iconRect.height <= 0)
            return;

        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
    }

    private static Texture2D GetIconForType(Type assetType)
    {
        if (assetType == null)
            return null;

        if (_exactTypeIcons.TryGetValue(assetType, out Texture2D exactIcon))
            return exactIcon;

        foreach (var mapping in _derivedTypeIcons)
        {
            if (mapping.type.IsAssignableFrom(assetType))
                return mapping.icon;
        }

        return null;
    }

    private static Rect GetIconRect(Rect selectionRect)
    {
        bool isListMode = selectionRect.height <= 20f;

        if (isListMode)
        {
            // Small icon beside the asset name in list view.
            return new Rect(
                selectionRect.x,
                selectionRect.y,
                16f,
                16f
            );
        }

        // Larger thumbnail mode.
        float size = Mathf.Min(selectionRect.width, selectionRect.height - 18f);
        size = Mathf.Clamp(size, 24f, 64f);

        return new Rect(
            selectionRect.x + (selectionRect.width - size) * 0.5f,
            selectionRect.y + 4f,
            size,
            size
        );
    }
}
#endif