using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    internal class UIButtonLayouts
    {
        public static List<UIButton> CreateMainMenuButtons(
            Texture2D continueTexture,
            Texture2D newGameTexture,
            Texture2D exitTexture,
            Game1 game)
        {
            return new List<UIButton>
            {
                new UIButton(continueTexture, new Vector2(195, 380), () => {
                    if (File.Exists("save.txt"))
                    {
                        // Load saved scene
                        GameData loadedData = SaveSystem.LoadGame();

                        // Setup static fields
                        SceneManager.WINWIDTH = game.Window.ClientBounds.Width;
                        SceneManager.WINHEIGHT = game.Window.ClientBounds.Height;
                        SceneManager.SCENEWIDTH = game.Window.ClientBounds.Width * 2;
                        SceneManager.SCENEHEIGHT = game.Window.ClientBounds.Height * 2;
                        SceneManager.CONTENT = game.Content;
                        SceneManager.graphicsDevice = game.GraphicsDevice;
                        SceneManager._game = game;

                        // Assign loaded scene to game
                        game._sceneManager = new SceneManager(loadedData);

                        // Resume playing
                        game._gameState = GameState.Playing;
                    }
                }),

                new UIButton(newGameTexture, new Vector2(150, 500), () => {
                    SceneManager.WINWIDTH = game.Window.ClientBounds.Width;
                    SceneManager.WINHEIGHT = game.Window.ClientBounds.Height;
                    SceneManager.SCENEWIDTH = game.Window.ClientBounds.Width * 2;
                    SceneManager.SCENEHEIGHT = game.Window.ClientBounds.Height * 2;
                    SceneManager.CONTENT = game.Content;
                    SceneManager.graphicsDevice = game.GraphicsDevice;
                    SceneManager._game = game;

                    game._sceneManager = new SceneManager(); // Create the scene
                    game._gameState = GameState.Playing;     // Switch to gameplay
                }),

                new UIButton(exitTexture, new Vector2(235, 620), () => {
                    game.Exit();
                }),
            };
        }
    }
}
