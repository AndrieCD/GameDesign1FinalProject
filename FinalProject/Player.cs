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

        // < Constructor > ---------------------------------------
        // This constructor sets up the player with its texture, position, and color.
        // It also creates a HUD (Heads-Up Display) for the player.
        public Player(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
            HUD hUD = new HUD(this); // Create a HUD to display player info (like health).
        }

        // < Methods > -------------------------------------
        // This method is called every frame to update the player's state.
        public override void Update(Sprite[] _platform, GameTime gameTime)
        {
            // If the player falls below the bottom of the screen, respawn them.
            if (_destination.Y > SceneManager.WINHEIGHT)
            {
                Die( );
            }

            // Move the player and handle collisions.
            ChangePosition(_platform);

            // Handle player input (keyboard and mouse).
            GetInputs(gameTime);

            // If the player's health drops to zero, respawn them.
            if (_health <= 0) Die( );
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

            // If the player clicks the left mouse button and is not already attacking, start an attack.
            if (mouseState.LeftButton == ButtonState.Pressed && !_attacking)
            {
                float attackDuration = 0.5f; // How long the attack lasts (in seconds).

                _attacking = true; // Set attacking state.
                _attackTimer = attackDuration; // Start attack timer.
            }

            // Handle jumping: Space or W key, and only if on the ground.
            if (( keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) ) && _isGrounded)
            {
                _velocity.Y = -JUMP_POWER; // Move up (jump).
                _isGrounded = false; // Player is now in the air.
            }

            //------------------------STATE MANAGER-------------------------------//

            // Set the player's state based on movement and whether they're on the ground.
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

            _previousState = _state; // Store the previous state for animation purposes.

            // Print the current state and velocity to the debug output (for developers).
            Debug.WriteLine($"State: {_state}   Velocity: {_velocity}");

            // Play the correct animation for the current state.
            PlayAnimation(_state, 7);
        }

        // This method selects and displays the correct animation frame based on the player's state.
        public override void PlayAnimation(CharState state, int speed)
        {
            // Only update the animation frame if enough time has passed.
            if (frameCounter > speed)
            {
                int framesPerRow = 4; // Number of frames per row in the sprite sheet.

                int startFrame, endFrame;

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
                        endFrame = 27;
                        break;

                    default:
                        startFrame = 0;
                        endFrame = 0;
                        break;
                }

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
