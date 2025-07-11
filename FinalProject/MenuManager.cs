using FinalProject;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static FinalProject.UIButton;

public class MenuManager
{
    Game1 _game;
    SpriteFont _font;
    Texture2D _menuBg, _gameOverBg, _victoryBg;
    Texture2D backMainTex;
    Color backMainCol;

    // Buttons
    List<UIButton> _mainMenuButtons;
    Texture2D _newGameButtonTexture, _continueButtonTexture, _exitButtonTexture;

    // Game name in main menu
    Texture2D _logTexture;

    List<UIButton> _pauseMenuButtons;
    Texture2D _resumeButtonTexture, _saveQuitButtonTexture;

    Texture2D _dimOverlay;

    Texture2D _pauseMenu;


    public MenuManager(Game1 game)
    {
        _game = game;
        _font = game.Content.Load<SpriteFont>("LevelFont");
        _menuBg = game.Content.Load<Texture2D>("MenuBackground");
        _gameOverBg = game.Content.Load<Texture2D>("GO_1");
        _victoryBg = game.Content.Load<Texture2D>("YW_1");

        backMainTex = game.Content.Load<Texture2D>("BackMain");
        backMainCol = Color.White;
        _logTexture = game.Content.Load<Texture2D>("LOGO"); // Logo texture

        _newGameButtonTexture = game.Content.Load<Texture2D>("NewGameButton");
        _continueButtonTexture = game.Content.Load<Texture2D>("ContinueButton");
        _exitButtonTexture = game.Content.Load<Texture2D>("ExitButton");

        _pauseMenu = game.Content.Load<Texture2D>("PAUSE");

        _mainMenuButtons = UIButtonLayouts.CreateMainMenuButtons(
        _continueButtonTexture,
        _newGameButtonTexture,
        _exitButtonTexture,
        _game);

        bool saveExists = System.IO.File.Exists("save.xml");
        _mainMenuButtons[0].IsEnabled = saveExists;

        foreach (var btn in _mainMenuButtons)
            btn.Scale = 3f;

        _resumeButtonTexture = game.Content.Load<Texture2D>("ResumeButton");
        _saveQuitButtonTexture = game.Content.Load<Texture2D>("SaveAndQuitButton");

        _pauseMenuButtons = new List<UIButton>
        {
            new UIButton(_resumeButtonTexture, new Vector2(500, 350), () => {
                _game._gameState = GameState.Playing;
            }),
            new UIButton(_saveQuitButtonTexture, new Vector2(410, 480), () => {
                SaveSystem.SaveGame(_game._sceneManager);
                _mainMenuButtons[0].IsEnabled = true; // to enable continue button when the game is saved
                _game._gameState = GameState.MainMenu;
            })
        };

        foreach (var btn in _pauseMenuButtons)
            btn.Scale = 3f;

        _dimOverlay = new Texture2D(game.GraphicsDevice, 1, 1);
        _dimOverlay.SetData(new[] { new Color(0, 0, 0, 150) });

    }

    public void UpdateMainMenu(GameTime gameTime)
    {
        if (Keyboard.GetState( ).IsKeyDown(Keys.Enter))
        {
            // Initialize the SceneManager
            SceneManager.WINWIDTH = _game.Window.ClientBounds.Width;
            SceneManager.WINHEIGHT = _game.Window.ClientBounds.Height;
            SceneManager.SCENEWIDTH = _game.Window.ClientBounds.Width * 2; // entire scene width is double the window width
            SceneManager.SCENEHEIGHT = _game.Window.ClientBounds.Height * 2; // entire scene height is double the window height
            SceneManager.CONTENT = _game.Content; // set the content manager for SceneManager
            SceneManager.graphicsDevice = _game.GraphicsDevice; // set the graphics device for SceneManager
            _game._sceneManager = new SceneManager(_game); // initialize sprites in SceneManager constructor

            _game._gameState = GameState.Playing;
        }
        foreach (var button in _mainMenuButtons)
            button.Update();
    }

    public void UpdatePauseMenu(GameTime gameTime)
    {
        foreach (var button in _pauseMenuButtons)
            button.Update();
    }

    public void UpdateGameOverMenu(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState( );

        if (new Rectangle(SceneManager.WINWIDTH / 4, SceneManager.WINHEIGHT - 250, 550, 80).Contains(mouseState.Position))
        {
            backMainCol = Color.Yellow;
            if (mouseState.LeftButton == ButtonState.Pressed)
                _game._gameState = GameState.MainMenu;
        } else backMainCol = Color.White;
    }

    public void UpdateVictoryMenu(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState( );

        if (new Rectangle(SceneManager.WINWIDTH / 4, SceneManager.WINHEIGHT - 250, 550, 80).Contains(mouseState.Position))
        {
            backMainCol = Color.Yellow;
            if (mouseState.LeftButton == ButtonState.Pressed)
                _game._gameState = GameState.MainMenu;
        } else backMainCol = Color.White;
    }

    public void DrawMainMenu(SpriteBatch sb)
    {
        sb.Begin();

        // Draw background
        sb.Draw(_menuBg, new Rectangle(0, 0, _game.Window.ClientBounds.Width, _game.Window.ClientBounds.Height), Color.White);

        float logoScale = 5.5f;
        int logoX = 30;
        int logoY = -90;
        int logoWidth = (int)(_logTexture.Width * logoScale);
        int logoHeight = (int)(_logTexture.Height * logoScale);
        Rectangle logPosition = new Rectangle(logoX, logoY, logoWidth, logoHeight);
        sb.Draw(_logTexture, logPosition, Color.White);

        // === Draw Buttons ===
        foreach (var button in _mainMenuButtons)
            button.Draw(sb);

        sb.End();
    }


    public void DrawPauseMenu(SpriteBatch sb)
    {
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        // Draw the dark overlay to dim the background
        sb.Draw(_dimOverlay, new Rectangle(0, 0, _game.Window.ClientBounds.Width, _game.Window.ClientBounds.Height), Color.White);

        // Draw pause logo centered
        float pauseScale = 0.5f;
        int pauseWidth = (int)(_pauseMenu.Width * pauseScale);
        int pauseHeight = (int)(_pauseMenu.Height * pauseScale);

        int pauseX = (_game.Window.ClientBounds.Width - pauseWidth) / 2;
        int pauseY = -100;

        Rectangle pausePosition = new Rectangle(pauseX, pauseY, pauseWidth, pauseHeight);
        sb.Draw(_pauseMenu, pausePosition, Color.White);

        // Draw the pause menu buttons (Resume, Save & Quit)
        foreach (var button in _pauseMenuButtons)
            button.Draw(sb);

        sb.End();
    }



    public void DrawGameOverMenu(SpriteBatch sb)
    {
        sb.Begin( );
        sb.Draw(_gameOverBg, new Rectangle(0, 0, _game.Window.ClientBounds.Width, _game.Window.ClientBounds.Height), Color.White);
        sb.Draw(backMainTex, new Rectangle(SceneManager.WINWIDTH / 4, SceneManager.WINHEIGHT - 250, 550, 80), backMainCol);
        sb.End( );
    }

    public void DrawVictoryMenu(SpriteBatch sb)
    {
        sb.Begin( );
        sb.Draw(_victoryBg, new Rectangle(0, 0, _game.Window.ClientBounds.Width, _game.Window.ClientBounds.Height), Color.White);
        sb.Draw(backMainTex, new Rectangle(SceneManager.WINWIDTH / 4, SceneManager.WINHEIGHT - 250, 550, 80), backMainCol);
        sb.End( );
    }
}
