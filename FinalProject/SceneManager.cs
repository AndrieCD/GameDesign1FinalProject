using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace FinalProject
{
    // The SceneManager class is responsible for setting up, updating, and drawing everything in a game scene.
    // It manages the background, platforms, player, and camera movement for side-scrolling.
    class SceneManager
    {
        // < FIELDS > -----------------------------------------------------------------------
        // Static fields store window and scene dimensions so they can be accessed from anywhere.
        static int _WINWIDTH;      // Width of the game window
        static int _WINHEIGHT;     // Height of the game window
        static int _SCENEWIDTH;    // Total width of the scene (can be larger than window for scrolling)
        static int _SCENEHEIGHT;   // Total height of the scene
        static ContentManager Content;

        // Constants for the size of each sprite (platform, player, etc.)
        const int _spriteWidth = 64;  // Width of a single sprite in pixels
        const int _spriteHeight = 64; // Height of a single sprite in pixels

        private int _currentLevel; // Tracks the current level (not used in this code, but could be for multiple levels)
        private Level[] _levels;
        private Character[] _enemies;
        // This string visually represents the layout of the level.
        // Each character stands for a different type of tile or object.
        // '-' = platform, 'z' = moving platform, 'x' = spike/trap, ' ' = empty space
        private string _sceneLayout;

        // Scene object fields
        Sprite _background;      // The background image
        Sprite[] _platform;      // Array of all platforms and traps in the scene
        Player _player;          // The player character

        Texture2D grassPlatform, spike;

        private Matrix _cameraTransform; // Used to move the camera for side-scrolling

        // < CONSTRUCTOR > -----------------------------------------------------------------
        // The constructor sets up the scene: loads textures, creates platforms, and places the player.
        public SceneManager()
        {
            _levels = InitializeLevels( );
            _currentLevel = 1;

            // -- Background --
            Texture2D bgTexture = Content.Load<Texture2D>("Game Background 6"); // Load the background image
            _background = new Sprite(
                bgTexture,
                new Rectangle(0, 0, WINWIDTH * 2, WINHEIGHT * 2),   // Make the background cover the whole window
                new Rectangle(0, 0, bgTexture.Width, bgTexture.Height), // Use the whole image
                Color.White
            );

            // -- Platform --
            grassPlatform = Content.Load<Texture2D>("Platform 9"); // Load platform texture

            // -- Spike --
            spike = Content.Load<Texture2D>("spike_trap_full"); // Load spike/trap texture
          
            // --- Player ---- //
            Texture2D plyrTexture = Content.Load<Texture2D>("PlayerSprites"); // Load player sprite sheet
            Rectangle plyrDest = new Rectangle(
                _spriteWidth * 11,       // X position (in pixels)
                WINHEIGHT - ( _spriteHeight * 7 ),  // Y position (in pixels)
                _spriteWidth * 2,        // Player is twice as wide as a platform
                _spriteHeight * 2        // Player is twice as tall as a platform
            );
            Rectangle plyrSource = new Rectangle(0, 0, plyrTexture.Width / 4, plyrTexture.Height / 7); // First frame of sprite sheet
            _player = new Player(plyrTexture, plyrDest, plyrSource, Color.White); // Create the player
        }

        // < PROPERTIES > -----------------------------------------------------------------
        // These properties allow other parts of the program to get or set the window and scene size.
        public static int WINWIDTH { get => _WINWIDTH; set => _WINWIDTH = value; }
        public static int WINHEIGHT { get => _WINHEIGHT; set => _WINHEIGHT = value; }
        public static int SCENEWIDTH { get => _SCENEWIDTH; set => _SCENEWIDTH = value; }
        public static int SCENEHEIGHT { get => _SCENEHEIGHT; set => _SCENEHEIGHT = value; }
        public static ContentManager CONTENT { get => Content; set => Content =  value ; }

        // < METHODS > ---------------------------------------------------------------------

        // This method updates the camera's position so it follows the player as they move.
        public void UpdateCamera( )
        {
            // Get the player's position
            Vector2 playerPos = new Vector2(_player.Destination.X, _player.Destination.Y);
            // Center the camera on the player
            Vector2 cameraPosition = playerPos - new Vector2(WINWIDTH / 2, WINHEIGHT / 2);

            int SceneHeightBounds = _spriteHeight * 6; // Used to limit vertical camera movement

            // Prevent the camera from moving outside the scene boundaries
            cameraPosition.X = MathHelper.Clamp(cameraPosition.X, 0, SceneManager.SCENEWIDTH - WINWIDTH);
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, -SceneHeightBounds, SceneHeightBounds - ( _spriteHeight * 3 ));

            // Create a transformation matrix to move everything on screen according to the camera
            _cameraTransform = Matrix.CreateTranslation(new Vector3(-cameraPosition, 0));
        }

        // This method draws everything in the scene: background, platforms, and player.
        public void Draw(SpriteBatch spriteBatch)
        {
            // Begin drawing with the camera transformation (for scrolling)
            spriteBatch.Begin(transformMatrix: _cameraTransform);

            // Draw the background (does not use a source rectangle, draws the whole image)
            spriteBatch.Draw(
                _background.Texture,
                _background.Destination,
                _background.Color
            );

            // Draw all platforms and traps
            foreach (Sprite platform in _platform)
            {
                if (platform == null) continue; // Skip empty spaces
                spriteBatch.Draw(
                    platform.Texture,
                    platform.Destination,
                    platform.Source,
                    platform.Color
                );
            }

            // Draw the player. Flip the sprite if moving left.
            SpriteEffects flip = _player.Direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(_player.Texture, _player.Destination, _player.Source, _player.Color, 0f, Vector2.Zero, flip, 0f);

            spriteBatch.End( ); // Finish drawing
        }

        // This method updates the player and all moving platforms, and updates the camera.
        public void Update(GameTime gameTime)
        {
            // Update the player (handles movement, input, collisions, etc.)
            _player.Update(_platform, gameTime);

            // Update all moving platforms (those marked with 'z' in the layout)
            for (int i = 0; i < _platform.Length; i++)
            {
                Sprite sprite = _platform[i];
                if (sprite == null) continue;

                // If this is a moving platform, call its Move() method
                if (_sceneLayout[i] == 'z')
                {
                    sprite.Move( );
                }
            }

            UpdateCamera( ); // Move the camera to follow the player

            if ( // check if enemies are 0 )
            {
                if (_currentLevel + 1 < _levels.Length)
                {
                    _currentLevel++;
                    _sceneLayout = Level.Layout;
                    CreatePlatforms( );
                    
                } else
                {
                    // Game finished (last level)
                    Debug.WriteLine("All levels complete!");
                }
            }
        }

        public void SpawnEnemies()
        {
            _enemies = new Character[_levels[_currentLevel].EnemyCount];

            for (int i = 0; i < _enemies.Length; i++)
            {
                Point pos = _levels[_currentLevel].EnemyPositions[i];

                Texture2D enemyTexture = Content.Load<Texture2D>("EnemySprite");
                Rectangle dest = new Rectangle((int)pos.X, (int)pos.Y, _spriteWidth * 2, _spriteHeight * 2);
                Rectangle source = new Rectangle(0, 0, enemyTexture.Width / 4, enemyTexture.Height); // Assuming sprite sheet
                _enemies[i] = new Enemy(enemyTexture, dest, source, Color.White); // Or any custom NPC subclass
            }
        }

        public Level[] InitializeLevels( )
        {
            Level level1 = new Level(2, new Point[] {
                                                    new Point(6 * _spriteWidth, 1 * _spriteHeight),
                                                    new Point(15 * _spriteWidth, WINHEIGHT - ( _spriteHeight * 7 ))
                                                });

            return new Level[] { level1 };
        }

        public void CreatePlatforms()
        {
            _platform = new Sprite[_sceneLayout.Length]; // Create an array for all platforms/traps

            // Loop through each character in the layout string to create the correct object at each spot
            for (int i = 0; i < _sceneLayout.Length; i++)
            {
                char tile = _sceneLayout[i];
                int x = ( i % 40 ) * _spriteWidth;  // Calculate X position based on column
                int y = ( i / 40 ) * _spriteHeight; // Calculate Y position based on row
                Rectangle destRect = new Rectangle(x, y, _spriteWidth, _spriteHeight);
                Rectangle sourceRectangle;
                switch (tile)
                {
                    case '-': // Normal platform
                        sourceRectangle = new Rectangle(0, 0, grassPlatform.Width / 6, grassPlatform.Height);
                        _platform[i] = new Sprite(grassPlatform, destRect, sourceRectangle, Color.White);
                        break;
                    case 'z': // Moving platform (uses same texture as normal platform)
                        sourceRectangle = new Rectangle(0, 0, grassPlatform.Width / 6, grassPlatform.Height);
                        _platform[i] = new Sprite(grassPlatform, destRect, sourceRectangle, Color.White);
                        break;
                    //case 'o': // coin (not implemented here)
                    //    sourceRectangle = new Rectangle(0, 0, coin.Width, coin.Height/4);
                    //    _platform[i] = new Coin(coin, destRect, sourceRectangle, Color.White);
                    //    break;
                    case 'x': // Spike/trap
                        sourceRectangle = new Rectangle(( spike.Width / 8 ) * 3, 0, spike.Width / 8, spike.Height);
                        _platform[i] = new Spike(spike, destRect, sourceRectangle, Color.White);
                        break;
                    default:
                        _platform[i] = null; // Empty space, no object
                        break;
                }
            }

        }

    }


    struct Level
    {
        public const string Layout =    "                                        " +
                                        "                                        " +
                                        "     -------               -------      " +
                                        "                                        " +
                                        "                                        " +
                                        "                                        " +
                                        "        zzz      -----       zzz        " +
                                        "                                        " +
                                        "                                        " +
                                        "        x                               " +
                                        "     -------      zzzz     -------      " +
                                        "                                        " +
                                        "                                        " +
                                        "             xxx                        " +
                                        "----------------------------------------";
        public int EnemyCount;
        public Point[] EnemyPositions;

        public Level(int enemyCount, Point[] enemyPositions)
        {
            EnemyCount = enemyCount;
            EnemyPositions = enemyPositions;
        }
    }

}
