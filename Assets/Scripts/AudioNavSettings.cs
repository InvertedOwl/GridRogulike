using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class AudioNavSettings : MonoBehaviour
{
    private const string MasterVolumeKey = "masterVolume";
    private const string MusicVolumeKey = "musicVolume";
    private const string SfxVolumeKey = "sfxVolume";
    private const string UiVolumeKey = "uiVolume";
    private const string MuteWhenUnfocusedKey = "muteWhenUnfocused";

    [Header("Mixer")]
    public AudioMixer AudioMixer;
    public string MasterVolumeParameter = "MasterVolume";
    public string MusicVolumeParameter = "MusicVolume";
    public string SfxVolumeParameter = "SFXVolume";
    public string UiVolumeParameter = "UIVolume";

    [Header("Controls")]
    public Slider MasterVolume;
    public Slider MusicVolume;
    public Slider SfxVolume;
    public Slider UiVolume;
    public Toggle MuteWhenUnfocused;

    private bool _hasFocus = true;

    public void Start()
    {
        LoadSettings();
        ApplySettings();
    }

    public void LoadSettings()
    {
        SetSliderWithoutNotify(MasterVolume, PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        SetSliderWithoutNotify(MusicVolume, PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        SetSliderWithoutNotify(SfxVolume, PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
        SetSliderWithoutNotify(UiVolume, PlayerPrefs.GetFloat(UiVolumeKey, 1f));

        if (MuteWhenUnfocused != null)
            MuteWhenUnfocused.SetIsOnWithoutNotify(PlayerPrefs.GetInt(MuteWhenUnfocusedKey, 0) == 1);
    }

    public void ApplySettings()
    {
        ApplyMixerVolume(MasterVolumeParameter, GetSliderValue(MasterVolume));
        ApplyMixerVolume(MusicVolumeParameter, GetSliderValue(MusicVolume));
        ApplyMixerVolume(SfxVolumeParameter, GetSliderValue(SfxVolume));
        ApplyMixerVolume(UiVolumeParameter, GetSliderValue(UiVolume));
        ApplyFocusMute();
    }

    public void SetMasterVolume()
    {
        SaveAndApplyVolume(MasterVolumeKey, MasterVolumeParameter, MasterVolume);
    }

    public void SetMusicVolume()
    {
        SaveAndApplyVolume(MusicVolumeKey, MusicVolumeParameter, MusicVolume);
    }

    public void SetSfxVolume()
    {
        SaveAndApplyVolume(SfxVolumeKey, SfxVolumeParameter, SfxVolume);
    }

    public void SetSFXVolume()
    {
        SetSfxVolume();
    }

    public void SetUiVolume()
    {
        SaveAndApplyVolume(UiVolumeKey, UiVolumeParameter, UiVolume);
    }

    public void SetUIVolume()
    {
        SetUiVolume();
    }

    public void SetMuteWhenUnfocused()
    {
        PlayerPrefs.SetInt(MuteWhenUnfocusedKey, IsMuteWhenUnfocusedEnabled() ? 1 : 0);
        PlayerPrefs.Save();
        ApplyFocusMute();
    }

    private void SaveAndApplyVolume(string prefsKey, string mixerParameter, Slider slider)
    {
        float value = GetSliderValue(slider);
        PlayerPrefs.SetFloat(prefsKey, value);
        PlayerPrefs.Save();
        ApplyMixerVolume(mixerParameter, value);
    }

    private void ApplyMixerVolume(string mixerParameter, float sliderValue)
    {
        if (AudioMixer == null || string.IsNullOrWhiteSpace(mixerParameter))
            return;

        AudioMixer.SetFloat(mixerParameter, SliderToDecibels(sliderValue));
    }

    private static float SliderToDecibels(float value)
    {
        value = Mathf.Clamp01(value);
        if (value <= 0f)
            return -80f;

        return Mathf.Log10(value) * 20f;
    }

    private static float GetSliderValue(Slider slider)
    {
        return slider != null ? Mathf.Clamp01(slider.value) : 1f;
    }

    private static void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private bool IsMuteWhenUnfocusedEnabled()
    {
        return MuteWhenUnfocused != null
            ? MuteWhenUnfocused.isOn
            : PlayerPrefs.GetInt(MuteWhenUnfocusedKey, 0) == 1;
    }

    private void ApplyFocusMute()
    {
        AudioListener.volume = IsMuteWhenUnfocusedEnabled() && !_hasFocus ? 0f : 1f;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        _hasFocus = hasFocus;
        ApplyFocusMute();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        _hasFocus = !pauseStatus;
        ApplyFocusMute();
    }
}
