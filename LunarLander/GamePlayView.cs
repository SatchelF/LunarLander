using LunarLander;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        private List<int> safeZoneStartXs;
        private List<int> safeZoneEndXs;
        private int numberOfLandingZones;
        private const float LandingZonePadding = .5f;
        private List<Triangle> terrainTriangles = new List<Triangle>();
        private Vector2 m_velocity; // Velocity of the lander
        private Vector2 m_gravity; // Gravity applied to the lander
        private const float Thrust = -100f; // Thrust power
        private const float RotationSpeed = 1.0f; // Rotation speed
        private const int MaxFuel = 300; // Maximum fuel capacity
        private int m_fuel; // Current fuel amount
        private float m_verticalSpeed; // Add a member variable to store the vertical speed
        private bool GameOver;
        private bool successfulLanding;
        private SoundEffect thrustSound; // Sound effect for the thruster
        private SoundEffectInstance thrustSoundInstance; // To control playback of the thruster sound
        private SoundEffect destructionSound;
        private bool explosionSoundPlayed;
        private int currentLevel = 1; // Track the current level
        private float countdownTimer = 3.0f; // 3 seconds countdown
        private bool showCountdown = true; // Flag to control countdown display
        private bool showVictoryMessage = false; // Flag to control victory message display



        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/menu");
            int randomBackgroundIndex = rand.Next(2, 7);
            string backgroundPath = $"Images/Space_Background{randomBackgroundIndex}";
            m_background = contentManager.Load<Texture2D>(backgroundPath);
            m_lunarLander = contentManager.Load<Texture2D>("Images/lunar_lander");
            destructionSound = contentManager.Load<SoundEffect>("lander_explosion");
            thrustSound = contentManager.Load<SoundEffect>("lunar_booster");
            thrustSoundInstance = thrustSound.CreateInstance();
            thrustSoundInstance.IsLooped = true;
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
            explosionSoundPlayed = false;
            int maxX = 5 * screenWidth / 6;
            m_velocity = Vector2.Zero; // Start with no movement
            m_gravity = new Vector2(0, 10.00f); // Downward gravity. Adjust as needed.
            GameOver = false;
            successfulLanding = false;
            showCountdown = false; // Make sure countdown does not start at the beginning
            countdownTimer = 3.0f; // Reset for when we do need it

            int randomX = rand.Next(minX, maxX);
            int randomY = rand.Next(minY, maxY);
            m_landerPosition = new Vector2(randomX, randomY);
            m_fuel = MaxFuel; // Set initial fuel to maximum capacity

            m_landerRotation = rand.Next(2) == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;

            GenerateTerrain();
        }

        public override GameStateEnum processInput(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }

            if (GameOver)
            {
                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    if (!successfulLanding || currentLevel == 2)
                    {
                        currentLevel = 1; // Reset to level 1 if the game was not successful or if it was the last level
                    }
                    else
                    {
                        currentLevel++; // Move to next level if successful
                    }
                    InitializeNewGame(); // Initialize game based on the current level
                    GameOver = false; // Reset the game over flag
                }
                return GameStateEnum.GamePlay; // Keep the game state on GamePlay
            }

            

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                m_landerRotation -= RotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                m_landerRotation = (m_landerRotation + MathHelper.TwoPi) % MathHelper.TwoPi;
            }

            // Rotate Right
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                m_landerRotation += RotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                m_landerRotation = (m_landerRotation + MathHelper.TwoPi) % MathHelper.TwoPi;
            }

            if (keyboardState.IsKeyDown(Keys.Up) && m_fuel > 0)
            {
                if (thrustSoundInstance.State != SoundState.Playing)
                {
                    thrustSoundInstance.Play(); // Play the sound only if not already playing
                }

                m_fuel -= 1;

                Vector2 thrustDirection = new Vector2(-(float)Math.Sin(m_landerRotation), (float)Math.Cos(m_landerRotation));
                m_velocity += thrustDirection * Thrust * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                if (thrustSoundInstance.State == SoundState.Playing)
                {
                    thrustSoundInstance.Stop(); // Stop the sound if the up key is released or fuel is depleted
                }
            }

            return GameStateEnum.GamePlay;
        }

        private void GenerateTerrain()
        {
            int screenWidth = m_graphics.PreferredBackBufferWidth;
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            int maxTerrainHeight = (int)(screenHeight * MaxTerrainHeight);

            terrainPoints = new Vector2[screenWidth];
            float leftHeight = rand.Next(maxTerrainHeight / 2, maxTerrainHeight);
            float rightHeight = rand.Next(maxTerrainHeight / 2, maxTerrainHeight);

            terrainPoints[0] = new Vector2(0, screenHeight - leftHeight);
            terrainPoints[screenWidth - 1] = new Vector2(screenWidth - 1, screenHeight - rightHeight);

            // Generate the midpoint displacement
            MidpointDisplacement(0, screenWidth - 1, maxTerrainHeight);

            // Set number of landing zones based on the current level
            SetNumberOfLandingZones(currentLevel);

            // Adjust landing zone generation to account for screen margins
            int margin = (int)(screenWidth * 0.15); // 15% margin from sides
            int usableWidth = screenWidth - 2 * margin; // Width minus margins
            int segmentWidth = usableWidth / (numberOfLandingZones + 1);

            safeZoneStartXs = new List<int>();
            safeZoneEndXs = new List<int>();

            // Determine landing zone size modifier based on the level
            float landingZoneSizeModifier = currentLevel == 2 ? 0.25f : LandingZonePadding; // Smaller zones for level 2

            for (int i = 0; i < numberOfLandingZones; i++)
            {
                int safeZoneWidth = (int)(segmentWidth * landingZoneSizeModifier);
                int safeZoneStartX = margin + segmentWidth * i + (segmentWidth - safeZoneWidth) / 2;
                int safeZoneEndX = safeZoneStartX + safeZoneWidth;

                safeZoneStartXs.Add(Math.Max(safeZoneStartX, margin)); // Ensure it's within the margin
                safeZoneEndXs.Add(Math.Min(safeZoneEndX, screenWidth - margin)); // Ensure it's within the margin
            }

            // Flatten the landing zones and interpolate terrain
            for (int i = 0; i < numberOfLandingZones; i++)
            {
                FlattenLandingZone(safeZoneStartXs[i], safeZoneEndXs[i]);
            }

            // Clear previous terrain triangles and generate new ones
            terrainTriangles.Clear();
            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                Vector2 bottomLeft = new Vector2(terrainPoints[i].X, screenHeight);
                Vector2 bottomRight = new Vector2(terrainPoints[i + 1].X, screenHeight);

                terrainTriangles.Add(new Triangle(terrainPoints[i], terrainPoints[i], bottomLeft)); // This triangle is for the outline (top)
                terrainTriangles.Add(new Triangle(terrainPoints[i], bottomLeft, bottomRight)); // This triangle is for the filled part
                terrainTriangles.Add(new Triangle(terrainPoints[i], bottomRight, terrainPoints[i + 1])); // This triangle is for the outline (top)
            }
        }

        private void FlattenLandingZone(int safeZoneStartX, int safeZoneEndX)
        {
            int screenHeight = m_graphics.PreferredBackBufferHeight;
            float landingZoneY = terrainPoints[safeZoneStartX].Y;
            for (int i = safeZoneStartX; i <= safeZoneEndX; i++)
            {
                // Ensure landing zones do not go below the screen by limiting their Y value
                terrainPoints[i].Y = Math.Min(landingZoneY, screenHeight - 1); // Ensure Y-value is above the screen bottom
            }

            // Gradually adjust the terrain leading up to the landing zones to prevent sharp edges.
            InterpolateTerrainToLandingZone(safeZoneStartX, safeZoneEndX);
        }

        private void InterpolateTerrainToLandingZone(int safeZoneStartX, int safeZoneEndX)
        {
            // Define the range over which we will interpolate the heights leading up to the landing zone
            int interpolationRange = 20; // Adjust this value as needed for a smoother transition

            // Interpolate points before the landing zone
            if (safeZoneStartX > interpolationRange)
            {
                float startHeight = terrainPoints[safeZoneStartX - interpolationRange].Y;
                float endHeight = terrainPoints[safeZoneStartX].Y;
                float heightDiff = endHeight - startHeight;

                for (int i = 0; i < interpolationRange; i++)
                {
                    float fractionalHeight = heightDiff * (i / (float)interpolationRange);
                    terrainPoints[safeZoneStartX - interpolationRange + i].Y = startHeight + fractionalHeight;
                }
            }

            // Interpolate points after the landing zone
            if (safeZoneEndX < terrainPoints.Length - interpolationRange)
            {
                float startHeight = terrainPoints[safeZoneEndX].Y;
                float endHeight = terrainPoints[safeZoneEndX + interpolationRange].Y;
                float heightDiff = endHeight - startHeight;

                for (int i = 1; i <= interpolationRange; i++)
                {
                    float fractionalHeight = heightDiff * (i / (float)interpolationRange);
                    terrainPoints[safeZoneEndX + i].Y = startHeight + fractionalHeight;
                }
            }
        }



        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, m_graphics.PreferredBackBufferWidth, m_graphics.PreferredBackBufferHeight), Color.White);


            Vector2 statusPosition = new Vector2(m_graphics.PreferredBackBufferWidth - 500, 50);
            Vector2 leftSidePosition = new Vector2(50, 50); // Adjust x and y as needed

            string levelText = $"Level: {currentLevel}";
            Color levelColor = Color.Goldenrod;

            // Assuming you have a way to calculate or update score
            string scoreText = $"Score: [Your Score Here]"; // Replace [Your Score Here] with actual score
            Color scoreColor = Color.Goldenrod; // You can choose a different color if you like


            string fuelText = $"Fuel: {m_fuel / 3}";
            Color fuelColor = m_fuel > 0 ? Color.Green : Color.White;

            // Assume these values for now, replace with actual vertical speed and angle later
            string verticalSpeedText = $"Vertical Speed: {m_verticalSpeed / 10} m/s";
            Color verticalSpeedColor = m_verticalSpeed > 20 ? Color.White : Color.Green;

            float angle = (MathHelper.ToDegrees(m_landerRotation) + 360) % 360;
            string angleText = $"Angle: {angle}"; // Updated angle text
            Color angleColor = (angle > 5 && angle < 355) ? Color.White : Color.Green;



            // Draw level and score text
            m_spriteBatch.DrawString(m_font, levelText, leftSidePosition, levelColor);
            leftSidePosition.Y += m_font.LineSpacing; // Move down for the next piece of information
            m_spriteBatch.DrawString(m_font, scoreText, leftSidePosition, scoreColor);


            // Draw the status text
            m_spriteBatch.DrawString(m_font, fuelText, statusPosition, fuelColor);
            statusPosition.Y += m_font.LineSpacing; // Move down for next line
            m_spriteBatch.DrawString(m_font, verticalSpeedText, statusPosition, verticalSpeedColor);
            statusPosition.Y += m_font.LineSpacing; // Move down for next line
            m_spriteBatch.DrawString(m_font, angleText, statusPosition, angleColor);

            Vector2 centerScreen = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2);
            string messageText = "";
            Color messageColor = Color.White;


            // Draw the lander...

            if (!GameOver)
            {
                Vector2 origin = new Vector2(m_lunarLander.Width / 2f, m_lunarLander.Height / 2f);
                m_spriteBatch.Draw(m_lunarLander, m_landerPosition, null, Color.White, m_landerRotation, origin, 0.3f, SpriteEffects.None, 0f);
            }

            // Draw the filled terrain with gray color
            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                Vector2 start = terrainPoints[i];
                Vector2 end = terrainPoints[i + 1];
                Vector2 bottomStart = new Vector2(start.X, m_graphics.PreferredBackBufferHeight);
                Vector2 bottomEnd = new Vector2(end.X, m_graphics.PreferredBackBufferHeight);

                // Draw the gray filled area for this segment
                DrawRectangle(m_spriteBatch, start, bottomStart, end, bottomEnd, Color.Gray);
            }

            // Draw the white terrain outline
            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                DrawLine(m_spriteBatch, terrainPoints[i], terrainPoints[i + 1], Color.White, 3);
            }

            // Draw the safe zones
            for (int zone = 0; zone < numberOfLandingZones; zone++)
            {
                DrawLine(m_spriteBatch,
                    new Vector2(safeZoneStartXs[zone], terrainPoints[safeZoneStartXs[zone]].Y),
                    new Vector2(safeZoneEndXs[zone], terrainPoints[safeZoneEndXs[zone]].Y),
                    Color.Green, 3);
            }

            if (GameOver)
            {
                

                if (successfulLanding && currentLevel == 2)
                {
                    messageText = "YOU WIN - Press Enter to Restart or ESC for new game ";
                    messageColor = Color.Goldenrod;
                }


                else if (successfulLanding)
                {
                    messageText = "Mission Success - Press Enter to Continue ";
                    messageColor = Color.Green;
                }


                else
                {
                    messageText = "Mission Failed - Press Enter to Restart";
                    messageColor = Color.Red;
                }

                Vector2 textSize = m_font.MeasureString(messageText);
                Vector2 textPosition = centerScreen - textSize / 2;
                m_spriteBatch.DrawString(m_font, messageText, textPosition, messageColor);
            }


            if (showCountdown)
            {
                // Display countdown
                string countdownText = Math.Ceiling(countdownTimer).ToString();
                Vector2 countdownPosition = new Vector2(m_graphics.PreferredBackBufferWidth / 2, m_graphics.PreferredBackBufferHeight / 2 + 100);
                // Adjust the position as needed
                m_spriteBatch.DrawString(m_font, countdownText, countdownPosition, Color.White);
            }
            

            m_spriteBatch.End();
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 bottomLeft, Vector2 topRight, Vector2 bottomRight, Color color)
        {
            spriteBatch.Draw(m_pixel, new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(topRight.X - topLeft.X), (int)(bottomLeft.Y - topLeft.Y)), null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }


        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            // Calculate the angle to rotate the line
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
                new Vector2(0, 0), // Origin within the line, set to (0,0) as we're rotating around the start point
                SpriteEffects.None,
                0);
        }


        public override void update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Apply gravity to the lander's velocity
            m_velocity += m_gravity * deltaTime;

            // Update the lander's position
            m_landerPosition += m_velocity * deltaTime;

            // Calculate the vertical speed for display (adjust as necessary for your logic)
            m_verticalSpeed = m_velocity.Y;

            if (showCountdown && countdownTimer > 0)
            {
                countdownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (countdownTimer <= 0)
                {
                    showCountdown = false;
                    if (currentLevel == 1 && successfulLanding)
                    {
                        currentLevel = 2; // Move to next level after countdown
                        InitializeNewGame();
                    }
                }
                return; // Skip other updates during countdown
            }

            for (int i = 0; i < terrainPoints.Length - 1; i++)
            {
                if (LineCircleIntersection(terrainPoints[i], terrainPoints[i + 1], m_landerPosition, 30)) // Assuming a small radius for simplicity
                {
                    if (CheckForSafeLanding(m_landerPosition))
                    {
                        // Handle successful landing
                        successfulLanding = true;
                        GameOver = true; // Or handle as appropriate for your game
                        // Optionally trigger success actions here
                    }
                    else
                    {
                        // Handle crash
                        ShowGameOver();
                    }
                    break;
                }
            }
            // Check if the lander has fallen off the screen
            if (m_landerPosition.Y > m_graphics.PreferredBackBufferHeight)
            {
                ShowGameOver();
            }





        }





        public void SetNumberOfLandingZones(int level)
        {
            // Adjust this method to set the number of landing zones based on the level
            if (level == 1)
            {
                numberOfLandingZones = 2;
            }
            else if (level == 2)
            {
                numberOfLandingZones = 1; // Level 2 has a smaller, single landing zone
            }
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


        private bool LineCircleIntersection(Vector2 pt1, Vector2 pt2, Vector2 circleCenter, float circleRadius)
        {
            Vector2 v1 = pt2 - pt1;
            Vector2 v2 = pt1 - circleCenter;
            float b = -2 * (v1.X * v2.X + v1.Y * v2.Y);
            float c = 2 * (v1.X * v1.X + v1.Y * v1.Y);
            float d = b * b - 2 * c * (v2.X * v2.X + v2.Y * v2.Y - circleRadius * circleRadius);
            if (d < 0)
            {
                return false;
            }
            d = (float)Math.Sqrt(d);
            float u1 = (b - d) / c;
            float u2 = (b + d) / c;
            return (u1 <= 1 && u1 >= 0) || (u2 <= 1 && u2 >= 0);
        }

        private void ShowGameOver()
        {


            if (thrustSoundInstance.State == SoundState.Playing)
            {
                thrustSoundInstance.Stop();
            }

            GameOver = true;
            if (successfulLanding)
            {
                if (currentLevel == 1)
                {
                    // Transition from Level 1 to 2 with a countdown
                    showCountdown = true;
                    countdownTimer = 3.0f;
                }
                else if (currentLevel == 2)
                {
                    // After completing Level 2, show victory message without a countdown
                    showVictoryMessage = true;
                    // Do not automatically reset or proceed; wait for player input
                }
            }
            else if (!explosionSoundPlayed)
            {
                destructionSound.Play();
                explosionSoundPlayed = true;
            }
        }


        private bool CheckForSafeLanding(Vector2 collisionPoint)
        {
            // Check if within a safe zone
            bool isInSafeZone = false;
            for (int i = 0; i < safeZoneStartXs.Count; i++)
            {
                if (collisionPoint.X >= safeZoneStartXs[i] && collisionPoint.X <= safeZoneEndXs[i])
                {
                    isInSafeZone = true;
                    break;
                }
            }

            // Check the vertical speed is less than 2 m/s
            bool isSpeedSafe = Math.Abs(m_velocity.Y) < 20;

            // Check the lander's angle is within 5 degrees of vertical
            float angleDegrees = MathHelper.ToDegrees(m_landerRotation) % 360;
            if (angleDegrees < 0) angleDegrees += 360; // Normalize angle to 0-360 range
            bool isAngleSafe = angleDegrees <= 5 || angleDegrees >= 355;

            // Checks for vertical speed remain the same...

            return isInSafeZone && isSpeedSafe && isAngleSafe;
        }
    }





}