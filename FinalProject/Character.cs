using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{
    // This enum defines all possible states a character can be in.
    public enum CharState { Idle = 1, Walking = 2, Jumping = 3, Falling = 4, Hurt = 5, Sprinting = 6, Dead = 7, Attacking = 8 };

    // The Character class is an abstract base class for all characters in the game (like players or enemies).
    // It inherits from the Sprite class, so it has all the properties and methods of a Sprite.
    public abstract class Character : Sprite
    {
        // < Fields > -----------------------------------------
        // These variables store the character's health, state, movement, and other important info.
        protected int _health; // The character's current health points.
        protected CharState _state; // The character's current state (idle, walking, etc.).
        protected CharState _previousState; // The character's previous state.
        protected Vector2 _velocity; // The character's current speed and direction.
        protected bool _isGrounded; // True if the character is standing on the ground.
        protected int frameCounter; // Used for animation frame timing.
        protected int _direction; // 1 for right, -1 for left (which way the character is facing).

        // Variables for handling when the character is hurt or attacking.
        protected bool _isHurt;
        protected float _hurtTimer;
        protected bool _attacking;
        protected float _attackTimer;
        protected const float HURT_DURATION = 0.25f; // How long the hurt state lasts (in seconds).

        // Constants for movement and collision.
        protected const int OFFSET = 50; // Used to adjust collision detection area.
        protected const float SPEED = 8f; // How fast the character moves.
        protected const float GRAVITY = 1f; // How fast the character falls.
        protected const float JUMP_POWER = 25f; // How high the character jumps.

        // These are used to help with sprite sheet animations.
        protected readonly int _frameWidth;  // Width of a single animation frame.
        protected readonly int _frameHeight; // Height of a single animation frame.

        // < Constructor > ---------------------------------------
        // This method runs when a new Character is created.
        // It sets up the character's starting values and calculates animation frame sizes.
        public Character(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            // Set default values for state and timers.
            _isHurt = false;
            _hurtTimer = 0f;
            _attacking = false;
            _attackTimer = 0f;

            _health = 100; // Start with full health.
            _state = CharState.Idle; // Start in idle state.
            _previousState = CharState.Idle;
            _velocity = Vector2.Zero; // Not moving at start.
            _isGrounded = true; // Start on the ground.
            _originalLocation = new Point(destination.X, destination.Y);

            // Set up animation frame sizes based on the texture.
            frameCounter = 0;
            _frameWidth = texture.Width / 4; // Assumes 4 columns in the sprite sheet.
            _frameHeight = texture.Height / 7; // Assumes 7 rows in the sprite sheet.

            _direction = 1; // Start facing right.
        }

        // < Properties > -----------------------------------------
        // These provide read-only access to some private fields.

        public Vector2 Velocity { get => _velocity; } // Get the current velocity.
        public int Direction { get => _direction; } // Get the current direction.
        protected int Health { get => _health; } // Get the current health.

        // < Methods > -------------------------------------
        // This method updates the character every frame.
        // It handles hurt state, movement, and checks if the character should die.
        public virtual void Update(Sprite[] _platform, GameTime gameTime)
        {
            
        }

        // This method must be implemented by subclasses to define what happens when the character dies.
        public abstract void Die( );

        // Changes the character's state to a new one.
        public void ChangeState(CharState newState)
        {
            if (_state == newState) return;
            _previousState = _state;
            _state = newState;
        }

        // Handles the logic for when the character is hurt.
        public void HandleHurtState(GameTime gameTime)
        {
            _hurtTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_hurtTimer > 0f)
            {
                ChangeState(CharState.Hurt);
                ChangeColor(Color.Red);
            } else
            {
                _isHurt = false;
                ChangeColor(Color.White);
                //ChangeState(CharState.Idle);
            }
        }


        // Handles the logic for when the character is attacking.
        public void HandleAttackState(GameTime gameTime)
        {
            _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_attackTimer > 0f)
            {
                ChangeState(CharState.Attacking);
            } else
            {
                _attacking = false;
                //ChangeState(CharState.Idle);
            }
        }


        // Moves the character and checks for collisions with platforms.
        public void ChangePosition(Sprite[] platforms)
        {
            if (!_isGrounded)
            {
                // If not on the ground, apply gravity to make the character fall.
                if (!_isGrounded) _velocity.Y += GRAVITY;
            }

            // Calculate the new position after moving and colliding.
            Point newPos = HandleCollisions(platforms);

            // Make sure the character stays within the screen boundaries.
            newPos.X = Math.Clamp(newPos.X, 0 - OFFSET, SceneManager.SCENEWIDTH - _destination.Width + OFFSET);
            //newPos.Y = Math.Clamp(newPos.Y, 0, SceneManager.WINHEIGHT + _destination.Height); // (Commented out) Would clamp Y position.

            _destination.Location = newPos; // Update the character's position.
        }

        // Returns a rectangle representing the character's collision area at a given position.
        protected Rectangle GetCharacterBounds(Point newPos)
        {
            return new Rectangle(newPos.X + OFFSET, newPos.Y + OFFSET, _destination.Width - ( 2 * OFFSET ), _destination.Height - OFFSET);
        }

        // Handles collisions with tiles (like platforms or spikes) and updates the character's position.
        protected Point HandleCollisions(Sprite[] tiles)
        {
            _isGrounded = false; // Assume not grounded until proven otherwise.
            _isHurt = false; // Reset hurt state (will be set if hit by a spike).

            // Start with the current position.
            Point newPos = new Point(_destination.X, _destination.Y);

            // --- Horizontal Collision ---
            newPos.X += (int)_velocity.X; // Move horizontally.
            Rectangle horizBounds = GetCharacterBounds(newPos);

            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite tile = tiles[i];
                if (tile == null) continue;

                if (horizBounds.Intersects(tile.Destination))
                {
                    // If the tile is a spike, hurt the character.
                    if (tile is Spike)
                    {
                        if (!_isHurt)
                        {
                            _isHurt = true;
                            _hurtTimer = HURT_DURATION;
                            _health -= Spike.Damage;
                            ChangeState(CharState.Hurt);
                        }

                        continue; // Don't block movement for spikes.
                    }

                    // If moving right, stop at the left edge of the tile.
                    if (_velocity.X > 0)
                    {
                        newPos.X = tile.Destination.Left - _destination.Width + OFFSET;
                    }
                    // If moving left, stop at the right edge of the tile.
                    else if (_velocity.X < 0)
                    {
                        newPos.X = tile.Destination.Right - OFFSET;
                    }
                }
            }

            // --- Vertical Collision ---
            newPos.Y += (int)_velocity.Y; // Move vertically.
            Rectangle vertBounds = GetCharacterBounds(newPos);

            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite tile = tiles[i];
                if (tile == null) continue;

                if (vertBounds.Intersects(tile.Destination))
                {
                    // If the tile is a spike, hurt the character.
                    if (tile is Spike)
                    {
                        if (!_isHurt)
                        {
                            _isHurt = true;
                            _hurtTimer = HURT_DURATION;
                            _health -= Spike.Damage;
                        }
                        continue; // Don't block movement for spikes.
                    }

                    // If falling down, land on top of the tile.
                    if (_velocity.Y > 0)
                    {
                        newPos.Y = tile.Destination.Top - _destination.Height;
                        _isGrounded = true; // Now on the ground.
                        _velocity.Y = 0; // Stop falling.
                    }
                    // If jumping up, stop at the bottom of the tile.
                    else if (_velocity.Y < 0)
                    {
                        newPos.Y = tile.Destination.Bottom;
                        _velocity.Y = 0; // Stop moving up.
                    }
                }
            }

            return newPos; // Return the new position after handling collisions.
        }

        // This method must be implemented by subclasses to play the correct animation for the current state.
        public abstract void PlayAnimation(CharState state);
    }
}
