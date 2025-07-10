using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;



namespace FinalProject
{
    public class Sprite
    {
        // < Fields > -----------------------------------------
        protected Texture2D _texture;
        protected Rectangle _destination;
        protected Rectangle _source;
        protected Color _color;
        protected Color _origColor;
        protected Point _originalLocation;

        private int _moveDirection; // 1 = right, -1 = left

        // < Constructor > ---------------------------------------
        public Sprite(Texture2D texture, Rectangle destination, Rectangle source, Color color)
        {
            _texture = texture;
            _destination = destination;
            _source = source;
            _color = color;
            _origColor = color;

            _originalLocation = destination.Location;
            _moveDirection = 1;
        }

        // < Properties > -----------------------------------------
        public Texture2D Texture { get => _texture; }
        public Rectangle Destination { get => _destination; }
        public Rectangle Source { get => _source; }
        public Color Color { get => _color; }

        // < Methods > -------------------------------------
        public void ChangeColor(Color color) { _color = color; }    // for debugging

        public void Move( )
        {
            // Move left and right between boundaries
            int leftLimit = _originalLocation.X - Destination.Width * 3;
            int rightLimit = _originalLocation.X + Destination.Width * 3;

            if (_destination.X <= leftLimit)
            {
                _moveDirection = 1; // move right
            } else if (_destination.X >= rightLimit)
            {
                _moveDirection = -1; // move left
            }

            int velocity = 2 * _moveDirection;
            _destination.X += velocity;

        }
    }

    public class Spike : Sprite
    {
        private const int DAMAGE = 1;
        public Spike(Texture2D texture, Rectangle destination, Rectangle source, Color color)
            : base(texture, destination, source, color)
        {
        }

        public static int Damage => DAMAGE;
    }

}
