using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Zenject;

public class BaseLoadSaveData : MonoBehaviour
{
    public static BaseList Bases;

    private const string FileText = "BASELIST"; // можно оставить или поменять
    private const string FileExtension = ".json";
    private Coroutine _saveCoroutine;

    private string FullPath => Path.Combine(Application.persistentDataPath, FileText + FileExtension);

    [Inject]
    private void Construct(BaseList bases)
    {
        // Если Zenject предоставляет инстанс — используем его, иначе создаём новый
       Bases = bases;
    }

    private void Awake()
    {
        // Попробуем загрузить из файла; если файла нет — оставляем _bases (инжектированный или новый)
        var loaded = LoadBaseListFromJson(FileText);
        if (loaded != null && loaded.Count > 0)
        {
            Bases = loaded;
        }
        else
        {
            Debug.Log("No saved base list found, using provided/empty list.");
        }
    }

    private void OnEnable()
    {
        // Запускаем периодическое сохранение
        _saveCoroutine = StartCoroutine(AutoSaveCoroutine());
    }

    private void OnDisable()
    {
        // Остановить корутину
        if (_saveCoroutine != null)
        {
            StopCoroutine(_saveCoroutine);
            _saveCoroutine = null;
        }
        
        // Очистить (удалить) старый файл перед сохранением
        TryDeleteSaveFile();

        // Сохраняем текущее состояние (файл будет создан заново)
        SaveBaseListToJson(Bases, FileText);

    }

    private void OnDestroy()
    {
        // Остановить корутину
        if (_saveCoroutine != null)
        {
            StopCoroutine(_saveCoroutine);
            _saveCoroutine = null;
        }

        // Очистить старый файл и сохранить
        TryDeleteSaveFile();
        SaveBaseListToJson(Bases, FileText);
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            Debug.Log("C=" + (Bases != null ? Bases.Count.ToString() : "null"));
            SaveBaseListToJson(Bases, FileText);
            yield return new WaitForSeconds(1f);
            
        }
    }

    // Вспомогательный метод для удаления файла с защитой
    private void TryDeleteSaveFile()
    {
        try
        {
            string path = FullPath;
            if (File.Exists(path))
            {
                File.Delete(path);
#if UNITY_EDITOR
                Debug.Log("[BaseLoadSaveData] Deleted old save file: " + path);
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[BaseLoadSaveData] Error deleting save file: " + ex);
        }
    }

    // Публичный метод для явного сохранения
    public void SaveNow()
    {
        // перед явной записью тоже удалим старый файл
        TryDeleteSaveFile();
        SaveBaseListToJson(Bases, FileText);
    }

    // Сохранение с обёрткой для JsonUtility
    public void SaveBaseListToJson(BaseList baseList, string filename)
    {
        try
        {
            if (baseList == null) baseList = new BaseList();

            var wrapper = new BaseListWrapper { items = baseList.ToArray() };
            string jsonString = JsonUtility.ToJson(wrapper, true);
            Debug.Log("Saved JSON: " + jsonString);

            string filePath = Path.Combine(Application.persistentDataPath, filename + FileExtension);
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving base list: " + ex);
        }
    }

    // Загрузка и разворачивание обратно в BaseList
    public BaseList LoadBaseListFromJson(string filename)
    {
        string filePath = Path.Combine(Application.persistentDataPath, filename + FileExtension);
        if (File.Exists(filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var wrapper = JsonUtility.FromJson<BaseListWrapper>(jsonString);
                var result = new BaseList();
                if (wrapper != null && wrapper.items != null)
                {
                    result.AddRange(wrapper.items);
                }
                Debug.Log("Loaded base list from: " + filePath);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading base list: " + ex);
                return new BaseList();
            }
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
            return new BaseList(); // Возвращаем новый пустой список
        }
    }

    // Обёртка, сериализуемая JsonUtility
    [Serializable]
    private class BaseListWrapper
    {
        public BaseElementsSaveData[] items;
    }

    // Дополнительные методы для работы с _bases
    public BaseList GetBases() => Bases;
    public void SetBases(BaseList list) => Bases = list ?? new BaseList();

    public void AddBase(BaseElementsSaveData element)
    {
        if (Bases == null) Bases = new BaseList();
        Bases.Add(element);
    }

    public bool RemoveBaseById(string id)
    {
        if (Bases == null) return false;
        var idx = Bases.FindIndex(b => b.ID == id);
        if (idx >= 0)
        {
            Bases.RemoveAt(idx);
            return true;
        }
        return false;
    }
}