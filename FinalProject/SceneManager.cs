using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FinalProject
{
    /// <summary>
    /// Manages the setup, update, and drawing of the game scene, including background, platforms, player, and camera.
    /// </summary>
    public class SceneManager
    {
        // --- Static Fields (Window/Scene Info) ---
        public static int WINWIDTH { get; set; }
        public static int WINHEIGHT { get; set; }
        public static int SCENEWIDTH { get; set; }
        public static int SCENEHEIGHT { get; set; }
        public static ContentManager CONTENT { get; set; }
        public static List<Enemy> Enemies { get; set; }

        public static GraphicsDevice graphicsDevice;
        public static Game1 _game { get; set; }
        public int CurrentLevel => _currentLevel;

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
        private Texture2D platformBlocks, spike, heart;
        private Matrix _cameraTransform;

        // --- Checkpoint ---
        private GameData _latestCheckpoint;
        private GameData _previousCheckpoint;


        // --- Constructor ---
        public SceneManager()
        {
            Debug.WriteLine($"{SCENEHEIGHT} | {SCENEWIDTH}");
            LoadTextures();
            InitializePlayer();
            _currentLevel = 0;
            _levels = SceneManager.InitializeLevels();
            _sceneLayout = Level.Layout;
            CreatePlatforms();
            SpawnEnemies();
        }

        public SceneManager(GameData loadedData)
        {
            LoadTextures();
            LoadPlayer(loadedData.Player);
            _currentLevel = loadedData.CurrentLevel;
            _levels = SceneManager.InitializeLevels();
            _sceneLayout = Level.Layout;
            CreatePlatforms();
            LoadEnemies(loadedData.Enemies);

        }

        // --- Scene Setup Methods ---

        private void LoadTextures()
        {
            platformBlocks = CONTENT.Load<Texture2D>("Blocks");
            spike = CONTENT.Load<Texture2D>("spike_trap_full");
            Texture2D bgTexture = CONTENT.Load<Texture2D>("GameBackground");
            _background = new Sprite(
                bgTexture,
                new Rectangle(0, 0, SCENEWIDTH, SCENEHEIGHT),
                new Rectangle(0, 0, bgTexture.Width, bgTexture.Height),
                Color.LightGray
            );
            heart = CONTENT.Load<Texture2D>("heart");
        }

        public void LoadEnemies(List<CharData> enemiesData)
        {
            Enemies = new List<Enemy>();

            for (int i = 0; i < enemiesData.Count; i++)
            {
                CharData enemyData = enemiesData[i];
                Point pos = new Point((int)enemyData.X, (int)enemyData.Y);
                Texture2D enemyTexture = CONTENT.Load<Texture2D>("EnemySprite");
                Rectangle dest = new Rectangle(pos.X, pos.Y, _spriteWidth * 2, _spriteHeight * 2);
                Rectangle source = new Rectangle(0, 0, enemyTexture.Width / 4, enemyTexture.Height / 7);
                Enemies.Add(new Enemy(enemyTexture, dest, source, Color.White, _player));
                Enemies[i].Health = enemyData.Health;
            }
        }

        private void LoadPlayer(CharData playerData)
        {
            Texture2D plyrTexture = CONTENT.Load<Texture2D>("PlayerSprites");
            Rectangle plyrDest = new Rectangle(
                (int)playerData.X,
                (int)playerData.Y,
                _spriteWidth * 2,
                _spriteHeight * 2
            );
            Rectangle plyrSource = new Rectangle(0, 0, plyrTexture.Width / 4, plyrTexture.Height / 7);
            _player = new Player(graphicsDevice, plyrTexture, plyrDest, plyrSource, Color.White);
            _player.Health = playerData.Health;
        }

        private void InitializePlayer()
        {
            Texture2D plyrTexture = CONTENT.Load<Texture2D>("PlayerSprites");
            Rectangle plyrDest = new Rectangle(
                _spriteWidth * 20,
                SCENEHEIGHT - (_spriteHeight * 10),
                _spriteWidth * 2,
                _spriteHeight * 2
            );
            Rectangle plyrSource = new Rectangle(0, 0, plyrTexture.Width / 4, plyrTexture.Height / 7);
            _player = new Player(graphicsDevice, plyrTexture, plyrDest, plyrSource, Color.White);
        }

        //public static Level[] InitializeLevels( ) => new Level[] { new Level(3), new Level(7) };
        public static Level[] InitializeLevels() => new Level[] { new Level(1), new Level(1) };

        public void CreatePlatforms()
        {
            _platform = new Sprite[_sceneLayout.Length];
            for (int i = 0; i < _sceneLayout.Length; i++)
            {
                char tile = _sceneLayout[i];
                int x = (i % 40) * _spriteWidth;
                int y = (i / 40) * _spriteHeight;
                Rectangle destRect = new Rectangle(x, y, _spriteWidth, _spriteHeight);
                Rectangle sourceRectangle;
                switch (tile)
                {
                    case '-':
                    case 'z':
                        sourceRectangle = new Rectangle(platformBlocks.Width / 4 * 0, 0, platformBlocks.Width / 4, platformBlocks.Height / 3);
                        _platform[i] = new Sprite(platformBlocks, new Rectangle(x, y, _spriteWidth, _spriteHeight / 2), sourceRectangle, Color.White);
                        break;
                    case 'c':
                        sourceRectangle = new Rectangle(platformBlocks.Width / 4 * 2, 0, platformBlocks.Width / 4, platformBlocks.Height);
                        _platform[i] = new Sprite(platformBlocks, destRect, sourceRectangle, Color.White);
                        break;
                    case 'v':
                        sourceRectangle = new Rectangle(platformBlocks.Width / 4 * 3, 0, platformBlocks.Width / 4, platformBlocks.Height);
                        _platform[i] = new Sprite(platformBlocks, destRect, sourceRectangle, Color.White);
                        break;
                    case 'x':
                        sourceRectangle = new Rectangle((spike.Width / 8) * 3, 0, spike.Width / 8, spike.Height);
                        _platform[i] = new Spike(spike, destRect, sourceRectangle, Color.White);
                        break;
                    case 'o':
                        sourceRectangle = new Rectangle((heart.Width / 4) * 0, 0, heart.Width / 4, heart.Height);
                        _platform[i] = new Heart(heart, destRect, sourceRectangle, Color.White);
                        break;
                    default:
                        _platform[i] = null;
                        break;
                }
            }
        }

        public void SpawnEnemies()
        {
            Enemies = new List<Enemy>();
            List<Point> spawnPositions = new List<Point>();
            for (int i = 0; i < _sceneLayout.Length; i++)
            {
                if (_sceneLayout[i] == 'Y')
                {
                    int x = (i % 40) * _spriteWidth;
                    int y = (i / 40) * _spriteHeight;
                    spawnPositions.Add(new Point(x, y));
                }
            }

            Random rand = new Random();
            int spawnCount = Math.Min(_levels[_currentLevel].EnemyCount, spawnPositions.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int index = rand.Next(spawnPositions.Count);
                Point pos = spawnPositions[index];
                spawnPositions.RemoveAt(index);

                Texture2D enemyTexture = CONTENT.Load<Texture2D>("EnemySprite");
                Rectangle dest = new Rectangle(pos.X, pos.Y, _spriteWidth * 2, _spriteHeight * 2);
                Rectangle source = new Rectangle(0, 0, enemyTexture.Width / 4, enemyTexture.Height / 7);
                Enemies.Add(new Enemy(enemyTexture, dest, source, Color.White, _player));
            }
        }

        // --- Update & Draw Methods ---

        public void Update(GameTime gameTime)
        {
            // Checkpoint condition
            bool healthCheckpoint1 = _player.Health == 100;
            bool healthCheckpoint2 = _player.Health > 40 && _player.Health < 50;

            if (healthCheckpoint1 || healthCheckpoint2)
            {
                if (_previousCheckpoint == null) _previousCheckpoint = _latestCheckpoint;

                // Save current progress to checkpoint
                _latestCheckpoint = new GameData
                {
                    Player = new CharData
                    {
                        Health = _player.Health,
                        X = _player.Position.X,
                        Y = _player.Position.Y
                    },
                    Enemies = Enemies.Select(enemy => new CharData
                    {
                        Health = enemy.Health,
                        X = enemy.Position.X,
                        Y = enemy.Position.Y
                    }).ToList(),
                    CurrentLevel = _currentLevel
                };
                SaveSystem.WriteToFile(_latestCheckpoint);
            }



            if (_player.IsDead) _game._gameState = GameState.GameOver;
            _player.Update(_platform, gameTime);

            // Update moving platforms
            for (int i = 0; i < _platform.Length; i++)
            {
                Sprite sprite = _platform[i];
                if (sprite == null) continue;

                // Animate hearts
                if (_sceneLayout[i] == 'o')
                {
                    Heart heart = sprite as Heart;
                    if (heart.Collected) continue;
                    heart.Animate();
                }

                if (_sceneLayout[i] == 'z')
                    sprite.Move();
            }

            //Update enemies
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                if (Enemies[i].State == CharState.Dead && Enemies[i].IsDead)
                {
                    Enemies.RemoveAt(i);
                    continue;
                }

                Enemies[i].SetPlayerPosition(_player.Destination.Center.ToVector2());
                Enemies[i].Update(_platform, gameTime);
            }

            UpdateCamera();

            // Level progression
            if (Enemies.Count <= 0)
            {
                if (_currentLevel + 1 < _levels.Length)
                {
                    _currentLevel++;
                    _sceneLayout = Level.Layout;
                    //CreatePlatforms( );
                    SpawnEnemies();
                }
                else
                {
                    _game._gameState = GameState.Victory;
                }
            }


        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _cameraTransform);

            // Draw background
            spriteBatch.Draw(_background.Texture, _background.Destination, _background.Color);

            // Draw platforms
            for (int i = 0; i < _platform.Length; i++)
            {
                Sprite sprite = _platform[i];
                if (sprite == null) continue;
                if (_sceneLayout[i] == 'o')
                    if (sprite is Heart heart)
                        if (heart.Collected) continue;
                spriteBatch.Draw(sprite.Texture, sprite.Destination, sprite.Source, sprite.Color);

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

            spriteBatch.End();
        }

        // --- Camera ---

        public void UpdateCamera()
        {
            Vector2 playerPos = new Vector2(_player.Destination.X, _player.Destination.Y);
            Vector2 cameraPosition = playerPos - new Vector2(WINWIDTH / 2, WINHEIGHT / 2);

            cameraPosition.X = MathHelper.Clamp(cameraPosition.X, 0, SCENEWIDTH - WINWIDTH);
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, 0, SCENEHEIGHT / 2);
            _cameraTransform = Matrix.CreateTranslation(new Vector3(-cameraPosition, 0));
        }

        public int GetEnemyCount()
        {
            int enemyCount = 0;
            foreach (Enemy enemy in Enemies)
            {
                if (!enemy.IsDead)
                {
                    enemyCount++;
                }
            }
            return enemyCount;
        }
    }

    /// <summary>
    /// Level struct holds the layout and enemy count for each level.
    /// </summary>
    public struct Level
    {
        public const string Layout =
            "                                        " +    // Y for spawn points
            "         Y                     Y        " +    // - for thin platforms
            "             xx    o     xx             " +    // z for moving platforms
            "     -----------------------------      " +    // c for static sand
            "                                        " +    // o for heart
            "                                        " +    // x for spikes
            "                                        " +
            "----             vvvvvvv            ----" +
            "                    Y                   " +
            "                                        " +
            "                               o        " +
            "      zzz      vv  vv  vv      zzz      " +
            "                                        " +
            "                                        " +
            "   x                                x   " +
            "------    zzz     vvvvv    zzz    ------" +
            "                                        " +
            "                                        " +
            "      o       x         x               " +
            "     zzzz     --  ---- --     zzzz      " +
            "                                        " +
            "   Y                                Y   " +
            "ccc      x         xx         x      ccc" +
            "cccccccccccccccccccccccccccccccccccccccc";
        public int EnemyCount;

        public Level(int enemyCount)
        {
            EnemyCount = enemyCount;
        }
    }
}
