using CS5410.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace CS5410
{
    public class SettingsView : GameStateView
    {
        private SpriteFont m_font;
        private SpriteFont m_font2;
        private const string MESSAGE = "Press Enter to change the control, ESC to return, and up and down arrows to navigate";
        private Dictionary<string, Keys> keyBindings;
        private enum SettingsState { Viewing, Changing }
        private SettingsState currentState = SettingsState.Viewing;
        private string changingControl = "";
        private List<string> controlNames; // To iterate through controls
        private int selectedControlIndex = 0;
        private KeyboardInput keyboardInput;
        private KeyboardState oldState;


        public SettingsView()
        {
            keyBindings = new Dictionary<string, Keys>
            {
                { "Rotate Left", Keys.Left },
                { "Rotate Right", Keys.Right },
                { "Thrust", Keys.Up }
            };

            controlNames = new List<string>(keyBindings.Keys);
            keyboardInput = new KeyboardInput();
        }

        public override void loadContent(ContentManager contentManager)
        {
            m_font = contentManager.Load<SpriteFont>("Fonts/main-menu");
            m_font2 = contentManager.Load<SpriteFont>("Fonts/menu");
            keyboardInput.registerCommand(Keys.Up, true, NavigateUp);
            keyboardInput.registerCommand(Keys.Down, true, NavigateDown);
            keyboardInput.registerCommand(Keys.Enter, true, SelectControl);

        }



        public override GameStateEnum processInput(GameTime gameTime)
        {
            keyboardInput.Update(gameTime); // Update keyboard input system


            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                return GameStateEnum.MainMenu;
            }


            if (currentState == SettingsState.Changing)
            {

                var keys = keyboardState.GetPressedKeys();


                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    return GameStateEnum.MainMenu;
                }

                foreach (var key in keys)
                {
                    if (key != Keys.Up && key != Keys.Down && key != Keys.Enter && key != Keys.Escape)
                    {
                        keyBindings[changingControl] = key;
                        currentState = SettingsState.Viewing;
                        changingControl = "";
                        break;
                    }
                }
            }

            return GameStateEnum.Settings;
        }

        private void NavigateUp(GameTime gameTime, float value)
        {
            if (currentState == SettingsState.Viewing)
            {
                selectedControlIndex--;
                if (selectedControlIndex < 0) selectedControlIndex = controlNames.Count - 1;
            }
        }

        private void NavigateDown(GameTime gameTime, float value)
        {
            if (currentState == SettingsState.Viewing)
            {
                selectedControlIndex++;
                if (selectedControlIndex >= controlNames.Count) selectedControlIndex = 0;
            }
        }

        private void SelectControl(GameTime gameTime, float value)
        {
            if (currentState == SettingsState.Viewing)
            {
                currentState = SettingsState.Changing;
                changingControl = controlNames[selectedControlIndex];
            }

        }




        public override void render(GameTime gameTime)
        {
            m_spriteBatch.Begin();
            int totalTextHeight = controlNames.Count * 100;
            Vector2 position = new Vector2(m_graphics.PreferredBackBufferWidth / 3, (m_graphics.PreferredBackBufferHeight - totalTextHeight) / 2);


            for (int i = 0; i < controlNames.Count; i++)
            {
                Color textColor = Color.White;
                string text = $"{controlNames[i]}: {keyBindings[controlNames[i]]}";

                // Change text color to red if it's the currently selected control for editing
                if (i == selectedControlIndex && currentState == SettingsState.Viewing)
                {
                    textColor = Color.Red;
                }
                else if (changingControl == controlNames[i] && currentState == SettingsState.Changing)
                {
                    text += " (press new key)";
                    textColor = Color.Red; //ake it red if it's currently being changed
                }

                m_spriteBatch.DrawString(m_font, text, position, textColor);
                position.Y += 100; 
            }

            Vector2 stringSize = m_font.MeasureString(MESSAGE);
            m_spriteBatch.DrawString(m_font2, MESSAGE, new Vector2(m_graphics.PreferredBackBufferWidth - stringSize.X / 2, m_graphics.PreferredBackBufferHeight - 100), Color.Yellow);

            m_spriteBatch.End();
        }

        public override void update(GameTime gameTime)
        {
            
        }
    }
}