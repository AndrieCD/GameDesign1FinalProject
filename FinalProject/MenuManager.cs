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
    Texture2D _menuBg;

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
        if (Keyboard.GetState( ).IsKeyDown(Keys.Enter))
            _game._gameState = GameState.MainMenu;
    }

    public void UpdateVictoryMenu(GameTime gameTime)
    {
        if (Keyboard.GetState( ).IsKeyDown(Keys.Enter))
            _game._gameState = GameState.MainMenu;
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
        sb.DrawString(_font, "Game Over\nPress ENTER to return to Menu", new Vector2(250, 300), Color.Red);
        sb.End( );
    }

    public void DrawVictoryMenu(SpriteBatch sb)
    {
        sb.Begin( );
        sb.DrawString(_font, "Victory!\nPress ENTER to return to Menu", new Vector2(250, 300), Color.Green);
        sb.End( );
    }
}
