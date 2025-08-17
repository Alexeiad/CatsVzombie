
public class Building
{
    public BuildingType Type { get; private set; }
    public int Level { get; private set; }  // Уровень: 1..4

    public int FoodCost { get; private set; }
    public int WaterCost { get; private set; }
    public int MaterialCost { get; private set; }

    public Building(BuildingType type, int level, int foodCost, int waterCost, int materialCost)
    {
        Type = type;
        Level = level;
        FoodCost = foodCost;
        WaterCost = waterCost;
        MaterialCost = materialCost;
    }

    // Можно добавить методы для апгрейда, получения производства и т.д.
}
