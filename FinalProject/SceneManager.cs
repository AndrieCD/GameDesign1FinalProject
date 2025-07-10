using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FinalProject
{
    /// <summary>
    /// Manages the setup, update, and drawing of the game scene, including background, platforms, player, and camera.
    /// </summary>
    class SceneManager
    {
        // --- Static Fields (Window/Scene Info) ---
        public static int WINWIDTH { get; set; }
        public static int WINHEIGHT { get; set; }
        public static int SCENEWIDTH { get; set; }
        public static int SCENEHEIGHT { get; set; }
        public static ContentManager CONTENT { get; set; }
        public static List<Enemy> Enemies { get; set; }
        public static GraphicsDevice graphicsDevice;
        public int CurrentLevel=> _currentLevel;

        public Player Player => _player; 

        // --- Constants ---
        private const int _spriteWidth = 64;
        private const int _spriteHeight = 64;

        // --- Scene State ---
        private int _currentLevel;
        private Level[] _levels;
        private string _sceneLayout;

        // --- Scene Objects ---
        private Sprite _background;
        private Sprite[] _platform;
        private Player _player;
        private Texture2D grassPlatform, spike;
        private Matrix _cameraTransform;

        // --- Constructor ---
        public SceneManager( )
        {
            LoadTextures( );
            InitializePlayer( );
            _currentLevel = 0;
            _levels = InitializeLevels( );
            _sceneLayout = Level.Layout;
            CreatePlatforms( );
            SpawnEnemies( );
        }

        // --- Scene Setup Methods ---

        private void LoadTextures( )
        {
            grassPlatform = CONTENT.Load<Texture2D>("Platform 9");
            spike = CONTENT.Load<Texture2D>("spike_trap_full");
            Texture2D bgTexture = CONTENT.Load<Texture2D>("Game Background 6");
            _background = new Sprite(
                bgTexture,
                new Rectangle(0, 0, WINWIDTH * 2, WINHEIGHT * 2),
                new Rectangle(0, 0, bgTexture.Width, bgTexture.Height),
                Color.White
            );
        }

        private void InitializePlayer( )
        {
            Texture2D plyrTexture = CONTENT.Load<Texture2D>("PlayerSprites");
            Rectangle plyrDest = new Rectangle(
                _spriteWidth * 11,
                WINHEIGHT - ( _spriteHeight * 7 ),
                _spriteWidth * 2,
                _spriteHeight * 2
            );
            Rectangle plyrSource = new Rectangle(0, 0, plyrTexture.Width / 4, plyrTexture.Height / 7);
            _player = new Player(graphicsDevice, plyrTexture, plyrDest, plyrSource, Color.White);
        }

        public Level[] InitializeLevels( ) => new Level[] { new Level(1), new Level(6) };

        public void CreatePlatforms( )
        {
            _platform = new Sprite[_sceneLayout.Length];
            for (int i = 0; i < _sceneLayout.Length; i++)
            {
                char tile = _sceneLayout[i];
                int x = ( i % 40 ) * _spriteWidth;
                int y = ( i / 40 ) * _spriteHeight;
                Rectangle destRect = new Rectangle(x, y, _spriteWidth, _spriteHeight);
                Rectangle sourceRectangle;
                switch (tile)
                {
                    case '-':
                    case 'z':
                        sourceRectangle = new Rectangle(0, 0, grassPlatform.Width / 6, grassPlatform.Height);
                        _platform[i] = new Sprite(grassPlatform, destRect, sourceRectangle, Color.White);
                        break;
                    case 'x':
                        sourceRectangle = new Rectangle(( spike.Width / 8 ) * 3, 0, spike.Width / 8, spike.Height);
                        _platform[i] = new Spike(spike, destRect, sourceRectangle, Color.White);
                        break;
                    default:
                        _platform[i] = null;
                        break;
                }
            }
        }

        public void SpawnEnemies( )
        {
            Enemies = new List<Enemy>( );
            List<Point> spawnPositions = new List<Point>( );
            for (int i = 0; i < _sceneLayout.Length; i++)
            {
                if (_sceneLayout[i] == 'Y')
                {
                    int x = ( i % 40 ) * _spriteWidth;
                    int y = ( i / 40 ) * _spriteHeight;
                    spawnPositions.Add(new Point(x, y));
                }
            }

            Random rand = new Random( );
            int spawnCount = Math.Min(_levels[_currentLevel].EnemyCount, spawnPositions.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int index = rand.Next(spawnPositions.Count);
                Point pos = spawnPositions[index];
                spawnPositions.RemoveAt(index);

                Texture2D enemyTexture = CONTENT.Load<Texture2D>("PlayerSprites");
                Rectangle dest = new Rectangle(pos.X, pos.Y, _spriteWidth * 2, _spriteHeight * 2);
                Rectangle source = new Rectangle(0, 0, enemyTexture.Width / 4, enemyTexture.Height / 7);
                Enemies.Add(new Enemy(enemyTexture, dest, source, Color.Gray, _player));
            }
        }

        // --- Update & Draw Methods ---

        public void Update(GameTime gameTime)
        {
            if (_player.Health <= 0)
            {
                
                //return; // Player is dead, no further action needed
            }
            _player.Update(_platform, gameTime);

            // Update moving platforms
            for (int i = 0; i < _platform.Length; i++)
            {
                Sprite sprite = _platform[i];
                if (sprite == null) continue;
                if (_sceneLayout[i] == 'z')
                    sprite.Move( );
            }

             //Update enemies
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                if (Enemies[i].State == CharState.Dead && Enemies[i].DeathTimer <= 0f)
                {
                    Enemies.RemoveAt(i);
                    continue;
                }

                Enemies[i].SetPlayerPosition(_player.Destination.Center.ToVector2( ));
                Enemies[i].Update(_platform, gameTime);
            }

            UpdateCamera( );

            // Level progression
            if (Enemies.Count <= 0)
            {
                if (_currentLevel + 1 < _levels.Length)
                {
                    _currentLevel++;
                    _sceneLayout = Level.Layout;
                    CreatePlatforms( );
                    SpawnEnemies( );
                } else
                {
                    Debug.WriteLine("Game Finished! All enemies defeated.");
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _cameraTransform);

            // Draw background
            spriteBatch.Draw(_background.Texture, _background.Destination, _background.Color);

            // Draw platforms
            foreach (Sprite platform in _platform)
            {
                if (platform == null) continue;
                spriteBatch.Draw(platform.Texture, platform.Destination, platform.Source, platform.Color);
            }

            // Draw player
            SpriteEffects flip = _player.Direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(_player.Texture, _player.Destination, _player.Source, _player.Color, 0f, Vector2.Zero, flip, 0f);

            // Draw enemies
            foreach (Enemy enemy in Enemies)
            {
                if (enemy.IsDead) continue;
                flip = enemy.Direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                spriteBatch.Draw(enemy.Texture, enemy.Destination, enemy.Source, enemy.Color, 0f, Vector2.Zero, flip, 0f);
            }

            spriteBatch.End( );
        }

        // --- Camera ---

        public void UpdateCamera( )
        {
            Vector2 playerPos = new Vector2(_player.Destination.X, _player.Destination.Y);
            Vector2 cameraPosition = playerPos - new Vector2(WINWIDTH / 2, WINHEIGHT / 2);
            int sceneHeightBounds = _spriteHeight * 6;

            cameraPosition.X = MathHelper.Clamp(cameraPosition.X, 0, SCENEWIDTH - WINWIDTH);
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, -sceneHeightBounds, sceneHeightBounds - ( _spriteHeight * 3 ));

            _cameraTransform = Matrix.CreateTranslation(new Vector3(-cameraPosition, 0));
        }
    }

    /// <summary>
    /// Level struct holds the layout and enemy count for each level.
    /// </summary>
    struct Level
    {
        public const string Layout =
            "         Y                     Y        " +
            "                                        " +
            "     -------               -------      " +
            "                                        " +
            "         Y        Y                     " +
            "                                        " +
            "        zzz      -----       zzz Y      " +
            "                                        " +
            "                                 -      " +
            "        x                        -      " +
            "     -------      zzzz     -------      " +
            "                         -              " +
            "-  Y          -          -         Y    " +
            "---           -          -              " +
            "----------------------------------------";
        public int EnemyCount;

        public Level(int enemyCount)
        {
            EnemyCount = enemyCount;
        }
    }
}
