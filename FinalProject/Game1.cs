using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;


namespace FinalProject;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    public static ContentManager content;
    public static SpriteFont LevelFont;
    public static GameTime _gameTime;
    private SpriteBatch _spriteBatch;

    // SceneManager will be used to create a scene (layout of sprites)
    public SceneManager _sceneManager; // helps in "bundling" drawing of multiple sprites together
    public MenuManager _menuManager;
    public GameState _gameState;

    bool hasPlayedEndMusic;

    public Game1( )
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280; // set the width of the window
        _graphics.PreferredBackBufferHeight = 768; // set the height of the window

        content = Content;

        this.Exiting += OnGameExiting;
    }

    protected override void Initialize( )
    {
        _gameState = GameState.MainMenu;
        _menuManager = new MenuManager(this);
        base.Initialize( );
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        LevelFont = Content.Load<SpriteFont>("LevelFont"); // Level Text

        SoundManager.LoadContent(Content); // Load all sound files
    }

    protected override void Update(GameTime gameTime)
    {

        KeyboardState ks = Keyboard.GetState( );

        switch (_gameState)
        {
            case GameState.MainMenu:
                hasPlayedEndMusic = false;
                SoundManager.PlayBackgroundMusic( ); // Start BGM
                _menuManager.UpdateMainMenu(gameTime);
                break;

            case GameState.Playing:
                if (ks.IsKeyDown(Keys.Escape))
                    _gameState = GameState.Paused;
                _sceneManager.Update(gameTime);
                break;

            case GameState.Paused:
                _menuManager.UpdatePauseMenu(gameTime);
                break;

            case GameState.GameOver:
                if (!hasPlayedEndMusic)
                {
                    SoundManager.PlayGameOverMusic( );
                    hasPlayedEndMusic = true;
                }
                _menuManager.UpdateGameOverMenu(gameTime);
                break;

            case GameState.Victory:
                if (!hasPlayedEndMusic)
                {
                    SoundManager.PlayVictoryMusic( );
                    hasPlayedEndMusic = true;
                }

                _menuManager.UpdateVictoryMenu(gameTime);
                break;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        switch (_gameState)
        {
            case GameState.MainMenu:
                _menuManager.DrawMainMenu(_spriteBatch);
                break;

            case GameState.Playing:
                _sceneManager.Draw(_spriteBatch);
                _sceneManager.Player.DrawHUD(_spriteBatch, _sceneManager.CurrentLevel, _sceneManager.GetEnemyCount()); //HUD
                break;

            case GameState.Paused:
                _sceneManager.Draw(_spriteBatch); // Draw game under pause
                _menuManager.DrawPauseMenu(_spriteBatch);
                break;

            case GameState.GameOver:
                _menuManager.DrawGameOverMenu(_spriteBatch);
                break;

            case GameState.Victory:
                _menuManager.DrawVictoryMenu(_spriteBatch);
                break;
        }



        base.Draw(gameTime);
    }

    private void OnGameExiting(object sender, EventArgs e)
    {
        // Only save if currently playing
        if (_gameState == GameState.Playing && _sceneManager != null)
        {
            SaveSystem.SaveGame(_sceneManager);
        }
    }


    public void ResetGame( )
    {
        Initialize( );
    }
}
