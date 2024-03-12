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
        private Texture2D m_background;
        private Texture2D m_lunarLander;
        private Texture2D m_pixel;
        private Vector2 m_landerPosition;
        private float m_landerRotation;
        private Vector2[] terrainPoints;
        private Random rand = new Random();
        private const float MaxTerrainHeight = 1.0f / 3.0f;
        private const float Roughness = 2.5f;
        private int safeZoneStartX;
        private int safeZoneEndX;
        private const float LandingZonePadding = .5f;
        private List<Triangle> terrainTriangles = new List<Triangle>();

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            m_background = contentManager.Load<Texture2D>("Images/Space_Background");
            m_lunarLander = contentManager.Load<Texture2D>("Images/lunar_lander");
            m_pixel = new Texture2D(m_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            m_pixel.SetData(new[] { Color.White });

            InitializeNewGame();
        }

        private void InitializeNewGame()
        {
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            int screenWidth = m_graphics.PreferredBackBufferWidth;
            int maxY = screenHeight / 5;
            int minY = 0;
            int minX = screenWidth / 6;
            int maxX = 5 * screenWidth / 6;

            int randomX = rand.Next(minX, maxX);
            int randomY = rand.Next(minY, maxY);
            m_landerPosition = new Vector2(randomX, randomY);

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
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);
            Vector2 origin = new Vector2(m_lunarLander.Width / 2f, m_lunarLander.Height / 2f);
            m_spriteBatch.Draw(m_lunarLander, m_landerPosition, null, Color.White, m_landerRotation, origin, 0.3f, SpriteEffects.None, 0f);

            foreach (var triangle in terrainTriangles)
            {
                DrawLine(m_spriteBatch, triangle.Point1, triangle.Point2, Color.Gray);
                DrawLine(m_spriteBatch, triangle.Point2, triangle.Point3, Color.Gray);
            }

            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                DrawLine(m_spriteBatch, terrainPoints[i], terrainPoints[i + 1], Color.White);
            }

            DrawLine(m_spriteBatch, new Vector2(safeZoneStartX, terrainPoints[safeZoneStartX].Y), new Vector2(safeZoneEndX, terrainPoints[safeZoneEndX].Y), Color.Green);

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
            // Implement game logic updates here
        }

        private void FlattenLandingZone()
        {
            float minY = float.MaxValue;

            // Find the highest Y value in the landing zone, which is actually the lowest point on the screen.
            for (int i = safeZoneStartX; i <= safeZoneEndX; i++)
            {
                if (terrainPoints[i].Y < minY)
                {
                    minY = terrainPoints[i].Y;
                }
            }

            // Set all points in the safe zone to this Y value.
            for (int i = safeZoneStartX; i <= safeZoneEndX; i++)
            {
                terrainPoints[i] = new Vector2(terrainPoints[i].X, minY);
            }
        }

        private void GenerateTerrain()
        {
            int screenWidth = m_graphics.PreferredBackBufferWidth;
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            int maxTerrainHeight = (int)(screenHeight * MaxTerrainHeight);
            int landerWidth = m_lunarLander.Width;
            int safeZoneWidth = (int)(landerWidth * LandingZonePadding);

            terrainPoints = new Vector2[screenWidth];
            float leftHeight = rand.Next(maxTerrainHeight / 2, maxTerrainHeight);
            float rightHeight = rand.Next(maxTerrainHeight / 2, maxTerrainHeight);

            terrainPoints[0] = new Vector2(0, screenHeight - leftHeight);
            terrainPoints[screenWidth - 1] = new Vector2(screenWidth - 1, screenHeight - rightHeight);

            MidpointDisplacement(0, screenWidth - 1, maxTerrainHeight);

            safeZoneStartX = rand.Next(landerWidth, screenWidth - landerWidth - safeZoneWidth);
            safeZoneEndX = safeZoneStartX + safeZoneWidth;

            FlattenLandingZone();
            float threshold = screenHeight * 0.1f; // Threshold for landing zone visibility.
            for (int i = safeZoneStartX; i <= safeZoneEndX; i++)
            {
                if (terrainPoints[i].Y > screenHeight - threshold)
                {
                    terrainPoints[i].Y = screenHeight - threshold;
                }
            }

            terrainTriangles.Clear();
            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                Vector2 bottomLeft = new Vector2(terrainPoints[i].X, screenHeight);
                Vector2 bottomRight = new Vector2(terrainPoints[i + 1].X, screenHeight);

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

        

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 2)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(m_pixel,
                new Rectangle(
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(),
                    thickness),
                null,
                color,
                angle,
                new Vector2(0, 0),
                SpriteEffects.None,
                0);
        }
    }
}
