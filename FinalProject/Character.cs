using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{
    public enum CharState { Idle = 1, Walking = 2, Jumping = 3, Falling = 4, Hurt = 5, Sprinting = 6, Dead = 7, Attacking = 8 };

    public abstract class Character : Sprite
    {
        // < Fields > -----------------------------------------
        protected int _health;
        protected CharState _state;
        protected CharState _previousState;
        protected Vector2 _velocity;
        protected bool _isGrounded;
        protected int frameCounter;
        protected int _direction;

        protected bool _isHurt;
        protected float _hurtTimer;
        protected bool _attacking;
        protected float _attackTimer;
        protected const float HURT_DURATION = 0.25f;

        protected const int OFFSET = 50; // offset for collision detection
        protected const float SPEED = 8f;
        protected const float GRAVITY = 1f;
        protected const float JUMP_POWER = 25f;

        protected readonly int _frameWidth;  // for sprite sheet calculations
        protected readonly int _frameHeight; // for sprite sheet calculations

        // < Constructor > ---------------------------------------
        public Character(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            // default state and fields
            _isHurt = false;
            _hurtTimer = 0f;
            _attacking = false;
            _attackTimer = 0f;

            _health = 100;
            _state = CharState.Idle;
            _previousState = CharState.Idle;
            _velocity = Vector2.Zero;
            _isGrounded = true; // Player starts on the ground
            _originalLocation = new Point(destination.X, destination.Y);

            // animation-related fields
            frameCounter = 0;
            _frameWidth = texture.Width / 4;
            _frameHeight = texture.Height / 7;

            _direction = 1; // 1 for right, -1 for left
        }

        // < Properties > -----------------------------------------
        public Vector2 Velocity { get => _velocity; }

        public int Direction { get => _direction; }
        protected int Health { get => _health; }

        // < Methods > -------------------------------------
        public virtual void Update(Sprite[] _platform, GameTime gameTime)
        {
            //GetInputs( );
            HandleHurtState(gameTime);
            ChangePosition(_platform);

            if (_health <= 0) Die( );
        }

        public abstract void Die( );

        public void ChangeState(CharState newState)
        {
            _state = newState;
        }

        public void HandleHurtState(GameTime gameTime)
        {
            if (_isHurt)
            {
                //_hurtTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _state = CharState.Hurt;
                //if (_hurtTimer <= 0)
                //{
                //    _isHurt = false;

                //}
            } else ChangeColor(Color.White);
        }

        public void HandleAttackState(GameTime gameTime)
        {
            if (_attacking)
            {
                _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _state = CharState.Attacking;
                if (_attackTimer <= 0)
                {
                    _attacking = false;
                }
            } else ChangeColor(Color.White);
        }

        public void ChangePosition(Sprite[] platforms)
        {
            if (!_isGrounded)
            {
                // Gravity (fall)
                if (!_isGrounded) _velocity.Y += GRAVITY; // apply gravity
            }

            Point newPos = HandleCollisions(platforms);

            // clamp new position within window bounds
            newPos.X = Math.Clamp(newPos.X, 0 - OFFSET, SceneManager.SCENEWIDTH - _destination.Width + OFFSET);
            //newPos.Y = Math.Clamp(newPos.Y, 0, SceneManager.WINHEIGHT + _destination.Height);
            _destination.Location = newPos;
        }

        protected Rectangle GetPlayerBounds(Point newPos)
        {
            return new Rectangle(newPos.X + OFFSET, newPos.Y + OFFSET, _destination.Width - ( 2 * OFFSET ), _destination.Height - OFFSET);
        }

        protected Point HandleCollisions(Sprite[] tiles)
        {
            _isGrounded = false;
            _isHurt = false;
            // start with the current position
            Point newPos = new Point(_destination.X, _destination.Y);

            // --- Horizontal Collision ---
            newPos.X += (int)_velocity.X;
            Rectangle horizBounds = GetPlayerBounds(newPos);

            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite tile = tiles[i];
                if (tile == null) continue;

                if (horizBounds.Intersects(tile.Destination))
                {
                    
                    // Spike logic
                    if (tile is Spike)
                    {
                        if (!_isHurt)
                        {
                            _isHurt = true;
                            _hurtTimer = HURT_DURATION;
                            _health -= Spike.Damage;
                        }

                        continue; // skip for spikes
                    }

                    //-----------------------------------------------------------------//

                    if (_velocity.X > 0) // Moving right
                    {
                        newPos.X = tile.Destination.Left - _destination.Width + OFFSET;
                    } else if (_velocity.X < 0) // Moving left
                    {
                        newPos.X = tile.Destination.Right - OFFSET;
                    }

                    // Update horizontal bounds after adjustments
                    //horizBounds = GetPlayerBounds(newPos);
                }
            }

            // --- Vertical Collision ---
            newPos.Y += (int)_velocity.Y;
            Rectangle vertBounds = GetPlayerBounds(newPos);

            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite tile = tiles[i];
                if (tile == null) continue;

                if (vertBounds.Intersects(tile.Destination))
                {
                    // Spike logic
                    if (tile is Spike)
                    {
                        if (!_isHurt)
                        {
                            _isHurt = true;
                            _hurtTimer = HURT_DURATION;
                            _health -= Spike.Damage;
                        }

                        continue; // skip for spikes
                    }

                    //-----------------------------------------------------------------//

                    if (_velocity.Y > 0)
                    {
                        newPos.Y = tile.Destination.Top - _destination.Height;
                        _isGrounded = true;
                        _velocity.Y = 0;
                    } else if (_velocity.Y < 0) // Jumping
                    {
                        newPos.Y = tile.Destination.Bottom;
                        _velocity.Y = 0;
                    }

                    // Update vertical bounds after collision adjustments
                    //vertBounds = GetPlayerBounds(newPos);

                } 
            }

            return newPos;
        }

        public abstract void PlayAnimation(CharState state, int speed);
       

    }
}
