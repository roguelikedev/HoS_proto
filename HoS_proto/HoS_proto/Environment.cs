using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

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
            Engine.DrawAtWorld(type, x, y);
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

            var screenCenter = GridToPx(-5, -5);
            var playerCenter = Center(Player.Instance.X, Player.Instance.Y);
            screenCenter += GridToPx(Player.Instance.X, Player.Instance.Y);
            var screenSize = new Vector2(Engine.SCREEN_DIM_IN_TILES * Engine.TILE_DIM_IN_PX);

            foreach (var curr in new List<Environment>(all.Values).FindAll(e => e.blockSight))
            {
                Vector2 top, bottom;
                float rise = 0, run = 0;

                var tr = GridToPx(curr.x + 1, curr.y);
                var tl = GridToPx(curr.x, curr.y);
                var br = GridToPx(curr.x + 1, curr.y + 1);
                var bl = GridToPx(curr.x, curr.y + 1);

                if (tl.X > Player.Instance.X * Engine.TILE_DIM_IN_PX)
                {
                    top = tr; bottom = br;

                    run = top.X - playerCenter.X;
                    rise = top.Y - playerCenter.Y;

                    Engine.triDrawer.AddVertex(top - screenCenter);
                    var _1 = top - screenCenter + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    Engine.triDrawer.AddVertex(_1);
                    Engine.triDrawer.AddVertex(bottom - screenCenter);

                    run = bottom.X - playerCenter.X;
                    rise = bottom.Y - playerCenter.Y;

                    Engine.triDrawer.AddVertex(top - screenCenter);
                    var _2 = bottom - screenCenter + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    Engine.triDrawer.AddVertex(_2);
                    Engine.triDrawer.AddVertex(bottom - screenCenter);

                    Engine.triDrawer.AddVertex(top - screenCenter + Vector2.UnitY / 2);
                    Engine.triDrawer.AddVertex(_1);
                    Engine.triDrawer.AddVertex(_2);
                }
                else
                {
                    run = tl.X - playerCenter.X;
                    rise = tl.Y - playerCenter.Y;

                    var _1 = tl - screenCenter + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    Engine.triDrawer.AddVertex(_1);
                    Engine.triDrawer.AddVertex(tl - screenCenter);
                    Engine.triDrawer.AddVertex(bl - screenCenter);

                    run = bl.X - playerCenter.X;
                    rise = bl.Y - playerCenter.Y;

                    var _2 = bl - screenCenter + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    Engine.triDrawer.AddVertex(_2);
                    Engine.triDrawer.AddVertex(tl - screenCenter);
                    Engine.triDrawer.AddVertex(bl - screenCenter);

                    Engine.triDrawer.AddVertex(_1);
                    Engine.triDrawer.AddVertex(tl - screenCenter + Vector2.UnitY / 2);
                    Engine.triDrawer.AddVertex(_2);
                }
            }
        }
    }
}
