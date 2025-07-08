using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FinalProject
{

    public class Player : Character
    {
        private const float SPRINT_MULTIPLIER = 1.5f; // multiplier for sprinting speed

        // < Constructor > ---------------------------------------
        public Player(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            HUD hUD = new HUD(this);
        }

        // < Methods > -------------------------------------
        public override void Update(Sprite[] _platform, GameTime gameTime)
        {
            // reset player after fall to the void
            if (_destination.Y > SceneManager.WINHEIGHT)
            {
                Die( );
            }

            ChangePosition(_platform);
            GetInputs(gameTime);

            if (_health <= 0) Die( );
        }

        public override void Die( )
        {
            // respawn
            _destination.Location = _originalLocation;
            _velocity.Y = 0f; // Reset vertical velocity
            _isGrounded = true; // Player is now grounded
            _health = 100;
        }

        public void GetInputs(GameTime gameTime)
        {
            //--------------------INPUT MANAGER--------------------------------//

            KeyboardState keyboardState = Keyboard.GetState( );
            MouseState mouseState = Mouse.GetState( );

            // Left and Right Movement
            if (keyboardState.IsKeyDown(Keys.A))
            {
                _velocity.X = -SPEED;
                _direction = -1; // Set direction to left
            } else if (keyboardState.IsKeyDown(Keys.D))
            {
                _velocity.X = SPEED;
                _direction = 1; // Set direction to right
            } else
            {
                _velocity.X = 0f; // no horizontal movement
            }

            // Shift to sprint
            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                _velocity.X *= SPRINT_MULTIPLIER; // increase speed
            }

            // Left Click to attack
            if (mouseState.LeftButton == ButtonState.Pressed && !_attacking)
            {
                float attackDuration = 0.5f; // duration of the attack in seconds

                _attacking = true;
                _attackTimer = attackDuration;
            }

            

            // Jump
            if (( keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ) && _isGrounded)
            {
                _velocity.Y = -JUMP_POWER; // jump
                _isGrounded = false; // Player will no longer be grounded after jumping
            }

            //------------------------STATE MANAGER-------------------------------//

            if (_isGrounded)
            {
                if (_velocity.X == 0f)
                {
                    ChangeState(CharState.Idle);
                } else
                {
                    if (keyboardState.IsKeyDown(Keys.LeftShift))
                    {
                        ChangeState(CharState.Sprinting);
                    } else
                    {
                        ChangeState(CharState.Walking);
                    }
                }
            } else
            {
                if (_velocity.Y < 0f)
                {
                    ChangeState(CharState.Jumping);
                } else if(_velocity.Y > 0f)
                {
                    ChangeState(CharState.Falling);
                }
            }
            _previousState = _state; // Store the previous state for animation purposes
            Debug.WriteLine($"State: {_state}   Velocity: {_velocity}");
            PlayAnimation(_state, 7);

        }


        public override void PlayAnimation(CharState state, int speed)
        {
            if (frameCounter > speed)
            {
                int framesPerRow = 4;       // Number of frames per row in the sprite sheet

                int startFrame, endFrame;

                switch (state)
                {
                    case CharState.Idle:
                        startFrame = 0;  // frame 0
                        endFrame = 2;
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
                        startFrame = 8;  // frame 8
                        endFrame = 11;
                        break;

                    case CharState.Sprinting:
                        startFrame = 12;
                        endFrame = 14;
                        break;


                    case CharState.Hurt:
                        startFrame = 16;  // frame 45
                        endFrame = 18;    // frame 47
                        ChangeColor(Color.Red);
                        break;

                    case CharState.Attacking:
                        startFrame = 24;  // frame 45
                        endFrame = 27;    // frame 47
                        break;

                    default:
                        startFrame = 0;
                        endFrame = 0;
                        break;

                }

                // Frame logic
                int totalFrames = endFrame - startFrame + 1;        // total frames in the animation
                // whenever frameCounter > speed, we update the index but not exceeding totalFrames
                int currentIndex = ( frameCounter / speed ) % totalFrames;    //-- current frame index based on the speed
                int frameNumber = startFrame + currentIndex;    // frame number in the entire 10x9 sprite sheet

                int frameX = ( frameNumber % framesPerRow ) * _frameWidth;     // calculate x pos ( zero based)
                int frameY = ( frameNumber / framesPerRow ) * _frameHeight;

                _source = new Rectangle(new Point(frameX, frameY), new Point(_frameWidth, _frameHeight));
            }

            frameCounter++;
        }


    }
}
