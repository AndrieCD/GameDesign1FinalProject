using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{
    // The Player class represents the main controllable character in the game.
    // It inherits from Character, so it gets all movement, collision, and animation logic.
    public class Player : Character
    {
        // This constant controls how much faster the player moves when sprinting.
        private const float SPRINT_MULTIPLIER = 1.5f; // multiplier for sprinting speed

        // HUD
        private Texture2D _healthbarTexture;
        private Texture2D _healthbarBackground;
        private Texture2D _borderTexture;

        // < Constructor > ---------------------------------------
        // This constructor sets up the player with its texture, position, and color.
        // It also creates a HUD (Heads-Up Display) for the player.
        public Player(GraphicsDevice graphicsDevice, Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            HUD hUD = new HUD(this); // Create a HUD to display player info (like health).

            _healthbarTexture = new Texture2D(graphicsDevice, 1, 1);
            _healthbarTexture.SetData(new[] { Color.Green }); // Healthbar Color
            _healthbarBackground = new Texture2D(graphicsDevice, 1, 1);
            _healthbarBackground.SetData(new[] { Color.SaddleBrown }); // Bg healthbar Color
            _borderTexture = new Texture2D(graphicsDevice, 1, 1);
            _borderTexture.SetData(new[] { Color.SaddleBrown }); // Border Color
        }

        // < Methods > -------------------------------------
        // This method is called every frame to update the player's state.
        public override void Update(Sprite[] _platform, GameTime gameTime)
        {
            // 1. Handle damage/death
            if (_health <= 0 && _state != CharState.Dead)
            {
                ChangeState(CharState.Dead);
                Die( );
                return; // Prevent further updates if dead.
            }

            HandleHurtState(gameTime);
            HandleAttackState(gameTime);
            ChangePosition(_platform);
            GetInputs(gameTime);

            //// 2. Handle special states first
            //if (_state == CharState.Hurt)
            //{
            //    HandleHurtState(gameTime);
            //} else if (_state == CharState.Attacking)
            //{
            //    HandleAttackState(gameTime);
            //} else
            //{
            //    ChangePosition(_platform);
            //    GetInputs(gameTime);
            //}

            // Play animation at the end
            PlayAnimation(_state);
        }

        // This method resets the player to their original position and restores health.
        public override void Die( )
        {
            // Move player back to starting location.
            _destination.Location = _originalLocation;
            _velocity.Y = 0f; // Stop any vertical movement.
            _isGrounded = true; // Player is now on the ground.
            _health = 100; // Restore full health.
        }

        public void DrawHUD(SpriteBatch spriteBatch, int currentLevel)
        {
            // Fixed screen position (bottom left corner)
            Vector2 hudPosition = new Vector2(35, SceneManager.WINHEIGHT - 50);

            // Health bar background
            int barWidth = 300;
            int barHeight = 30;
            int borderThickness = 5;

            Rectangle bgRect = new Rectangle((int)hudPosition.X, (int)hudPosition.Y, barWidth, barHeight);
            Rectangle healthRect = new Rectangle((int)hudPosition.X, (int)hudPosition.Y, (int)(barWidth * (_health / 100f)), barHeight);
            // _health / 100f = to get the percentage, Multiply that by barWidth to shrink or grow the bar based on health.

            // Border rectangles
            Rectangle topBorder = new Rectangle(bgRect.X - borderThickness, bgRect.Y - borderThickness, bgRect.Width + 2 * borderThickness, borderThickness);
            Rectangle bottomBorder = new Rectangle(bgRect.X - borderThickness, bgRect.Y + bgRect.Height, bgRect.Width + 2 * borderThickness, borderThickness);
            Rectangle leftBorder = new Rectangle(bgRect.X - borderThickness, bgRect.Y, borderThickness, bgRect.Height);
            Rectangle rightBorder = new Rectangle(bgRect.X + bgRect.Width, bgRect.Y, borderThickness, bgRect.Height);

            spriteBatch.Begin();

            // Draw border
            spriteBatch.Draw(_borderTexture, topBorder, Color.White);
            spriteBatch.Draw(_borderTexture, bottomBorder, Color.White);
            spriteBatch.Draw(_borderTexture, leftBorder, Color.White);
            spriteBatch.Draw(_borderTexture, rightBorder, Color.White);

            // Draw health bar background and fill
            spriteBatch.Draw(_healthbarBackground, bgRect, Color.White);
            spriteBatch.Draw(_healthbarTexture, healthRect, Color.White);

            // Draw Level Text - top center
            string levelText = $"Level: {currentLevel}";
            Vector2 levelTextSize = Game1.LevelFont.MeasureString(levelText);

            // Centered on top middle of screen
            Vector2 levelTextPos = new Vector2(
                (SceneManager.WINWIDTH / 2f) - (levelTextSize.X / 2f), // center horizontally
                15 // slight padding from the top
            );
            spriteBatch.DrawString(Game1.LevelFont, levelText, levelTextPos, Color.White);

            spriteBatch.End();
        }



        // This method checks for keyboard and mouse input and updates the player's movement and state.
        public void GetInputs(GameTime gameTime)
        {
            //--------------------INPUT MANAGER--------------------------------//

            // Get the current state of the keyboard and mouse.
            KeyboardState keyboardState = Keyboard.GetState( );
            MouseState mouseState = Mouse.GetState( );


            // Handle left and right movement.
            if (keyboardState.IsKeyDown(Keys.A))
            {
                _velocity.X = -SPEED; // Move left.
                _direction = -1; // Face left.
            } else if (keyboardState.IsKeyDown(Keys.D))
            {
                _velocity.X = SPEED; // Move right.
                _direction = 1; // Face right.
            } else
            {
                _velocity.X = 0f; // No horizontal movement.
            }

            // If the player holds the shift key, increase speed for sprinting.
            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                _velocity.X *= SPRINT_MULTIPLIER; // Move faster.
            }

            if (mouseState.LeftButton == ButtonState.Pressed && !_attacking)
            {
                _attacking = true;
                _attackTimer = 0.5f;
                ChangeState(CharState.Attacking);
                Debug.WriteLine("Player is attacking!"); // Log attack action for debugging.
            }


            // Handle jumping: Space or W key, and only if on the ground.
            if (( keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ) && _isGrounded)
            {
                _velocity.Y = -JUMP_POWER; // Move up (jump).
                _isGrounded = false; // Player is now in the air.
            }

            //------------------------STATE MANAGER-------------------------------//

            // Set the player's state based on movement and whether they're on the ground.
            if (!_attacking && !_isHurt)
            {
                if (_isGrounded)
                {
                    if (_velocity.X == 0f)
                    {
                        ChangeState(CharState.Idle); // Not moving.
                    } else
                    {
                        if (keyboardState.IsKeyDown(Keys.LeftShift))
                        {
                            ChangeState(CharState.Sprinting); // Moving fast.
                        } else
                        {
                            ChangeState(CharState.Walking); // Moving at normal speed.
                        }
                    }
                } else
                {
                    if (_velocity.Y < 0f)
                    {
                        ChangeState(CharState.Jumping); // Moving up.
                    } else if (_velocity.Y > 0f)
                    {
                        ChangeState(CharState.Falling); // Moving down.
                    }
                }
            }

            // Print the current state and velocity to the debug output (for developers).
            Debug.WriteLine($"State: {_state}   Velocity: {_velocity}   Direction: {_direction}");
        }

        // This method selects and displays the correct animation frame based on the player's state.
        public override void PlayAnimation(CharState state)
        {
            int framesPerRow = 4; // Number of frames per row in the sprite sheet.
            int startFrame, endFrame;
            int speed = 7;  // Default animation speed (delay)

            // Choose which frames to use based on the player's state.
            switch (state)
            {
                case CharState.Idle:
                    startFrame = 0;  // Start at frame 0.
                    endFrame = 2;    // End at frame 2.
                    break;

                case CharState.Jumping:
                    startFrame = 4;
                    endFrame = 7;
                    break;

                case CharState.Falling:
                    startFrame = 7;
                    endFrame = 7;
                    break;

                case CharState.Walking:
                    startFrame = 8;  // Start at frame 8.
                    endFrame = 11;
                    break;

                case CharState.Sprinting:
                    startFrame = 12;
                    endFrame = 14;
                    break;

                case CharState.Hurt:
                    startFrame = 16;
                    endFrame = 18;
                    ChangeColor(Color.Red); // Change color to red when hurt.
                    break;

                case CharState.Attacking:
                    startFrame = 24;
                    endFrame = 26;
                    speed = 14; // Faster animation for attacking.
                    break;

                default:
                    startFrame = 0;
                    endFrame = 0;
                    break;
            }

            // Only update the animation frame if enough time has passed.
            if (frameCounter > speed)
            {
                // Calculate which frame to show.
                int totalFrames = endFrame - startFrame + 1; // How many frames in this animation.
                int currentIndex = ( frameCounter / speed ) % totalFrames; // Which frame to show now.
                int frameNumber = startFrame + currentIndex; // The actual frame number in the sprite sheet.

                // Calculate the X and Y position of the frame in the sprite sheet.
                int frameX = ( frameNumber % framesPerRow ) * _frameWidth;
                int frameY = ( frameNumber / framesPerRow ) * _frameHeight;

                // Set the source rectangle to the correct frame.
                _source = new Rectangle(new Point(frameX, frameY), new Point(_frameWidth, _frameHeight));
            }

            frameCounter++; // Move to the next frame for next time.
        }
    }
}
