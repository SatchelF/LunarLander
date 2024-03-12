using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CS5410
{
    public class GameStateDemo : Game
    {
        private GraphicsDeviceManager m_graphics;
        private IGameState m_currentState;
        private Dictionary<GameStateEnum, IGameState> m_states;

        public GameStateDemo()
        {
            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            m_graphics.PreferredBackBufferWidth = 1920;
            m_graphics.PreferredBackBufferHeight = 1080;

            m_graphics.ApplyChanges();

            // Create all the game states here
            m_states = new Dictionary<GameStateEnum, IGameState>
            {
                { GameStateEnum.MainMenu, new MainMenuView() },
                { GameStateEnum.GamePlay, new GamePlayView() },
                { GameStateEnum.HighScores, new HighScoresView() },
                { GameStateEnum.Settings, new SettingsView() },
                { GameStateEnum.Credits, new CreditsView() }
            };

            // Give all game states a chance to initialize, other than constructor
            foreach (var item in m_states)
            {
                item.Value.initialize(this.GraphicsDevice, m_graphics);
            }

            // We are starting with the main menu
            m_currentState = m_states[GameStateEnum.MainMenu];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Give all game states a chance to load their content
            foreach (var item in m_states)
            {
                item.Value.loadContent(this.Content);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            GameStateEnum nextStateEnum = m_currentState.processInput(gameTime);

            // Check if we are switching to the GamePlay state from another state
            if (nextStateEnum == GameStateEnum.GamePlay && m_currentState != m_states[GameStateEnum.GamePlay])
            {
                // Reset the GamePlay state
                GamePlayView gamePlayView = m_states[GameStateEnum.GamePlay] as GamePlayView;
                gamePlayView.loadContent(this.Content);
                // The above line will internally call InitializeNewGame method of GamePlayView
            }

            // Special case for exiting the game
            if (nextStateEnum == GameStateEnum.Exit)
            {
                Exit();
            }
            else if (m_currentState != m_states[nextStateEnum])
            {
                // Update current state
                m_currentState = m_states[nextStateEnum];
            }

            // Always update the current state regardless of any transitions
            m_currentState.update(gameTime);

            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            m_currentState.render(gameTime);

            base.Draw(gameTime);
        }
    }
}
