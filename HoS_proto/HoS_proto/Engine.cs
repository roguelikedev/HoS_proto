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
        public const int TILE_DIM_IN_PX = 64;
        public const int SCREEN_DIM_IN_TILES = 11;

        public static Random rand = new Random();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            instance = this;
            graphics.PreferredBackBufferHeight = TILE_DIM_IN_PX * 11;
            graphics.PreferredBackBufferWidth = TILE_DIM_IN_PX * 15;
        }
        
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            for (int x = -1; ++x < 11; ) for (int y = -1; ++y < 11; )
                {
                    string type = null;
                    switch (rand.Next(1, 5) + rand.Next(1, 5))
                    {
                        case 2:
                        case 3:
                        case 4:
                            type = Environment.DIRT;
                            break;
                        case 5:
                            type = Environment.ROCK;
                            break;
                        case 6:
                        case 7:
                        case 8:
                            type = Environment.GRASS;
                            break;
                        default:
                            throw new Exception("BARF!!!!");
                    }
                    new Environment(x, y, type);
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
            if (what == null) return;

            x -= Player.Instance.X;
            x += SCREEN_DIM_IN_TILES / 2;
            y -= Player.Instance.Y;
            y += SCREEN_DIM_IN_TILES / 2;

            instance.spriteBatch.Draw(instance.Content.Load<Texture2D>(what)
                                    , new Rectangle(x * TILE_DIM_IN_PX, y * TILE_DIM_IN_PX, TILE_DIM_IN_PX, TILE_DIM_IN_PX)
                                    , Color.White);
        }
    }
}
