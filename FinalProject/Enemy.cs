using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{
    /// <summary>
    /// Represents an enemy character with simple AI (roaming, chasing, attacking).
    /// </summary>
    public class Enemy : Character
    {
        // --- AI State ---
        private enum EnemyState { Roaming, Chasing, Attacking }
        private EnemyState _aiState = EnemyState.Roaming;

        // --- AI Timers ---
        private float _walkTimer = 0f, _walkDuration = 0f;
        private float _idleTimer = 0f, _idleDuration = 0f;

        // --- AI Movement ---
        private bool _facingRight = true;
        private bool _isIdle = false;
        private readonly Random _rand = new( );

        // --- Player Detection ---
        private readonly float _detectionRange = 300f;
        private readonly Player _player;
        private Vector2 _playerPosition;

        // --- Constructor ---
        public Enemy(Texture2D texture, Rectangle destination, Rectangle source, Color color, Player player)
            : base(texture, destination, source, color)
        {
            _player = player;
            _health = 100;
            _attackDamage = 20;
        }

        // --- Public Methods ---

        /// <summary>
        /// Updates the enemy's state, AI, and animation.
        /// </summary>
        public override void Update(Sprite[] platforms, GameTime gameTime)
        {
            if (_isDead) return;
            if (_health <= 0 && _state != CharState.Dead)
            {
                _player.HealOnKill( );
                _deathTimer = 0.75f;
                ChangeState(CharState.Dead);
                SoundManager.PlayDeathSound( );
            }

            HandleDeathState(gameTime);
            //Debug.WriteLine($"Enemy Update: {_state}, Health: {_health}, IsDead: {_isDead}");
            HandleHurtState(gameTime);
            HandleAttackState(gameTime);
            HandleAI(gameTime, platforms);
            ChangePosition(platforms);
            HandleFallJump( );
            PlayAnimation(_state);
            _attackCD -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCD < 0f) _attackCD = 0f;
        }

        /// <summary>
        /// Handles the enemy's death animation and sets IsDead when finished.
        /// </summary>
        public override void HandleDeathState(GameTime gameTime)
        {
            _deathTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_deathTimer <= 0f && _state == CharState.Dead)
            {
                _isDead = true;
            }
        }

        /// <summary>
        /// Sets the player's position for AI calculations.
        /// </summary>
        public void SetPlayerPosition(Vector2 pos) => _playerPosition = pos;

        // --- AI Logic ---

        private void HandleAI(GameTime gameTime, Sprite[] platforms)
        {
            float deltaT = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate player position relative to the enemy
            float distanceX = Math.Abs(_playerPosition.X - _destination.Center.X);
            float distanceY = Math.Abs(_playerPosition.Y - _destination.Center.Y);

            // --- AI State Decision ---
            if (distanceX < _detectionRange && distanceY < _detectionRange / 2)
            {
                _aiState = ( distanceX < 60 && distanceY < 60 ) ? EnemyState.Attacking : EnemyState.Chasing;
            } else
                _aiState = EnemyState.Roaming;

            // --- Behavior Based on AI State ---
            switch (_aiState)
            {
                case EnemyState.Roaming:
                    HandleRoaming(deltaT, platforms);
                    break;

                case EnemyState.Chasing:
                    HandleChasing(platforms);
                    break;

                case EnemyState.Attacking:
                    if (!_attacking)
                        if (_attackCD <= 0f) // Only attack if not on cooldown
                            HandleAttacking( );
                    else
                        ChangeState(CharState.Idle); // If on cooldown, just idle
                    break;
            }

        }

        private void HandleRoaming(float deltaT, Sprite[] platforms)
        {
            if (_isIdle)
            {
                _idleTimer += deltaT;
                _velocity.X = 0;
                ChangeState(CharState.Idle);

                if (_idleTimer >= _idleDuration)
                {
                    _isIdle = false;
                    _walkDuration = _rand.Next(1, 3);
                    _walkTimer = 0;
                }
                return;
            }

            _walkTimer += deltaT;
            _velocity.X = _facingRight ? SPEED : -SPEED;
            _direction = _facingRight ? 1 : -1;
            if (Velocity.Y == 0)
                ChangeState(CharState.Walking);

            if (_walkTimer >= _walkDuration)
            {
                _isIdle = true;
                _idleDuration = _rand.Next(1, 3);
                _idleTimer = 0;
                _facingRight = _rand.Next(0, 2) == 0 ? false : true;
            }

            if (!IsPlatformBelowNextStep(platforms) || IsWallAhead(platforms))
                Jump( );
        }

        private void HandleChasing(Sprite[] platforms)
        {
            float chaseSpeed = SPEED * 2f;
            float ignoreDistance = 10f;
            float distanceX = _playerPosition.X - _destination.X;

            if (Math.Abs(distanceX) > ignoreDistance)
            {
                _velocity.X = distanceX > 0 ? chaseSpeed : -chaseSpeed;
                _direction = distanceX > 0 ? 1 : -1;
                _facingRight = distanceX > 0;
            }

            ChangeState(CharState.Sprinting);

            if (!IsPlatformBelowNextStep(platforms) || IsWallAhead(platforms))
                Jump( );
        }

        protected virtual void HandleAttacking( )
        {
            if (_state != CharState.Attacking)
            {
                ChangeState(CharState.Attacking);
                _attacking = true;
                _attackTimer = 0.25f;
                _attackCD = 1.5f; // Set cooldown here, only when attack is triggered
                _hitLandedThisAttack = false; // resets to false on mouse click
            }
        }

        public override void HandleAttackState(GameTime gameTime)
        {
            _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_attackTimer > 0f)
            {
                Rectangle hitBox = GetCharacterBounds(_destination.Location);
                hitBox.Location = new Point(hitBox.X + ( hitBox.Width / 2 ) * _direction, hitBox.Y);
                hitBox.Width = hitBox.Width / 2;

                bool hitSomeone = false; // to detect if the enemy is hit
                if (!_hitLandedThisAttack && hitBox.Intersects(_player.Destination))
                {
                    _player.TakeDamage((int)_attackDamage, true);
                    SoundManager.PlayHitSound( );
                    _hitLandedThisAttack = true; // true if attack landed on enemy
                    hitSomeone = true; // enemy is hit = true
                }

                // If no enemy was hit, play the sword swing
                // I added this kasi ung hit.wav may kasama siyang sword swing
                if (!hitSomeone && !_hitLandedThisAttack)
                {
                    SoundManager.PlaySwordSwing( );
                    _hitLandedThisAttack = true; // set to true to not overlap sound effect
                }

                ChangeState(CharState.Attacking);
            } else
            {
                _attacking = false;

            }
        }

        // --- Movement/Environment Helpers ---

        private void Jump( )
        {
            if (_isGrounded)
            {
                _velocity.Y = -JUMP_POWER;
                _isGrounded = false;
                ChangeState(CharState.Jumping);
            }
        }

        private bool IsPlatformBelowNextStep(Sprite[] platforms)
        {
            if (_isGrounded) return true;
            int checkX = _facingRight ? _destination.Right + 5 : _destination.Left - 5;
            int checkY = _destination.Bottom + 5;
            Rectangle checkRect = new Rectangle(checkX, checkY, 2, 2);

            foreach (Sprite tile in platforms)
            {
                if (tile == null) continue;
                if (checkRect.Intersects(tile.Destination)) return true;
            }
            return false;
        }

        private bool IsWallAhead(Sprite[] platforms)
        {
            int checkX = _facingRight ? _destination.Right + 5 : _destination.Left - 5;
            int checkY = _destination.Top + _destination.Height / 2;
            Rectangle checkRect = new Rectangle(checkX, checkY, 5, 5);

            foreach (Sprite tile in platforms)
            {
                if (tile == null) continue;
                if (checkRect.Intersects(tile.Destination)) return true;
            }
            return false;
        }

        private void HandleFallJump( )
        {
            if (!_isGrounded)
            {
                if (_velocity.Y < 0)
                {
                    ChangeState(CharState.Jumping);
                } else if (_velocity.Y > 0)
                {
                    ChangeState(CharState.Falling);
                }
            }
        }

    }
}
