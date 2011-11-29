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
using Util;

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
        SpriteFont font;
        public static SpriteFont Font { get { return instance.font; } }
        public static TriangleDrawer triDrawer;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            instance = this;
            graphics.PreferredBackBufferHeight = TILE_DIM_IN_PX * SCREEN_DIM_IN_TILES;
            graphics.PreferredBackBufferWidth = TILE_DIM_IN_PX * SCREEN_DIM_IN_TILES;
        }
        
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            triDrawer = new TriangleDrawer(GraphicsDevice);

            font = Content.Load<SpriteFont>("SpriteFont1");
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
            new NPC(rand.Next(12), rand.Next(12));
            NPC.Instance.addOption("Talk.");
            NPC.Instance.addOption("Kick.");
            NPC.Instance.addOption("Punch.");
            NPC.Instance.addOption("Kiss.");
            {
                int x = -1, y = -1;
                while (Environment.At(new Point(x, y)).blockMove)
                {
                    x = rand.Next(SCREEN_DIM_IN_TILES);
                    y = rand.Next(SCREEN_DIM_IN_TILES);
                }
                new Player(x, y);
            }
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
            NPC.Instance.Draw();

            spriteBatch.End();

            triDrawer.Begin();
            Environment.DrawShadows();
            triDrawer.End();
        }

        #region view helpers
        public static Point ToScreen(Point worldRelative)
        {
            worldRelative.X -= Player.Instance.X;
            worldRelative.X += SCREEN_DIM_IN_TILES / 2;
            worldRelative.X *= TILE_DIM_IN_PX;
            worldRelative.Y -= Player.Instance.Y;
            worldRelative.Y += SCREEN_DIM_IN_TILES / 2;
            worldRelative.Y *= TILE_DIM_IN_PX;

            return worldRelative;
        }
        public static Point ToScreen(int x, int y) { return ToScreen(new Point(x, y)); }

        public static void DrawAtWorld(string what, int x, int y)
        {
            var where = ToScreen(x, y);
            DrawAtScreen(what, where.X, where.Y, TILE_DIM_IN_PX, TILE_DIM_IN_PX);
        }
        public static void DrawAtScreen(string what, int x, int y, int w, int h)
        {
            DrawAtScreen(what, x, y, w, h, Color.White);
        }
        public static void DrawAtScreen(string what, int x, int y, int w, int h, Color color)
        {
            if (what == null) return;

            instance.spriteBatch.Draw(instance.Content.Load<Texture2D>(what)
                                    , new Rectangle(x, y, w, h)
                                    , color);
        }
        public static void WriteAtWorld(string what, int x, int y, int size)
        {
            var where = ToScreen(x, y);
            WriteAtScreen(what, where.X, where.Y, size);
        }
        public static void WriteAtScreen(string what, int x, int y, int size)
        {
            instance.spriteBatch.DrawString(instance.font, what, new Vector2(x, y), Color.Black
                , 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        #endregion
    }
}
