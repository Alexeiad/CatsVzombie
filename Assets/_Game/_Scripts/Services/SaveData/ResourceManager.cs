
using UnityEngine;


[System.Serializable]
public class ResourceData
{
    public int food;
    public int water;
    public int materials;
    public int diamonds;
    public int catsCount;

    public ResourceData(int food, int water, int materials, int diamonds = 0, int catsCount = 0)
    {
        this.food = food;
        this.water = water;
        this.materials = materials;
        this.diamonds = diamonds;
        this.catsCount = catsCount;
    }
}

public class ResourceManager
{
    
    public int LevelFood;
    public int LevelWater;
    public int LevelMaterials;
    public int LevelDiamond;
    public int LevelCatsCount;


    public ResourceManager(int startFood, int startWater, int startMaterials)
    {
        _resourceData = new ResourceData(startFood, startWater, startMaterials);
    }

   

    // Свойства для доступа к ресурсам
    public int Food => _resourceData.food;
    public int Water => _resourceData.water;
    public int Materials => _resourceData.materials;
    public int Diamonds => _resourceData.diamonds;
    public int CatsCount => _resourceData.catsCount;

    // Методы для изменения ресурсов
    public void AddFood(int amount) => _resourceData.food += amount;
    public void AddWater(int amount) => _resourceData.water += amount;
    public void AddMaterials(int amount) => _resourceData.materials += amount;
    public void AddDiamonds(int amount) => _resourceData.diamonds += amount;
    public void SetCatsCount(int count) => _resourceData.catsCount = count;

    private ResourceData _resourceData;

    public void ClearData()
    {
        _resourceData = new ResourceData(10,10,10);
        SaveToJson();
    }



    // Сохранение и загрузка
    public void SaveToJson(string filename = "resources.json")
    {
        string json = JsonUtility.ToJson(_resourceData, true);
        System.IO.File.WriteAllText(GetFilePath(filename), json);

    }

    public void LoadFromJson(string filename = "resources.json")
    {
        string filePath = GetFilePath(filename);

        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            _resourceData = JsonUtility.FromJson<ResourceData>(json);
        }
        else
        {
            Debug.LogWarning($"Save file not found: {filePath}");
        }
    }

    private string GetFilePath(string filename)
    {
        return Application.persistentDataPath + "/" + filename;
    }

    // Для отладки
    public override string ToString()
    {
        return $"Food: {Food}, Water: {Water}, Materials: {Materials}, Diamonds: {Diamonds}, Cats: {CatsCount}";
    }
}


