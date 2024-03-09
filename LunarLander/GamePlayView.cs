using LunarLander;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CS5410
{
    public class GamePlayView : GameStateView
    {
        private SpriteFont m_font;
        private Texture2D m_background; // Texture for the background image
        private Texture2D m_lunarLander; // Texture for the lunar lander
        private Texture2D m_pixel; // Texture for drawing lines
        private Vector2 m_landerPosition; // Position of the lunar lander
        private float m_landerRotation; // Rotation of the lunar lander
        private Vector2[] terrainPoints; // Points for the terrain
        private Random rand = new Random(); // Random number generator
        private const float MaxTerrainHeight = 2.0f / 3.0f; // Maximum height for the terrain
        private const float Roughness = 2.0f;
        private int safeZoneStartX; // X position of the start of the safe zone
        private int safeZoneEndX;   // X position of the end of the safe zone
        private const float LandingZonePadding = .5f; // To make the landing zone a bit larger than the lunar lander
        private List<Triangle> terrainTriangles = new List<Triangle>();


        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/Space_Background");
            m_lunarLander = contentManager.Load<Texture2D>("Images/lunar_lander");

            // Create a 1x1 white texture for drawing lines
            m_pixel = new Texture2D(m_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            m_pixel.SetData(new[] { Color.White });

            InitializeNewGame();
        }

        private void InitializeNewGame()
        {
            // Screen height and width
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            int screenWidth = m_graphics.PreferredBackBufferWidth;

            // Upper part of the screen (e.g., top 50% for more restriction)
            int maxY = screenHeight / 3; // Upper half
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

            GenerateTerrain();
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

            // Test: Draw the outlines of the terrain triangles
            foreach (var triangle in terrainTriangles)
            {
                // Draw edges for the triangle - this is a simplified example
                DrawLine(m_spriteBatch, triangle.Point1, triangle.Point2, Color.White);
                DrawLine(m_spriteBatch, triangle.Point2, triangle.Point3, Color.White);
                DrawLine(m_spriteBatch, triangle.Point3, triangle.Point1, Color.White);
            }

            m_spriteBatch.End();
        }


        public override void update(GameTime gameTime)
        {
            // Implement game logic updates here
        }

        private void GenerateTerrain()
        {
            int screenWidth = m_graphics.PreferredBackBufferWidth;
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            int maxTerrainHeight = (int)(screenHeight * MaxTerrainHeight);

            // Calculate the width of the safe zone based on the width of the lunar lander
            int landerWidth = m_lunarLander.Width;
            int safeZoneWidth = (int)(landerWidth * LandingZonePadding);

            terrainPoints = new Vector2[screenWidth];
            float leftHeight = rand.Next(maxTerrainHeight / 2, maxTerrainHeight); // Increase the range for more verticality
            float rightHeight = rand.Next(maxTerrainHeight / 2, maxTerrainHeight);

            // Initialize the first and last points
            terrainPoints[0] = new Vector2(0, screenHeight - leftHeight);
            terrainPoints[screenWidth - 1] = new Vector2(screenWidth - 1, screenHeight - rightHeight);

            // Apply recursive subdivision with higher initial displacement for more extreme terrain
            MidpointDisplacement(0, screenWidth - 1, maxTerrainHeight); // Increased displacement for more verticality

            // Determine the random position for the landing zone, ensuring it's not too close to the edges
            safeZoneStartX = rand.Next(landerWidth, screenWidth - landerWidth - safeZoneWidth);
            safeZoneEndX = safeZoneStartX + safeZoneWidth;

            // Flatten the landing zone
            FlattenLandingZone();


            // Assuming terrainPoints have been generated...
            terrainTriangles.Clear();
            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                Vector2 bottomLeft = new Vector2(terrainPoints[i].X, m_graphics.PreferredBackBufferHeight);
                Vector2 bottomRight = new Vector2(terrainPoints[i + 1].X, m_graphics.PreferredBackBufferHeight);

                // Create two triangles for each terrain segment to simulate a filled polygon
                terrainTriangles.Add(new Triangle(terrainPoints[i], bottomLeft, terrainPoints[i + 1]));
                terrainTriangles.Add(new Triangle(terrainPoints[i + 1], bottomLeft, bottomRight));
            }
            System.Diagnostics.Debug.WriteLine($"Generated {terrainTriangles.Count} terrain triangles.");


        }

        private void MidpointDisplacement(int leftIndex, int rightIndex, float displacement)
        {
            if (leftIndex < rightIndex - 1)
            {
                int midIndex = (leftIndex + rightIndex) / 2;
                float midHeight = (terrainPoints[leftIndex].Y + terrainPoints[rightIndex].Y) / 2;

                midHeight = MathHelper.Clamp(midHeight + RandomDisplacement(midIndex - leftIndex),
                                             m_graphics.PreferredBackBufferHeight * MaxTerrainHeight,
                                             m_graphics.PreferredBackBufferHeight);

                terrainPoints[midIndex] = new Vector2(midIndex, midHeight);

                MidpointDisplacement(leftIndex, midIndex, displacement * Roughness);
                MidpointDisplacement(midIndex, rightIndex, displacement * Roughness);
            }
        }


        private float RandomDisplacement(int length)
        {
            return (float)(rand.NextDouble() - 0.5) * Roughness * length;
        }


        private void FlattenLandingZone()
        {
            // Calculate the average height where the safe zone will be to make it flat
            float averageHeight = 0f;
            int count = 0;
            for (int i = safeZoneStartX; i <= safeZoneEndX; i++)
            {
                averageHeight += terrainPoints[i].Y;
                count++;
            }
            averageHeight /= count;

            // Apply the average height to the landing zone to flatten it
            for (int i = safeZoneStartX; i <= safeZoneEndX; i++)
            {
                terrainPoints[i] = new Vector2(i, averageHeight);
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 3)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(m_pixel,
                new Rectangle( // Define the rectangle
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), // Length of the line
                    thickness), // Thickness of the line
                null,
                color,
                angle, // Rotation angle
                new Vector2(0, 0), // Origin within the line
                SpriteEffects.None,
                0);
        }


    }
}