using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;



namespace FinalProject
{
    // SceneManager helps in bundling up multiple sprites for various scenes in the game.
    // One SceneManager class can initialize, draw, and update all sprites inside an instance of it.
    class SceneManager
    {
        // < FIELDS > -----------------------------------------------------------------------
        // static (global) fields
        static int _WINWIDTH;
        static int _WINHEIGHT;
        static int _SCENEWIDTH; // width of the scene in pixels, including sidescrolling areas
        static int _SCENEHEIGHT; // height of the scene in pixels, including sidescrolling areas

        // constant sprite size
        const int _spriteWidth = 64; // width of each sprite
        const int _spriteHeight = 64; // height of each sprite
        int currentLevel = 1;

        const string _sceneLayout = "                                        " +  // x    is trap
                                    "                                        " +  // ---- is walkable platform
                                    "     -------               -------      " +  // o    is pickable coin
                                    "                                        " +
                                    "                                        " +  // z    is moving walkable platform
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

        // Scene object fields
        Sprite _background;
        Sprite[] _platform;
        Player _player;

        private Matrix _cameraTransform;    // TEST

        // < CONSTRUCTOR > -----------------------------------------------------------------
        public SceneManager(ContentManager Content)
        {
            ///// SPRITES //////

            // -- Background --
            Texture2D bgTexture = Content.Load<Texture2D>("Game Background 6"); // Load the background texture
            _background = new Sprite(
                bgTexture,
                new Rectangle(0, 0, WINWIDTH*2, WINHEIGHT*2),   // background cover the entire window
                new Rectangle(0, 0, bgTexture.Width, bgTexture.Height), // source is the entire texture
                Color.White
            );

            // -- Platform --
            Texture2D grassPlatform = Content.Load<Texture2D>("Platform 9");

            // -- Spike --
            Texture2D spike = Content.Load<Texture2D>("spike_trap_full");

            _platform = new Sprite[_sceneLayout.Length];

            for (int i = 0; i < _sceneLayout.Length; i++)
            {
                char tile = _sceneLayout[i];
                int x = ( i % 40 ) * _spriteWidth;  // 20 is the "width" of each row in _sceneLayout
                int y = ( i / 40 ) * _spriteHeight;
                Rectangle destRect = new Rectangle(x, y, _spriteWidth, _spriteHeight);
                Rectangle sourceRectangle;
                switch (tile)
                {
                    case '-': // walkable platform
                        sourceRectangle = new Rectangle(0, 0, grassPlatform.Width / 6, grassPlatform.Height);
                        _platform[i] = new Sprite(grassPlatform, destRect, sourceRectangle, Color.White);
                        break;
                    case 'z':
                        sourceRectangle = new Rectangle(0, 0, grassPlatform.Width / 6, grassPlatform.Height);
                        _platform[i] = new Sprite(grassPlatform, destRect, sourceRectangle, Color.White);
                        break;
                    //case 'o': // coin
                    //    sourceRectangle = new Rectangle(0, 0, coin.Width, coin.Height/4);
                    //    _platform[i] = new Coin(coin, destRect, sourceRectangle, Color.White);
                    //    break;
                    case 'x': // trap
                        sourceRectangle = new Rectangle(( spike.Width / 8 ) * 3, 0, spike.Width / 8, spike.Height);
                        _platform[i] = new Spike(spike, destRect, sourceRectangle, Color.White);
                        break;
                    default:
                        _platform[i] = null; // empty space
                        break;
                }
            }

            // --- Player ---- //
            Texture2D plyrTexture = Content.Load<Texture2D>("PlayerSprites");
            Rectangle plyrDest = new Rectangle(
                _spriteWidth * 11,       // X pos
                WINHEIGHT - ( _spriteHeight * 7 ),  // Y pos
                _spriteWidth * 2,
                _spriteHeight * 2
            );
            Rectangle plyrSource = new Rectangle(0, 0, plyrTexture.Width / 4, plyrTexture.Height / 7);
            _player = new Player(plyrTexture, plyrDest, plyrSource, Color.White);
        }

        // < PROPERTIES > -----------------------------------------------------------------
        public static int WINWIDTH { get => _WINWIDTH; set => _WINWIDTH = value; }
        public static int WINHEIGHT { get => _WINHEIGHT; set => _WINHEIGHT = value; }
        public static int SCENEWIDTH { get => _SCENEWIDTH; set => _SCENEWIDTH = value; }
        public static int SCENEHEIGHT { get => _SCENEHEIGHT; set => _SCENEHEIGHT =  value ; }


        // < METHODS > ---------------------------------------------------------------------

        private void UpdateCamera( )
        {
            Vector2 playerPos = new Vector2(_player.Destination.X, _player.Destination.Y);
            Vector2 cameraPosition = playerPos - new Vector2(WINWIDTH / 2, WINHEIGHT / 2);

            int SceneHeightBounds = _spriteHeight * 6; // == 384

            // clamp the position of the camera when side scrolling
            cameraPosition.X = MathHelper.Clamp(cameraPosition.X, 0, SceneManager.SCENEWIDTH - WINWIDTH);
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, -SceneHeightBounds, SceneHeightBounds-(_spriteHeight*3));

            _cameraTransform = Matrix.CreateTranslation(new Vector3(-cameraPosition, 0));
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: _cameraTransform);

            // Draw the background sprite
            spriteBatch.Draw(
                _background.Texture,
                _background.Destination,
                _background.Color
            );

            // Draw the platform sprites
            foreach (Sprite platform in _platform)
            {
                if (platform == null) continue;
                spriteBatch.Draw(
                    platform.Texture,
                    platform.Destination,
                    platform.Source,
                    platform.Color
                );

            }

            // Draw player with direction flip based on horizontal velocity
            SpriteEffects flip = _player.Direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(_player.Texture, _player.Destination, _player.Source, _player.Color, 0f, Vector2.Zero, flip, 0f);
            spriteBatch.End( );

        }


        public void Update(GameTime gameTime)
        {
            // Update everything related to the player using one method, Update(Sprite[], GameTime)
            _player.Update(_platform, gameTime);

            // update all updatable tiles in the scene
            for (int i = 0; i < _platform.Length; i++)
            {
                Sprite sprite = _platform[i];
                if (sprite == null) continue;

                // move the moveable tile
                if (_sceneLayout[i] == 'z')
                {
                    sprite.Move();
                }
            }

            UpdateCamera( );    /// TEST

        }
    }
}
