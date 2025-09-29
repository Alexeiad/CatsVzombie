using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slider_SaveManager : MonoBehaviour
{
    // Ссылка на панель или объект, содержащий слайдеры
    public GameObject slidersPanel;
    // Список для хранения слайдеров
    private List<Slider> slidersList = new List<Slider>();

    void Start()
    {
        // Получаем все слайдеры с панели (или создаем список вручную через Inspector)
        Slider[] foundSliders = slidersPanel.GetComponentsInChildren<Slider>();
        slidersList.AddRange(foundSliders);

        // Загружаем сохраненные позиции для каждого слайдера
        for (int i = 0; i < slidersList.Count; i++)
        {
            // Создаем уникальный ключ для каждого слайдера, например, "Slider0", "Slider1"
            string uniqueKey = "Slider" + i;

            // Подписываемся на событие изменения значения
            int index = i; // Важно скопировать значение для замыкания
            slidersList[i].onValueChanged.AddListener((float value) => { OnSliderValueChanged(index, value); });

            // Загружаем и устанавливаем сохраненное значение
            slidersList[i].value = PlayerPrefs.GetFloat(uniqueKey, slidersList[i].value);
        }
    }

    // Этот метод вызывается при изменении значения любого слайдера
    private void OnSliderValueChanged(int sliderIndex, float value)
    {
        // Сохраняем значение по уникальному ключу
        string uniqueKey = "Slider" + sliderIndex;
        PlayerPrefs.SetFloat(uniqueKey, value);
        // PlayerPrefs.Save(); // Можно вызывать реже, например, при выходе из игры:cite[9]
    }

    // Вызовите этот метод, например, при закрытии меню или выходе из игры
    public void SaveAllSettings()
    {
        PlayerPrefs.Save();
        Debug.Log("Все настройки сохранены.");
    }
}
