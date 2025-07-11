using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FinalProject
{
    internal class UIButton
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float Scale = 1f; // scaling
        public Action OnClick;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            (int)(Texture.Width * Scale),
            (int)(Texture.Height * Scale)
         );

        public bool IsEnabled = true;

        private Color _baseColor = Color.White;
        private Color _hoverColor = Color.LightGray;
        private Color _disabledColor = Color.Gray;

        public UIButton(Texture2D texture, Vector2 position, Action onClick)
        {
            Texture = texture;
            Position = position;
            OnClick = onClick;
        }

        private bool _wasPreviouslyPressed = false;

        public void Update()
        {
            if (!IsEnabled) return;

            MouseState mouse = Mouse.GetState();
            bool isHovering = Bounds.Contains(mouse.Position);
            bool isClicking = mouse.LeftButton == ButtonState.Pressed;

            if (isHovering && isClicking && !_wasPreviouslyPressed)
            {
                OnClick?.Invoke();
            }

            _wasPreviouslyPressed = isClicking;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            MouseState mouse = Mouse.GetState();
            Color drawColor = _baseColor;

            if (!IsEnabled)
                drawColor = _disabledColor;
            else if (Bounds.Contains(mouse.Position))
                drawColor = _hoverColor;

            spriteBatch.Draw(Texture, Position, null, drawColor, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }
    }
}
