using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Zenject;

public class BaseSaveManager : MonoBehaviour
{
    private BuildingsList _bases;

    private const string FileText = "BASELIST";
    private const string FileExtension = ".json";
    private Coroutine _saveCoroutine;

    private string FullPath => Path.Combine(Application.persistentDataPath, FileText + FileExtension);

    [Inject]
    private void Construct(BuildingsList bases)
    {
       _bases = bases;
    }

    private void Awake()
    {

        var loaded = LoadBaseListFromJson(FileText);
        if (loaded != null && loaded.Count > 0)
        {
            _bases = loaded;
        }
        else
        {
            Debug.Log("No saved base list found, using provided/empty list.");
        }
    }

    private void OnEnable()
    {
        _saveCoroutine = StartCoroutine(AutoSaveCoroutine());
    }

    private void OnDisable()
    {
        if (_saveCoroutine != null)
        {
            StopCoroutine(_saveCoroutine);
            _saveCoroutine = null;
        }
        
        TryDeleteSaveFile();
        SaveBaseListToJson(_bases, FileText);

    }

    private void OnDestroy()
    {

        if (_saveCoroutine != null)
        {
            StopCoroutine(_saveCoroutine);
            _saveCoroutine = null;
        }

        TryDeleteSaveFile();
        SaveBaseListToJson(_bases, FileText);
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            Debug.Log("C=" + (_bases != null ? _bases.Count.ToString() : "null"));
            SaveBaseListToJson(_bases, FileText);
            yield return new WaitForSeconds(1f);
            
        }
    }
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

    public void SaveNow()
    {
        TryDeleteSaveFile();
        SaveBaseListToJson(_bases, FileText);
    }
    public void SaveBaseListToJson(BuildingsList baseList, string filename)
    {
        try
        {
            if (baseList == null) baseList = new BuildingsList();

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
    public BuildingsList LoadBaseListFromJson(string filename)
    {
        string filePath = Path.Combine(Application.persistentDataPath, filename + FileExtension);
        if (File.Exists(filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var wrapper = JsonUtility.FromJson<BaseListWrapper>(jsonString);
                var result = new BuildingsList();
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
                return new BuildingsList();
            }
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
            return new BuildingsList();
        }
    }

    [Serializable]
    private class BaseListWrapper
    {
        public BuildingsSaveData[] items;
    }

    public BuildingsList GetBases() => _bases;
    public void SetBases(BuildingsList list) => _bases = list ?? new BuildingsList();

    public void AddBase(BuildingsSaveData element)
    {
        if (_bases == null) _bases = new BuildingsList();
        _bases.Add(element);
    }

    public bool RemoveBaseById(string id)
    {
        if (_bases == null) return false;
        var idx = _bases.FindIndex(b => b.ID == id);
        if (idx >= 0)
        {
            _bases.RemoveAt(idx);
            return true;
        }
        return false;
    }
}