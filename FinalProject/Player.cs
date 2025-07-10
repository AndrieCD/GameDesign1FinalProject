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
        private const float SPRINT_MULTIPLIER = 2f; // Multiplier for sprinting speed

        // --- Constructor ---
        public Player(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            _health = 200;
            _ = new HUD(this); // Create a HUD to display player info (like health)
        }

        // --- Public Methods ---

        /// <summary>
        /// Updates the player's state, handles input, and manages animation.
        /// </summary>
        public override void Update(Sprite[] platforms, GameTime gameTime)
        {
            if (_health <= 0 && _state != CharState.Dead)
            {
                _deathTimer = 1f;
                ChangeState(CharState.Dead);
                Die(gameTime);
                return;
            }

            HandleHurtState(gameTime);
            HandleAttackState(gameTime);
            ChangePosition(platforms);
            HandleInput(gameTime);

            PlayAnimation(_state);
        }

        /// <summary>
        /// Handles the player's death animation and respawn.
        /// </summary>
        public override void Die(GameTime gameTime)
        {
            _deathTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_deathTimer <= 0f)
            {
                _destination.Location = _originalLocation;
                _velocity.Y = 0f;
                _isGrounded = true;
                _health = 100;
            }
        }

        /// <summary>
        /// Handles keyboard and mouse input, updates movement and state.
        /// </summary>
        private void HandleInput(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState( );
            MouseState mouseState = Mouse.GetState( );

            if (!_attacking && _health > 0)
            {
                HandleMovementInput(keyboardState);
                HandleJumpInput(keyboardState);
            }

            if (_health > 0 && mouseState.LeftButton == ButtonState.Pressed && !_attacking && _attackCD <= 0f)
            {
                HandleAttackInput(mouseState, gameTime);
            }


            UpdateStateBasedOnInput(keyboardState);
            _attackCD -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCD < 0f) _attackCD = 0f;
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
                hitBox.Location = new Point(hitBox.X + ( hitBox.Width / 2 ) * _direction, hitBox.Y);
                hitBox.Width = hitBox.Width / 2; 
                foreach (Character enemy in SceneManager.Enemies)
                {
                    if (hitBox.Intersects(enemy.Destination))
                    {
                        enemy.TakeDamage(20);
                        break;
                    }
                }
                ChangeState(CharState.Attacking);
            } else
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
            } else if (keyboardState.IsKeyDown(Keys.D))
            {
                _velocity.X = SPEED;
                _direction = 1;
            } else
            {
                _velocity.X = 0f;
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                _velocity.X *= SPRINT_MULTIPLIER;
            }
        }

        private void HandleJumpInput(KeyboardState keyboardState)
        {
            if (( keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ) && _isGrounded)
            {
                _velocity.Y = -JUMP_POWER;
                _isGrounded = false;
            }
        }

        private void HandleAttackInput(MouseState mouseState, GameTime gameTime)
        {
            _attacking = true;
            _attackTimer = 0.25f;
            _attackCD = 0.5f; // Set cooldown here, only when attack is triggered
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
                    } else
                    {
                        ChangeState(keyboardState.IsKeyDown(Keys.LeftShift) ? CharState.Sprinting : CharState.Walking);
                    }
                } else
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
