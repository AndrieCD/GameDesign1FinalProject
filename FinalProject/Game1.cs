using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProject;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // SceneManager will be used to create a scene (layout of sprites)
    private SceneManager _sceneManager; // helps in "bundling" drawing of multiple sprites together


    public Game1( )
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280; // set the width of the window
        _graphics.PreferredBackBufferHeight = 768; // set the height of the window
    }

    protected override void Initialize( )
    {
        // Initialize the SceneManager
        SceneManager.WINWIDTH = Window.ClientBounds.Width;
        SceneManager.WINHEIGHT = Window.ClientBounds.Height;
        SceneManager.SCENEWIDTH = Window.ClientBounds.Width * 2; // entire scene width is double the window width
        SceneManager.SCENEHEIGHT = Window.ClientBounds.Height * 2; // entire scene height is double the window height
        SceneManager.CONTENT = Content; // set the content manager for SceneManager

        _sceneManager = new SceneManager(); // initialize sprites in SceneManager constructor

        base.Initialize( );
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

    }

    protected override void Update(GameTime gameTime)
    {

        _sceneManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _sceneManager.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}
