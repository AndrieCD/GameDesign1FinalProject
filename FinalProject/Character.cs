using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{
    public enum CharState
    {
        Idle = 1,
        Walking = 2,
        Jumping = 3,
        Falling = 4,
        Hurt = 5,
        Sprinting = 6,
        Dead = 7,
        Attacking = 8
    }

    public abstract class Character : Sprite
    {
        // --- State & Movement ---
        protected int _health;
        protected CharState _state;
        protected CharState _previousState;
        protected Vector2 _velocity;
        protected bool _isGrounded;
        protected int _direction; // 1 = right, -1 = left

        // --- Animation ---
        protected int frameCounter;
        protected readonly int _frameWidth;
        protected readonly int _frameHeight;

        // --- Status ---
        protected bool _isDead = false;
        public bool IsDead { get => _isDead; set => _isDead = value; }

        // --- Action Timers ---
        protected bool _isHurt;
        protected float _hurtTimer;
        protected bool _attacking;
        protected float _attackTimer;
        protected float _attackCD;
        protected float _deathTimer = 0f;
        protected float _attackDamage;
        protected float _spikeTimer = 0f;

        // --- Constants ---
        protected const float HURT_DURATION = 0.25f;
        protected const int OFFSET = 50;
        protected const float SPEED = 3f;
        protected const float GRAVITY = 1f;
        protected const float JUMP_POWER = 25f;

        private float startDelay;

        // <---- SOUND EFFECTS ---->
        // Hit sound effect (to only play once per hit)
        // nagooverlap kasi siya sa bawat frame if wala this
        protected bool _hitLandedThisAttack = false; // resets to false

        // --- Constructor ---
        protected Character(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            startDelay = 4f;

            _state = CharState.Idle;
            _previousState = CharState.Idle;
            _velocity = Vector2.Zero;
            _isGrounded = true;
            _direction = 1;
            _originalLocation = new Point(destination.X, destination.Y);

            _isHurt = false;
            _hurtTimer = 0f;
            _attacking = false;
            _attackTimer = 0f;
            _attackCD = 0f;

            frameCounter = 0;
            _frameWidth = texture.Width / 4;
            _frameHeight = texture.Height / 7;
        }

        // --- Properties ---
        public Vector2 Velocity => _velocity;
        public int Direction => _direction;
        public int Health
        {
            get { return _health; }
            set { _health = value; }
        }
        public CharState State => _state;
        public Point Position
        {
            get { return _destination.Location; }
            set { _destination.Location = value; }
        }

        public float DeathTimer => _deathTimer;

        public virtual void Update(Sprite[] platforms, GameTime gameTime)
        {
            if (startDelay > 0f)
            {
                startDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                return; // Wait for start delay before processing updates
            }
        }
        public abstract void HandleDeathState(GameTime gameTime);

        public void ChangeState(CharState newState)
        {
            if (_state == CharState.Dead) return;
            if (_state != newState)
            {
                _previousState = _state;
                _state = newState;
                frameCounter = 0;
                //Debug.WriteLine($"State: {_state}");
            }
        }

        public void HandleHurtState(GameTime gameTime)
        {
            _hurtTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_hurtTimer > 0f)
            {
                if (!_attacking)
                    ChangeState(CharState.Hurt);
                ChangeColor(Color.Red);
            }
            else
            {
                _isHurt = false;
                ChangeColor(_origColor);
            }
        }
        public virtual void TakeDamage(int damage, bool ignoreIFrames = false)
        {
            if (_health > 0 && (!_isHurt || ignoreIFrames))
            {
                _health -= damage;
                _isHurt = true;
                _hurtTimer = HURT_DURATION;
                ChangeState(CharState.Hurt);
                Debug.WriteLine($"Took {damage} damage! Remaining health: {_health}");
            }
        }

        public void Heal(int healAMount)
        {
            SoundManager.PlayHealSound();
            if (_health < 100 && _health > 0 && !_isDead)
            {
                _health += healAMount;
                _health = Math.Clamp(_health, 0, 100);
            }
        }


        public virtual void HandleAttackState(GameTime gameTime)
        {
            _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackTimer > 0f)
            {
                ChangeState(CharState.Attacking);
            }
            else
            {
                _attacking = false;
            }
        }
        public void ChangePosition(Sprite[] platforms)
        {
            if (_attacking)
                _velocity.X = 0;

            if (!_isGrounded)
                _velocity.Y += GRAVITY;



            Point newPos = HandleCollisions(platforms);
            newPos.X = Math.Clamp(newPos.X, 0 - OFFSET, SceneManager.SCENEWIDTH - _destination.Width + OFFSET);
            _destination.Location = newPos;
        }

        protected Rectangle GetCharacterBounds(Point newPos)
        {
            return new Rectangle(
                newPos.X + OFFSET,
                newPos.Y + OFFSET,
                _destination.Width - (2 * OFFSET),
                _destination.Height - OFFSET
            );
        }

        protected Point HandleCollisions(Sprite[] tiles)
        {
            _isGrounded = false;
            //_isHurt = false;
            Point newPos = new Point(_destination.X, _destination.Y);

            // Horizontal
            newPos.X += (int)_velocity.X;
            Rectangle horizBounds = GetCharacterBounds(newPos);
            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite tile = tiles[i];
                if (tile == null) continue;
                if (horizBounds.Intersects(tile.Destination))
                {
                    if (tile is Spike)
                    {
                        // small slow
                        if (_velocity.X > 0)
                            _velocity.X = -1f; // slow down
                        else if (_velocity.X < 0)
                            _velocity.X = 1f; // slow down
                        _spikeTimer -= 0.01f;
                        if (this is Player && _spikeTimer <= 0)
                        {
                            _spikeTimer = 1f; // Reset spike timer to prevent multiple hits
                            SoundManager.PlayHitSound();
                            TakeDamage(Spike.Damage, true);
                        }
                        continue;
                    }
                    if (tile is Heart)
                    {
                        Heart heart = (Heart)tile;
                        if (heart.Collected) continue;
                        heart.Collected = true;
                        Heal(Heart.Heal);
                        continue;
                    }

                    if (_velocity.X > 0)
                        newPos.X = tile.Destination.Left - _destination.Width + OFFSET;
                    else if (_velocity.X < 0)
                        newPos.X = tile.Destination.Right - OFFSET;
                }
            }

            // Vertical
            newPos.Y += (int)_velocity.Y;
            Rectangle vertBounds = GetCharacterBounds(newPos);
            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite tile = tiles[i];
                if (tile == null) continue;
                if (vertBounds.Intersects(tile.Destination))
                {
                    if (tile is Spike)
                    {
                        // small slow
                        if (_velocity.X > 0)
                            _velocity.X = -1f; // slow down
                        else if (_velocity.X < 0)
                            _velocity.X = 1f; // slow down
                        _spikeTimer -= 0.01f;
                        if (this is Player && _spikeTimer <= 0)
                        {

                            _spikeTimer = 1f; // Reset spike timer to prevent multiple hits
                            SoundManager.PlayHitSound();
                            TakeDamage(Spike.Damage, true);
                        }
                        continue;
                    }
                    if (tile is Heart)
                    {
                        Heart heart = (Heart)tile;
                        if (heart.Collected) continue;
                        heart.Collected = true;
                        Heal(Heart.Heal);
                        continue;
                    }



                    if (_velocity.Y > 0)
                    {
                        newPos.Y = tile.Destination.Top - _destination.Height;
                        _isGrounded = true;
                        _velocity.Y = 0;

                    }
                    else if (_velocity.Y < 0)
                    {
                        newPos.Y = tile.Destination.Bottom;
                        _velocity.Y = 0;
                    }
                }
            }
            return newPos;
        }

        /// <summary>
        /// Selects and displays the correct animation frame based on the player's state.
        /// </summary>
        protected void PlayAnimation(CharState state)
        {
            int framesPerRow = 4;
            int startFrame, endFrame;
            int speed = 7;

            switch (state)
            {
                case CharState.Idle:
                    startFrame = 0; endFrame = 3; speed = 9;
                    break;
                case CharState.Jumping:
                    startFrame = 4; endFrame = 7;
                    break;
                case CharState.Falling:
                    startFrame = 7; endFrame = 7;
                    break;
                case CharState.Walking:
                    startFrame = 8; endFrame = 11;
                    break;
                case CharState.Sprinting:
                    startFrame = 12; endFrame = 14;
                    break;
                case CharState.Hurt:
                    startFrame = 16; endFrame = 18;
                    ChangeColor(Color.Red);
                    break;
                case CharState.Attacking:
                    startFrame = 24; endFrame = 26; speed = 4;
                    break;
                case CharState.Dead:
                    startFrame = 20; endFrame = 23; speed = 14;
                    ChangeColor(Color.Gray);
                    break;
                default:
                    startFrame = 0; endFrame = 0;
                    break;
            }

            if (frameCounter > speed)
            {
                int totalFrames = endFrame - startFrame + 1;
                int currentIndex = (frameCounter / speed) % totalFrames;
                int frameNumber = startFrame + currentIndex;
                int frameX = (frameNumber % framesPerRow) * _frameWidth;
                int frameY = (frameNumber / framesPerRow) * _frameHeight;
                _source = new Rectangle(new Point(frameX, frameY), new Point(_frameWidth, _frameHeight));
            }
            frameCounter++;
        }
    }
}
