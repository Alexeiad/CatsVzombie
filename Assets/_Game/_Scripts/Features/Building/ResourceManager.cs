
public class ResourceManager
{
    public int Food { get; private set; }
    public int Water { get; private set; }
    public int Materials { get; private set; }

    public ResourceManager(int startFood, int startWater, int startMaterials)
    {
        Food = startFood;
        Water = startWater;
        Materials = startMaterials;
    }
    public bool HasEnoughResources(int food, int water, int materials)
    {
        return Food >= food && Water >= water && Materials >= materials;
    }

    public bool SpendResources(int food, int water, int materials)
    {
        if (!HasEnoughResources(food, water, materials)) return false;

        Food -= food;
        Water -= water;
        Materials -= materials;
        return true;
    }

    public void AddResources(int food, int water, int materials)
    {
        Food += food;
        Water += water;
        Materials += materials;
    }
}
