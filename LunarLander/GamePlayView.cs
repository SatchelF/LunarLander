using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CS5410
{
    public class GamePlayView : GameStateView
    {
        private SpriteFont m_font;
        private Texture2D m_background; // Texture for the background image
        private Texture2D m_lunarLander; // Texture for the lunar lander
        private Vector2 m_landerPosition; // Position of the lunar lander
        private float m_landerRotation; // Rotation of the lunar lander
        private const string MESSAGE = "Isn't this game fun!";
        private Random rand = new Random(); // Random number generator

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/Space_Background"); // Load the background texture
            m_lunarLander = contentManager.Load<Texture2D>("Images/lunar_lander"); // Load the lunar lander texture
            InitializeNewGame();
        }

        private void InitializeNewGame()
        {
            // Screen height and width
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            int screenWidth = m_graphics.PreferredBackBufferWidth;

            // Upper part of the screen (e.g., top 50% for more restriction)
            int maxY = screenHeight / 2; // Upper half
            int minY = 0;

            // Central two-thirds of the screen
            int minX = screenWidth / 6;
            int maxX = 5 * screenWidth / 6;

            // Randomize the position
            int randomX = rand.Next(minX, maxX);
            int randomY = rand.Next(minY, maxY);

            m_landerPosition = new Vector2(randomX, randomY);

            // Randomize rotation to make it appear on its side (90 degrees left or right)
            // MathHelper.PiOver2 is 90 degrees in radians
            m_landerRotation = rand.Next(2) == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.GamePlay;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            // Draw the background covering the entire screen
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);

            // Draw the lunar lander at the randomized position, rotated, and scaled down
            Vector2 origin = new Vector2(m_lunarLander.Width / 2f, m_lunarLander.Height / 2f); // Origin for rotation
            m_spriteBatch.Draw(m_lunarLander, m_landerPosition, null, Color.White, m_landerRotation, origin, 0.3f, SpriteEffects.None, 0f);

            // Draw the text message
            Vector2 stringSize = m_font.MeasureString(MESSAGE);
            m_spriteBatch.DrawString(m_font, MESSAGE,
                new Vector2(m_graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, m_graphics.PreferredBackBufferHeight / 2 - stringSize.Y / 2), Color.Yellow);

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
            // Implement game logic updates here
        }
    }
}
