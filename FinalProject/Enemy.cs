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

        // --- Status ---
        private bool _isDead = false;
        public bool IsDead { get => _isDead; set => _isDead = value; }

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

        }

        // --- Public Methods ---

        /// <summary>
        /// Updates the enemy's state, AI, and animation.
        /// </summary>
        public override void Update(Sprite[] platforms, GameTime gameTime)
        {
            if (_health <= 0 && _state != CharState.Dead)
            {
                _deathTimer = 2f;
                ChangeState(CharState.Dead);
                Die(gameTime);
                return;
            }

            HandleHurtState(gameTime);
            HandleAttackState(gameTime);
            HandleAI(gameTime, platforms);
            ChangePosition(platforms);
            HandleVerticalStates( );
            Debug.WriteLine($"State: {_state}");
            PlayAnimation(_state);
            _attackCD -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCD < 0f) _attackCD = 0f;
        }

        /// <summary>
        /// Handles the enemy's death animation and sets IsDead when finished.
        /// </summary>
        public override void Die(GameTime gameTime)
        {
            _deathTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_deathTimer <= 0f)
                _isDead = true;
        }

        /// <summary>
        /// Sets the player's position for AI calculations.
        /// </summary>
        public void SetPlayerPosition(Vector2 pos) => _playerPosition = pos;

        // --- AI Logic ---

        private void HandleAI(GameTime gameTime, Sprite[] platforms)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _aiState = EnemyState.Roaming;
            //ChangeState(CharState.Idle);

            float dx = _playerPosition.X - _destination.Center.X;
            float dy = Math.Abs(_playerPosition.Y - _destination.Center.Y);
            bool playerInFront = ( _facingRight && dx > 0 ) || ( !_facingRight && dx < 0 );

            if (_isHurt || ( Math.Abs(dx) < _detectionRange && dy < _detectionRange / 2 && playerInFront ))
            {
                if (!playerInFront)
                    _facingRight = !_facingRight;

                _aiState = ( Math.Abs(dx) < 60 && dy < 60 ) ? EnemyState.Attacking : EnemyState.Chasing;
            }

            switch (_aiState)
            {
                case EnemyState.Roaming: HandleRoaming(delta, platforms); break;
                case EnemyState.Chasing: HandleChasing(platforms); break;
                case EnemyState.Attacking:
                    if (_attackCD <= 0f) HandleAttacking( );
                    break;
            }
        }

        private void HandleRoaming(float delta, Sprite[] platforms)
        {
            if (_isIdle)
            {
                _idleTimer += delta;
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

            _walkTimer += delta;
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

            if (!IsGroundAhead(platforms) || IsGroundInFront(platforms))
                Jump( );
        }

        private void HandleChasing(Sprite[] platforms)
        {
            float chaseSpeed = SPEED * 2f;
            float deadZone = 10f;
            float dx = _playerPosition.X - _destination.X;

            if (Math.Abs(dx) > deadZone)
            {
                _velocity.X = dx > 0 ? chaseSpeed : -chaseSpeed;
                _direction = dx > 0 ? 1 : -1;
                _facingRight = dx > 0;
            }

            ChangeState(CharState.Sprinting);

            if (!IsGroundAhead(platforms) || IsGroundInFront(platforms))
                Jump( );
        }

        private void HandleAttacking( )
        {
            float dx = _playerPosition.X - _destination.Center.X;
            bool playerInFront = ( _facingRight && dx > 0 ) || ( !_facingRight && dx < 0 );

            if (_state != CharState.Attacking && playerInFront)
            {
                _attacking = true;
                _attackTimer = 0.25f;
                _attackCD = 1.5f; // Set cooldown here, only when attack is triggered
                ChangeState(CharState.Attacking);
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
                if (hitBox.Intersects(_player.Destination))
                {
                    _player.TakeDamage(5);
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

        private bool IsGroundAhead(Sprite[] platforms)
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

        private bool IsGroundInFront(Sprite[] platforms)
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

        private void HandleVerticalStates( )
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
