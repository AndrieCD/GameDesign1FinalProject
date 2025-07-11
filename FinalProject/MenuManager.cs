using FinalProject;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

public class MenuManager
{
    Game1 _game;
    SpriteFont _font;
    Texture2D _menuBg;

    public MenuManager(Game1 game)
    {
        _game = game;
        _font = game.Content.Load<SpriteFont>("LevelFont");
        _menuBg = game.Content.Load<Texture2D>("MenuBackground");
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
            _game._sceneManager = new SceneManager( ); // initialize sprites in SceneManager constructor

            _game._gameState = GameState.Playing;
        }
    }

    public void UpdatePauseMenu(GameTime gameTime)
    {
        KeyboardState ks = Keyboard.GetState( );

        if (ks.IsKeyDown(Keys.C))
            _game._gameState = GameState.Playing;

        if (ks.IsKeyDown(Keys.Q))
        {
            // Save logic here if needed
            _game._gameState = GameState.MainMenu;
        }
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
        sb.Begin( );
        sb.Draw(_menuBg, new Rectangle(0, 0, _game.Window.ClientBounds.Width, _game.Window.ClientBounds.Height), Color.White);
        sb.DrawString(_font, "Press ENTER to Start", new Vector2(300, 300), Color.White);
        sb.End( );
    }

    public void DrawPauseMenu(SpriteBatch sb)
    {
        sb.Begin( );
        sb.DrawString(_font, "Paused\nC - Continue\nQ - Save and Quit", new Vector2(280, 250), Color.Yellow);
        sb.End( );
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
