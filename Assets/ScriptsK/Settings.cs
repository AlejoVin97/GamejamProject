using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] 
    Slider sliderMusic,sliderSFX;
    [SerializeField] AudioMixer audioM;

    void Start()
    {
        float musicValue = PlayerPrefs.GetFloat("ValueMusic", 1f);
        float sfxValue = PlayerPrefs.GetFloat("ValueSFX",1f);
        sliderMusic.value = musicValue;
        sliderSFX.value = sfxValue;
        MusicChanges(musicValue);
        SFXChanges(sfxValue);
        sliderMusic.onValueChanged.AddListener(MusicChanges);
        sliderSFX.onValueChanged.AddListener(SFXChanges);
    }

    public void MusicChanges(float value)
    {
        ApplyChanges("ValueMusic", value);
        PlayerPrefs.SetFloat("ValueMusic", value);
        PlayerPrefs.Save();
    }

    public void SFXChanges(float value)
    {
        ApplyChanges("ValueSFX", value);
        PlayerPrefs.SetFloat("ValueSFX", value);
        PlayerPrefs.Save();
    }
    void ApplyChanges(string name,float value)
    {
        float decibels;
        if (value <= 0.0f)
            decibels = -80f;
        else
            decibels = Mathf.Log10(value) * 20f;

        audioM.SetFloat(name, decibels);

    }
}
