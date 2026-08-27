using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
#endif

public sealed class FXKeyAttribute : PropertyAttribute
{
}

public class FXManager : MonoBehaviour
{
    [Serializable]
    public class FXEntry
    {
        public string key;
        public GameObject prefab;
        public Vector3 spawnOffset;
        [Min(0f)] public float lifetime = 2f;
        public bool usePrefabRotation = true;
        public bool parentToTarget;
    }

    public static FXManager Instance { get; private set; }

    [SerializeField] private string prefabFolder = "Assets/Epic Toon FX/Prefabs";
    [SerializeField] private bool useRelativeFolderKeys;
    [SerializeField, Min(0f)] private float defaultLifetime = 2f;
    [SerializeField] private int sortingOrder = 200;
    [SerializeField] private Vector3 spawnScale = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField] private bool randomizeSoundPitch = true;
    [SerializeField] private Vector2 soundPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private AudioMixerGroup sfxOutputGroup;
    [SerializeField] private string sfxOutputGroupName = "SFX";
    [SerializeField] private List<FXEntry> effects = new List<FXEntry>();

    private readonly Dictionary<string, FXEntry> _lookup =
        new Dictionary<string, FXEntry>(StringComparer.OrdinalIgnoreCase);
    private AudioMixerGroup _resolvedSfxOutputGroup;
    private bool _hasTriedResolveSfxOutputGroup;

    public IReadOnlyList<FXEntry> Effects => effects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple FXManager instances found. Using the newest one.", this);
        }

        Instance = this;
        RebuildLookup();
    }

    private void OnValidate()
    {
        _resolvedSfxOutputGroup = null;
        _hasTriedResolveSfxOutputGroup = false;
        RebuildLookup();
    }

    public bool HasEffect(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _lookup.ContainsKey(key);
    }

    public GameObject Play(string key, Transform target)
    {
        if (target == null)
            return null;

        return Play(key, target.position, target.rotation, target);
    }

    public bool TryPlay(string key, Transform target)
    {
        return TryPlay(key, target, out _);
    }

    public bool TryPlay(string key, Transform target, out GameObject instance)
    {
        instance = null;
        if (target == null)
            return false;

        return TryPlay(key, target.position, target.rotation, target, out instance);
    }

    public GameObject Play(string key, Vector3 position)
    {
        return Play(key, position, Quaternion.identity);
    }

    public bool TryPlay(string key, Vector3 position)
    {
        return TryPlay(key, position, Quaternion.identity, null, out _);
    }

    public bool TryPlay(string key, Vector3 position, out GameObject instance)
    {
        return TryPlay(key, position, Quaternion.identity, null, out instance);
    }

    public GameObject Play(string key, Vector3 position, Quaternion rotation, Transform target = null)
    {
        if (!TryGetEntry(key, out FXEntry entry))
        {
            Debug.LogWarning($"FXManager could not find effect key '{key}'.", this);
            return null;
        }

        return Spawn(entry, position, rotation, target);
    }

    public bool TryPlay(
        string key,
        Vector3 position,
        Quaternion rotation,
        Transform target,
        out GameObject instance)
    {
        instance = null;
        if (!TryGetEntry(key, out FXEntry entry))
            return false;

        instance = Spawn(entry, position, rotation, target);
        return instance != null;
    }

    public GameObject PlayAtIndex(int index, Vector3 position)
    {
        if (index < 0 || index >= effects.Count)
        {
            Debug.LogWarning($"FXManager effect index {index} is out of range.", this);
            return null;
        }

        return Spawn(effects[index], position, Quaternion.identity, null);
    }

    private bool TryGetEntry(string key, out FXEntry entry)
    {
        entry = null;

        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (_lookup.TryGetValue(key, out entry))
            return true;

        RebuildLookup();
        return _lookup.TryGetValue(key, out entry);
    }

    private GameObject Spawn(FXEntry entry, Vector3 position, Quaternion rotation, Transform target)
    {
        if (entry == null || entry.prefab == null)
            return null;

        Vector3 spawnPosition = position + entry.spawnOffset;
        Quaternion spawnRotation = entry.usePrefabRotation ? entry.prefab.transform.rotation : rotation;
        Transform parent = entry.parentToTarget ? target : null;

        GameObject instance = Instantiate(entry.prefab, spawnPosition, spawnRotation, parent);
        instance.name = string.IsNullOrWhiteSpace(entry.key) ? entry.prefab.name : $"FX_{entry.key}";
        instance.transform.localScale = spawnScale;
        ApplySortingOrder(instance);
        ApplyAudioSettings(instance);

        if (entry.lifetime > 0f)
        {
            Destroy(instance, entry.lifetime);
        }

        return instance;
    }

    private void ApplySortingOrder(GameObject instance)
    {
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sortingOrder = sortingOrder;
        }
    }

    private void ApplyAudioSettings(GameObject instance)
    {
        float minPitch = Mathf.Min(soundPitchRange.x, soundPitchRange.y);
        float maxPitch = Mathf.Max(soundPitchRange.x, soundPitchRange.y);
        AudioMixerGroup outputGroup = GetSfxOutputGroup();

        foreach (AudioSource audioSource in instance.GetComponentsInChildren<AudioSource>(true))
        {
            if (outputGroup != null)
            {
                audioSource.outputAudioMixerGroup = outputGroup;
            }

            audioSource.volume *= soundVolume;

            if (randomizeSoundPitch)
            {
                audioSource.pitch *= UnityEngine.Random.Range(minPitch, maxPitch);
            }
        }
    }

    private AudioMixerGroup GetSfxOutputGroup()
    {
        if (sfxOutputGroup != null)
            return sfxOutputGroup;

        if (_hasTriedResolveSfxOutputGroup)
            return _resolvedSfxOutputGroup;

        _hasTriedResolveSfxOutputGroup = true;

        if (string.IsNullOrWhiteSpace(sfxOutputGroupName))
            return null;

        foreach (AudioMixerGroup group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
        {
            if (group != null &&
                string.Equals(group.name, sfxOutputGroupName, StringComparison.OrdinalIgnoreCase))
            {
                _resolvedSfxOutputGroup = group;
                return _resolvedSfxOutputGroup;
            }
        }

        return null;
    }

    private void RebuildLookup()
    {
        _lookup.Clear();

        if (effects == null)
            return;

        foreach (FXEntry entry in effects)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.prefab == null)
                continue;

            if (!_lookup.ContainsKey(entry.key))
            {
                _lookup.Add(entry.key, entry);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Populate FX Entries From Prefab Folder")]
    private void PopulateFxEntriesFromPrefabFolder()
    {
        if (string.IsNullOrWhiteSpace(prefabFolder))
        {
            Debug.LogWarning("FXManager needs a prefab folder before it can populate entries.", this);
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });
        Array.Sort(prefabGuids, CompareAssetPaths);

        effects.Clear();
        HashSet<string> usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            effects.Add(new FXEntry
            {
                key = GetUniqueKey(path, usedKeys),
                prefab = prefab,
                lifetime = defaultLifetime,
                usePrefabRotation = true
            });
        }

        RebuildLookup();
        EditorUtility.SetDirty(this);

        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        Debug.Log($"FXManager populated {effects.Count} entries from '{prefabFolder}'.", this);
    }

    private static int CompareAssetPaths(string a, string b)
    {
        return string.Compare(
            AssetDatabase.GUIDToAssetPath(a),
            AssetDatabase.GUIDToAssetPath(b),
            StringComparison.OrdinalIgnoreCase);
    }

    private string GetUniqueKey(string assetPath, HashSet<string> usedKeys)
    {
        string key = useRelativeFolderKeys
            ? BuildRelativeKey(assetPath)
            : BuildFilenameKey(assetPath);

        if (usedKeys.Add(key))
            return key;

        string relativeKey = BuildRelativeKey(assetPath);
        if (usedKeys.Add(relativeKey))
            return relativeKey;

        int suffix = 2;
        string candidate;
        do
        {
            candidate = $"{key}_{suffix}";
            suffix++;
        }
        while (!usedKeys.Add(candidate));

        return candidate;
    }

    private static string BuildFilenameKey(string assetPath)
    {
        string normalized = assetPath.Replace("\\", "/");
        int lastSlash = normalized.LastIndexOf("/", StringComparison.Ordinal);
        string filename = lastSlash >= 0 ? normalized.Substring(lastSlash + 1) : normalized;
        return RemovePrefabExtension(filename);
    }

    private string BuildRelativeKey(string assetPath)
    {
        string normalizedPath = assetPath.Replace("\\", "/");
        string normalizedFolder = prefabFolder.Replace("\\", "/").TrimEnd('/') + "/";

        if (normalizedPath.StartsWith(normalizedFolder, StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath = normalizedPath.Substring(normalizedFolder.Length);
        }

        return RemovePrefabExtension(normalizedPath);
    }

    private static string RemovePrefabExtension(string path)
    {
        const string prefabExtension = ".prefab";
        return path.EndsWith(prefabExtension, StringComparison.OrdinalIgnoreCase)
            ? path.Substring(0, path.Length - prefabExtension.Length)
            : path;
    }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FXKeyAttribute))]
public sealed class FXKeyDrawer : PropertyDrawer
{
    private const string NoneLabel = "None";
    private const string DefaultFxPrefabFolder = "Assets/Epic Toon FX/Prefabs";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        Rect buttonRect = EditorGUI.PrefixLabel(position, label);
        string currentKey = property.stringValue;
        string buttonLabel = string.IsNullOrWhiteSpace(currentKey) ? NoneLabel : currentKey;

        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(buttonLabel), FocusType.Keyboard))
        {
            UnityEngine.Object[] targets = property.serializedObject.targetObjects;
            string propertyPath = property.propertyPath;
            FXKeyDropdown dropdown = new FXKeyDropdown(
                new AdvancedDropdownState(),
                GetAvailableKeys(currentKey),
                selectedKey => ApplySelection(targets, propertyPath, selectedKey));
            dropdown.Show(buttonRect);
        }

        EditorGUI.EndProperty();
    }

    private static List<string> GetAvailableKeys(string currentKey)
    {
        HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (FXManager manager in Resources.FindObjectsOfTypeAll<FXManager>())
        {
            if (manager == null)
                continue;

            foreach (FXManager.FXEntry entry in manager.Effects)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.key))
                    keys.Add(entry.key);
            }
        }

        // Rules are often edited without the gameplay scene open. The manager's default setup
        // uses prefab filenames as keys, so keep the picker useful in that workflow too.
        if (keys.Count == 0 && AssetDatabase.IsValidFolder(DefaultFxPrefabFolder))
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { DefaultFxPrefabFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string key = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrWhiteSpace(key))
                    keys.Add(key);
            }
        }

        if (!string.IsNullOrWhiteSpace(currentKey))
            keys.Add(currentKey);

        List<string> sortedKeys = new List<string>(keys);
        sortedKeys.Sort(StringComparer.OrdinalIgnoreCase);
        return sortedKeys;
    }

    private static void ApplySelection(UnityEngine.Object[] targets, string propertyPath, string selectedKey)
    {
        foreach (UnityEngine.Object target in targets)
        {
            if (target == null)
                continue;

            SerializedObject serializedTarget = new SerializedObject(target);
            SerializedProperty targetProperty = serializedTarget.FindProperty(propertyPath);
            if (targetProperty == null)
                continue;

            Undo.RecordObject(target, $"Select {targetProperty.displayName}");
            targetProperty.stringValue = selectedKey;
            serializedTarget.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    private sealed class FXKeyDropdown : AdvancedDropdown
    {
        private readonly List<string> keys;
        private readonly Action<string> onSelected;

        public FXKeyDropdown(
            AdvancedDropdownState state,
            List<string> keys,
            Action<string> onSelected) : base(state)
        {
            this.keys = keys;
            this.onSelected = onSelected;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Attack VFX");
            root.AddChild(new FXKeyDropdownItem(NoneLabel, string.Empty));

            foreach (string key in keys)
            {
                FXKeyDropdownItem item = new FXKeyDropdownItem(key, key);
                root.AddChild(item);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is FXKeyDropdownItem fxItem)
                onSelected?.Invoke(fxItem.Key);
        }
    }

    private sealed class FXKeyDropdownItem : AdvancedDropdownItem
    {
        public string Key { get; }

        public FXKeyDropdownItem(string name, string key) : base(name)
        {
            Key = key;
        }
    }
}
#endif
