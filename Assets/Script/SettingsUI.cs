using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsUI : MonoBehaviour
{
    public GameObject optionPanel;

    public Slider bgmSlider;
    public Slider seSlider;

    public AudioMixer mixer;

    private void Start()
    {
        bgmSlider.onValueChanged.AddListener(SetBGM);
        seSlider.onValueChanged.AddListener(SetSE);

        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        seSlider.value = AudioManager.seVolume;

        SetBGM(bgmSlider.value);
        SetSE(seSlider.value);
    }

    public void ToggleOption()
    {
        optionPanel.SetActive(!optionPanel.activeSelf);
    }

    private void SetBGM(float value)
    {
        value = Mathf.Max(value, 0.001f);

        mixer.SetFloat(
            "BGMVolume",
            Mathf.Log10(value) * 20f
        );

        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    private void SetSE(float value)
    {
        value = Mathf.Clamp01(value);

        // AudioManagerが参照する音量を変更
        AudioManager.seVolume = value;

        // Mixerを使う場合も更新
        mixer.SetFloat(
            "SEVolume",
            Mathf.Log10(Mathf.Max(value, 0.001f)) * 20f
        );
    }
}