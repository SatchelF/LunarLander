using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Linq;
using System;

namespace CS5410
{
    public class HighScoresView : GameStateView
    {
        private SpriteFont m_font;
        private const string MESSAGE = "Scores";

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            return GameStateEnum.HighScores;
        }

        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();

            // Load high scores
            List<HighScore> highScores = LoadHighScores();

            Vector2 position = new Vector2(600, 100); // Starting position for high scores list

            // Display the message
            m_spriteBatch.DrawString(m_font, MESSAGE, new Vector2(m_graphics.PreferredBackBufferWidth / 2 - m_font.MeasureString(MESSAGE).X / 2, position.Y), Color.Yellow);
            position.Y += 100; // Adjust spacing as needed

            List<HighScore> sortedHighScores = highScores.OrderByDescending(score => score.FuelRemaining).ToList();

            // Display only the top 5 high scores
            int numberOfScoresToDisplay = Math.Min(sortedHighScores.Count, 5);
            for (int i = 0; i < numberOfScoresToDisplay; i++)
            {
                var score = sortedHighScores[i];
                string scoreText = $"{i + 1}. Score: {score.FuelRemaining} Date: {score.Date}";
                m_spriteBatch.DrawString(m_font, scoreText, position, Color.Goldenrod);
                position.Y += 50; // Increment Y position for the next score
            }

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
        }

        private List<HighScore> LoadHighScores()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.FileExists("HighScores.json"))
                {
                    return new List<HighScore>(); // Return an empty list if the file doesn't exist
                }

                using (IsolatedStorageFileStream stream = storage.OpenFile("HighScores.json", FileMode.Open))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<HighScore>));
                    return (List<HighScore>)serializer.ReadObject(stream);
                }
            }
        }

    }
}
