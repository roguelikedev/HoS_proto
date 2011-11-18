using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HoS_proto
{
    public class Engine : Microsoft.Xna.Framework.Game
    {
        static Engine instance;
        public const int TILE_DIM = 64;

        public Random rand = new Random();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            instance = this;
            graphics.PreferredBackBufferHeight = TILE_DIM * 11;
            graphics.PreferredBackBufferWidth = TILE_DIM * 15;
        }
        
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            for (int x = -1; ++x < 11; ) for (int y = -1; ++y < 11; )
                {
                    new Environment(x, y, rand.Next(2) == 0 ? Environment.DIRT : Environment.GRASS);
                }

            new Player(rand.Next(12), rand.Next(12));
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            Player.Instance.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            Environment.DrawAll();
            Player.Instance.Draw();

            spriteBatch.End();
        }

        public static void Draw(string what, int x, int y)
        {
            instance.spriteBatch.Draw(instance.Content.Load<Texture2D>(what)
                                    , new Rectangle(x * TILE_DIM, y * TILE_DIM, TILE_DIM, TILE_DIM)
                                    , Color.White);
        }
    }
}
