using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace HoS_proto
{
    public class Environment
    {
        public static Environment NOTHING { get; private set; }
        static Environment()
        {
            NOTHING = new Environment(-1, -1, null);
        }

        public const string GRASS = "grass",
                            DIRT  = "dirt",
                            ROCK  = "rock"
                            ;
        static Dictionary<Point, Environment> all = new Dictionary<Point, Environment>();
        public static Environment At(Point p)
        {
            if (all.ContainsKey(p)) return all[p];
            else return NOTHING;
        }

        int x, y;
        string type;
        public readonly bool blockMove, blockSight;
        Environment ground;

        public Environment(int x, int y, string type)
        {
            this.x = x; this.y = y; this.type = type;
            Point where = new Point(x, y);
            if (type == ROCK)
            {
                if (all.ContainsKey(where) && all[where].type != ROCK) ground = all[where];
                else ground = new Environment(x, y, Engine.rand.Next(2) == 0 ? GRASS : DIRT);
                blockMove = blockSight = true;
            }
            if (type == null) blockMove = true;

            all[where] = this;
        }

        void Draw()
        {
            if (ground != null) ground.Draw();
            Engine.Draw(type, x, y);
        }

        public static void DrawAll()
        {
            foreach (var curr in all.Values) curr.Draw();
        }

        public static void DrawShadows()
        {
            #region Lambdas
            Func<int, int, Vector2> GridToPx = (x, y) =>
            {
                return new Vector2(x * Engine.TILE_DIM_IN_PX, y * Engine.TILE_DIM_IN_PX);
            };
            Func<int, int, Vector2> Center = (x, y) =>
            {
                var rval = GridToPx(x, y);
                rval += new Vector2(Engine.TILE_DIM_IN_PX / 2);
                return rval;
            };
            #endregion

            var screenCenter = Center(-6, -6);
            var playerCenter = Center(Player.Instance.X, Player.Instance.Y);
            screenCenter += playerCenter;

            foreach (var curr in new List<Environment>(all.Values).FindAll(e => e.blockSight))
            {
                var tr = GridToPx(curr.x + 1, curr.y);
                var tl = GridToPx(curr.x, curr.y);
                var br = GridToPx(curr.x + 1, curr.y + 1);
                var bl = GridToPx(curr.x, curr.y + 1);

                if (Player.Instance.X < curr.x && Player.Instance.Y < curr.y)
                {
                    Engine.triDrawer.AddVertex(Center(curr.x, curr.y) - screenCenter);

                    var screenSize = new Vector2(Engine.SCREEN_DIM_IN_TILES * Engine.TILE_DIM_IN_PX);
                    var run = playerCenter.X - tr.X;
                    var rise = playerCenter.Y - tr.Y;

                    Engine.triDrawer.AddVertex(new Vector2(screenSize.X, tr.Y + rise / run * (screenSize.X - tr.X)));

                    Engine.triDrawer.AddVertex(screenSize);
                }
            }
        }
    }
}
