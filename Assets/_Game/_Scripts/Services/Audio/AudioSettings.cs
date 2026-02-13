using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Не забудьте добавить для работы с UI

public class AudioSettings : MonoBehaviour
{
    // Ссылки на шины (Buses) FMOD
    private FMOD.Studio.Bus masterBus;
    private FMOD.Studio.Bus musicBus;
    private FMOD.Studio.Bus sfxBus;

    // Переменные для хранения уровня громкости (от 0 до 1)
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    // Ссылки на UI-слайдеры (перетащите их в инспекторе)
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool _isInit;

    public void Initialize(MusicClip clip)
    {
        // Инициализируем шины. Пути должны совпадать с путями в вашем проекте FMOD!
        masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");

        SelectClip(clip);


        _isInit =true;
    }
    private void SelectClip(MusicClip clip)
    {
        musicBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        switch (clip) 
        {
            
            case MusicClip.Menu: FMODUnity.RuntimeManager.PlayOneShot("event:/Menu");break;
            case MusicClip.Base: FMODUnity.RuntimeManager.PlayOneShot("event:/Base"); break;
            case MusicClip.Level: FMODUnity.RuntimeManager.PlayOneShot("event:/Level"); break;
        }

    }
    void Update()
    {
        if (!_isInit) return;
        // Постоянно применяем текущие значения громкости к шинам FMOD
        masterBus.setVolume(masterSlider.value);
        musicBus.setVolume(musicSlider.value);
        sfxBus.setVolume(sfxSlider.value);
    }

    // Эти методы нужно привязать к событиям "OnValueChanged" слайдеров
    public void SetMasterVolume(float newVolume)
    {
        masterVolume = newVolume;
    }

    public void SetMusicVolume(float newVolume)
    {
        musicVolume = newVolume;
    }

    public void SetSFXVolume(float newVolume)
    {
        sfxVolume = newVolume;
    }
}
public enum MusicClip
{
    Menu,
    Base,
    Level,
    Forest,
    Willage,
    Town,
    City,
}