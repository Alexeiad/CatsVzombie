using System.Collections.Generic;

public class BuildPlot
{
    // Максимально зданий одного уровня, которые могут стоять на квадрате
    private Dictionary<int, int> capacityByLevel = new Dictionary<int, int>()
    {
        {1, 4},
        {2, 2},
        {3, 1},
        {4, 1}  // При 4 уровне можно одно здание 4уровня + одно 1 уровня
    };

    private List<Building> _buildings = new List<Building>();

    // Проверка можно ли построить здание такого типа и уровня
    public bool CanBuild(int level)
    {
        int countSameLevel = 0;
        foreach (var b in _buildings)
        {
            if (b.Level == level) countSameLevel++;
        }

        if (level == 4)
        {
            // Если уже есть здание 4 уровня — нельзя ставить
            if (countSameLevel >= capacityByLevel[4]) return false;
            // Проверяем, есть ли здание 1 уровня — можно если меньше одного
            int countLevel1 = 0;
            foreach (var b in _buildings)
                if (b.Level == 1) countLevel1++;
            return countLevel1 < capacityByLevel[1];
        }
        else if (level == 1)
        {
            // Если уже есть здание 4 уровня — можно одно 4 + одно 1 тогда проверить условий
            bool hasLevel4 = _buildings.Exists(b => b.Level == 4);
            if (hasLevel4)
            {
                // Можно добавить только одно здание 1 уровня
                int countLevel1 = _buildings.FindAll(b => b.Level == 1).Count;
                return countLevel1 < 1;
            }
        }

        return countSameLevel < capacityByLevel[level];
    }

    // Добавление здания на участок после проверки
    public void AddBuilding(Building building)
    {
        if (!CanBuild(building.Level))
            throw new System.Exception($"Нельзя построить здание уровня {building.Level} на этом участке");

        _buildings.Add(building);
    }
}