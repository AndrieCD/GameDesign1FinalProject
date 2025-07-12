using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{
    /// <summary>
    /// The Player class represents the main controllable character in the game.
    /// Inherits from Character, so it gets all movement, collision, and animation logic.
    /// </summary>
    public class Player : Character
    {
        private const float SPRINT_MULTIPLIER = 2.5f; // Multiplier for sprinting speed

        // HUD
        private Texture2D _healthbarTexture;
        private Texture2D _healthbarBackground;
        private Texture2D _borderTexture;
        private float _healTimer;
        private static float _soundTimer = 0f;

        // Dash
        private float _dashCooldown = 3f; // Time before dash can be used again
        private float _dashCooldownTimer = 0f;
        private float _dashDistance = 50f; // How far the dash moves
        private float _dashDuration = 0.15f; // Duration of dash in seconds
        private float _dashTimer = 0f;
        private bool _isDashing = false;
        private bool _dashHitLanded = false;


        // < Constructor > ---------------------------------------
        // This constructor sets up the player with its texture, position, and color.
        // It also creates a HUD (Heads-Up Display) for the player.
        public Player(GraphicsDevice graphicsDevice, Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            _health = 100;
            _attackDamage = 20;
            _healTimer = 3f; // Timer for passive healing every 5 seconds
            _healthbarTexture = new Texture2D(graphicsDevice, 1, 1);
            _healthbarTexture.SetData(new[] { Color.Green }); // Healthbar Color
            _healthbarBackground = new Texture2D(graphicsDevice, 1, 1);
            _healthbarBackground.SetData(new[] { Color.SaddleBrown }); // Bg healthbar Color
            _borderTexture = new Texture2D(graphicsDevice, 1, 1);
            _borderTexture.SetData(new[] { Color.SaddleBrown }); // Border Color
        }

        // --- Public Methods ---

        /// <summary>
        /// Updates the player's state, handles input, and manages animation.
        /// </summary>
        public override void Update(Sprite[] platforms, GameTime gameTime)
        {
            base.Update(platforms, gameTime);
            if (_isDead) return;
            if (_health <= 0 && _state != CharState.Dead)
            {
                _deathTimer = 0.75f;
                ChangeState(CharState.Dead);
                SoundManager.PlayDeathSound();
            }
            HandleDeathState(gameTime);
            PassiveHeal(gameTime);
            HandleHurtState(gameTime);
            HandleAttackState(gameTime);
            ChangePosition(platforms);
            HandleInput(gameTime);
            Debug.WriteLine($"IsDead: {_isDead}, CharState: {_state}, DeathTimer {_deathTimer}");

            PlayAnimation(_state);

            _dashCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_isDashing)
            {
                // Check hitbox during dash
                Rectangle hitBox = GetCharacterBounds(Destination.Location);
                hitBox.Location = new Point(hitBox.X + (hitBox.Width / 2) * _direction, hitBox.Y);
                hitBox.Width = hitBox.Width / 2;

                foreach (Character enemy in SceneManager.Enemies)
                {
                    if (!_dashHitLanded && hitBox.Intersects(enemy.Destination))
                    {
                        enemy.TakeDamage((int)_attackDamage * 2); // Or use _attackDamage if you want consistent values
                        SoundManager.PlayHitSound();
                        _dashHitLanded = true;
                        break;
                    }
                }

                _dashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Disable dash after time expires
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    _velocity.X = 0f;
                    _dashHitLanded = false; // Reset dash hit detection
                }
            }
            if (_dashCooldownTimer < 0f) _dashCooldownTimer = 0f;
        }

        public void HealOnKill()
        {
            int healing = 25;
            _health += healing;
            _health = Math.Clamp(_health, 0, 100); // Ensure health does not exceed 100
        }

        private void PassiveHeal(GameTime gameTime)
        {
            if (_health < 100 && _health > 0 && !_isDead)
            {
                _healTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds; // Decrease timer by 1 second
                if (_healTimer <= 0f)
                {
                    _health += 10;
                    _health = Math.Clamp(_health, 0, 100);
                    _healTimer = 4f;
                }
            }
        }

       
        /// <summary>
        /// Handles the player's death animation and respawn.
        /// </summary>
        public override void HandleDeathState(GameTime gameTime)
        {
            _deathTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_deathTimer <= 0f && _state == CharState.Dead)
            {
                _isDead = true;
            }
        }

        public void DrawHUD(SpriteBatch spriteBatch, int currentLevel, int enemyCount)
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
            string levelText = $"Level: {currentLevel + 1}";
            Vector2 levelTextSize = Game1.LevelFont.MeasureString(levelText);

            // Centered on top middle of screen
            Vector2 levelTextPos = new Vector2(
                (SceneManager.WINWIDTH / 2f) - (levelTextSize.X / 2f), // center horizontally
                15 // slight padding from the top
            );
            spriteBatch.DrawString(Game1.LevelFont, levelText, levelTextPos, Color.White);

            // Remaining enemies
            string enemyText = $"Enemies Remaining: {enemyCount}";
            Vector2 enemyTextSize = Game1.LevelFont.MeasureString(enemyText);

            // Centered below the level text
            Vector2 enemyTextPos = new Vector2(
                (SceneManager.WINWIDTH / 2f) - (enemyTextSize.X / 2f), // center horizontally
                levelTextPos.Y + levelTextSize.Y + 5 // slight padding below level text
            );
            spriteBatch.DrawString(Game1.LevelFont, enemyText, enemyTextPos, Color.White);

            spriteBatch.End();
        }


        /// <summary>
        /// Handles keyboard and mouse input, updates movement and state.
        /// </summary>
        private void HandleInput(GameTime gameTime)
        {
            if (_isDashing) return; // Ignore movement input during dash

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (!_attacking && _health > 0)
            {
                HandleMovementInput(keyboardState);
                HandleJumpInput(keyboardState);
            }

            if (_health > 0 && mouseState.LeftButton == ButtonState.Pressed && !_attacking && _attackCD <= 0f)
            {
                HandleAttackInput(mouseState, gameTime);
            }

            if (_health > 0 && keyboardState.IsKeyDown(Keys.Q) && _dashCooldownTimer <= 0f)
            {
                PerformDash();
            }


            UpdateStateBasedOnInput(keyboardState);
            _attackCD -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCD < 0f) _attackCD = 0f;
        }

        private void PerformDash()
        {
            _isDashing = true;
            _dashTimer = _dashDuration;
            _velocity.X = _direction * _dashDistance; // High velocity for dash
            _dashCooldownTimer = _dashCooldown;
            SoundManager.PlaySwordSwing(); // or PlayDashSound if you have one
        }


        /// <summary>
        /// Handles the logic for when the player is attacking.
        /// </summary>
        public override void HandleAttackState(GameTime gameTime)
        {
            _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_attackTimer > 0f)
            {
                Rectangle hitBox = GetCharacterBounds(Destination.Location);
                hitBox.Location = new Point(hitBox.X + (hitBox.Width / 2) * _direction, hitBox.Y);
                hitBox.Width = hitBox.Width / 2;

                bool hitSomeone = false; // to detect if the enemy is hit

                foreach (Character enemy in SceneManager.Enemies)
                {
                    if (!_hitLandedThisAttack && hitBox.Intersects(enemy.Destination))
                    {
                        enemy.TakeDamage(20);
                        SoundManager.PlayHitSound();
                        _hitLandedThisAttack = true; // true if attack landed on enemy
                        hitSomeone = true; // enemy is hit = true
                        break;
                    }
                }

                // If no enemy was hit, play the sword swing
                // I added this kasi ung hit.wav may kasama siyang sword swing
                if (!hitSomeone && !_hitLandedThisAttack)
                {
                    SoundManager.PlaySwordSwing();
                    _hitLandedThisAttack = true; // set to true to not overlap sound effect
                }

                ChangeState(CharState.Attacking);
            }
            else
            {
                _attacking = false;
            }
        }

        // --- Private Input Helpers ---

        private void HandleMovementInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.A))
            {
                _velocity.X = -SPEED;
                _direction = -1;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                _velocity.X = SPEED;
                _direction = 1;
            }
            else
            {
                _velocity.X = 0f;
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                _velocity.X *= SPRINT_MULTIPLIER;
                if (_soundTimer > 0f)
                {
                    _soundTimer -= 0.025f; // Decrease the timer to prevent spamming the sound
                    return; // Exit if the timer is still active
                }
                if (_velocity.X == 0f) return; // Don't play sound if not moving
                if (!_isGrounded) return;
                _soundTimer = 0.4f; // Reset the sound timer
                SoundManager.PlayWalkSound(); // Play the walk sound

            }
            else
            {
                if (_soundTimer > 0f)
                {
                    _soundTimer -= 0.025f; // Decrease the timer to prevent spamming the sound
                    return; // Exit if the timer is still active
                }
                if (_velocity.X == 0f) return; // Don't play sound if not moving
                _soundTimer = 0.7f; // Reset the sound timer
                SoundManager.PlayWalkSound(); // Play the walk sound
            }
        }

        private void HandleJumpInput(KeyboardState keyboardState)
        {
            if ((keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W)) && _isGrounded)
            {
                _velocity.Y = -JUMP_POWER;
                _isGrounded = false;
                SoundManager.PlayJumpSound();
            }
        }

        private void HandleAttackInput(MouseState mouseState, GameTime gameTime)
        {
            _attacking = true;
            _attackTimer = 0.25f;
            _attackCD = 0.5f; // Set cooldown here, only when attack is triggered
            _hitLandedThisAttack = false; // resets to false on mouse click
            ChangeState(CharState.Attacking);
            Debug.WriteLine("Player is attacking!");
        }


        private void UpdateStateBasedOnInput(KeyboardState keyboardState)
        {
            if (!_attacking && !_isHurt && _health > 0)
            {
                if (_isGrounded)
                {
                    if (_velocity.X == 0f)
                    {
                        ChangeState(CharState.Idle);
                    }
                    else
                    {
                        ChangeState(keyboardState.IsKeyDown(Keys.LeftShift) ? CharState.Sprinting : CharState.Walking);
                    }
                }
                else
                {
                    if (_velocity.Y < 0f)
                        ChangeState(CharState.Jumping);
                    else if (_velocity.Y > 0f)
                        ChangeState(CharState.Falling);
                }
            }
        }
    }
}
