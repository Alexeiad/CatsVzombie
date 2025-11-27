// Это наше "письмо" с данными
public class ObjectSelectedEvent
{
    public string ObjectName;
    public int ObjectId;

    public ObjectSelectedEvent(string name, int id)
    {
        ObjectName = name;
        ObjectId = id;
    }
}