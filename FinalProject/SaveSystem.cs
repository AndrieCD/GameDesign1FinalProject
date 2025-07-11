using FinalProject;
using System.IO;
using System.Xml.Serialization;

public static class SaveSystem
{
    public static void SaveGame(SceneManager sceneManager)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SceneManager));
        using (FileStream stream = new FileStream("save.xml", FileMode.Create))
        {
            serializer.Serialize(stream, sceneManager);
        }
    }

    public static SceneManager LoadGame()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SceneManager));
        using (FileStream stream = new FileStream("save.xml", FileMode.Open))
        {
            return (SceneManager)serializer.Deserialize(stream);
        }
    }
}
