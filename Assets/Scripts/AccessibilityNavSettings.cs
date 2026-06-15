using UnityEngine;
using UnityEngine.UI;

public class AccessibilityNavSettings : MonoBehaviour
{
    private const string DisableScreenShakeKey = "disableScreenShake";

    public Toggle DisableScreenShakeToggle;

    public static bool DisableScreenShake { get; private set; }
    private static bool _loaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsOnLoad()
    {
        DisableScreenShake = false;
        _loaded = false;
    }

    public void Start()
    {
        LoadSettings();
    }

    public void LoadSettings()
    {
        LoadStaticSettings();

        if (DisableScreenShakeToggle != null)
            DisableScreenShakeToggle.SetIsOnWithoutNotify(DisableScreenShake);
    }

    public void SetDisableScreenShake()
    {
        DisableScreenShake = DisableScreenShakeToggle != null && DisableScreenShakeToggle.isOn;
        _loaded = true;

        PlayerPrefs.SetInt(DisableScreenShakeKey, DisableScreenShake ? 1 : 0);
        PlayerPrefs.Save();

        if (DisableScreenShake && ScreenShake.Instance != null)
            ScreenShake.Instance.StopShake();
    }

    public static bool IsScreenShakeDisabled()
    {
        LoadStaticSettings();
        return DisableScreenShake;
    }

    private static void LoadStaticSettings()
    {
        if (_loaded)
            return;

        DisableScreenShake = PlayerPrefs.GetInt(DisableScreenShakeKey, 0) == 1;
        _loaded = true;
    }
}
