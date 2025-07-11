using FinalProject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1;
using System.IO;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class SaveSystem
{

    public static void SaveGame(SceneManager sceneManager)
    {
        GameData data = new GameData( );

        // Convert player to CharData
        data.Player = new CharData
        {
            Health = sceneManager.Player.Health,
            X = sceneManager.Player.Position.X,
            Y = sceneManager.Player.Position.Y
        };

        // Convert enemies to CharData list
        foreach (var enemy in SceneManager.Enemies)
        {
            data.Enemies.Add(new CharData
            {
                Health = enemy.Health,
                X = enemy.Position.X,
                Y = enemy.Position.Y
            });
        }

        data.CurrentLevel = sceneManager.CurrentLevel;

        // Save data to file
        XmlSerializer serializer = new XmlSerializer(typeof(GameData));
        using (StreamWriter stream = new StreamWriter("save.txt"))
        {
            serializer.Serialize(stream, data);
        }
    }

    public static void WriteToFile(GameData data)
    {
        // Save data to file
        XmlSerializer serializer = new XmlSerializer(typeof(GameData));
        using (StreamWriter stream = new StreamWriter("save.txt"))
        {
            serializer.Serialize(stream, data);
        }
    }


    public static GameData LoadGame()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(GameData));
        using (StreamReader stream = new StreamReader("save.txt"))
        {
            return (GameData)serializer.Deserialize(stream);
        }
    }


}
