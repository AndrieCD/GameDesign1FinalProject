using FinalProject;
using System.Collections.Generic;

public class GameData
{
    public CharData Player;
    public List<CharData> Enemies = new List<CharData>( );
    public int CurrentLevel;
}

public class CharData
{
    public int Health;
    public float X, Y;
}
