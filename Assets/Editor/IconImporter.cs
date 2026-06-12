using UnityEngine;

using UnityEditor;
using UnityEngine;

public class IconImporter : AssetPostprocessor
{
    private const string IconFolder = "Assets/Sprites/Icons/";
    private const string SpriteDatabasePath = "Assets/Datas/SpriteDatabase.asset";

    private static bool IsIconPath(string path)
    {
        path = path.Replace("\\", "/");
        return path.StartsWith(IconFolder, System.StringComparison.OrdinalIgnoreCase);
    }

    private void OnPreprocessTexture()
    {
        if (!IsIconPath(assetPath))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);

        settings.spriteMeshType = SpriteMeshType.FullRect;

        importer.SetTextureSettings(settings);
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool changed = false;

        foreach (string path in importedAssets)
        {
            if (!IsIconPath(path))
                continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                continue;

            changed |= AddOrUpdateSpriteDatabaseEntry(sprite);
        }

        foreach (string path in movedAssets)
        {
            if (!IsIconPath(path))
                continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                continue;

            changed |= AddOrUpdateSpriteDatabaseEntry(sprite);
        }

        if (changed)
        {
            AssetDatabase.SaveAssets();
        }
    }

    private static bool AddOrUpdateSpriteDatabaseEntry(Sprite sprite)
    {
        Object database = AssetDatabase.LoadAssetAtPath<Object>(SpriteDatabasePath);

        if (database == null)
        {
            Debug.LogWarning($"SpriteDatabase not found at: {SpriteDatabasePath}");
            return false;
        }

        SerializedObject serializedDatabase = new SerializedObject(database);
        SerializedProperty spritesProperty = serializedDatabase.FindProperty("sprites");

        if (spritesProperty == null || !spritesProperty.isArray)
        {
            Debug.LogWarning("Could not find serialized List<SpriteInfo> field named 'sprites' on SpriteDatabase.");
            return false;
        }

        string key = sprite.name;

        for (int i = 0; i < spritesProperty.arraySize; i++)
        {
            SerializedProperty entry = spritesProperty.GetArrayElementAtIndex(i);
            SerializedProperty keyProperty = entry.FindPropertyRelative("key");
            SerializedProperty spriteProperty = entry.FindPropertyRelative("sprite");

            if (keyProperty.stringValue == key)
            {
                if (spriteProperty.objectReferenceValue != sprite)
                {
                    spriteProperty.objectReferenceValue = sprite;
                    serializedDatabase.ApplyModifiedProperties();
                    EditorUtility.SetDirty(database);
                    Debug.Log($"Updated icon sprite database entry: {key}");
                    return true;
                }

                return false;
            }
        }

        int newIndex = spritesProperty.arraySize;
        spritesProperty.InsertArrayElementAtIndex(newIndex);

        SerializedProperty newEntry = spritesProperty.GetArrayElementAtIndex(newIndex);
        newEntry.FindPropertyRelative("key").stringValue = key;
        newEntry.FindPropertyRelative("sprite").objectReferenceValue = sprite;

        serializedDatabase.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);

        Debug.Log($"Added icon sprite database entry: {key}");

        return true;
    }
}