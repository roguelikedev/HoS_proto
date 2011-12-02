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
        public static readonly Point WORLD_DIM = new Point(24, 24);

        static Dictionary<Point, Environment> all = new Dictionary<Point, Environment>();
        public static Environment At(Point p)
        {
            if (all.ContainsKey(p)) return all[p];
            else return NOTHING;
        }
        public static Environment At(int x, int y) { return At(new Point(x, y)); }

        int x, y;
        public Point Location { get { return new Point(x, y); } }
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
            #region Lambdas, declarations
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

            var screenCenter = GridToPx(-5, -5);
            Action<Vector2> AddVert = v => Engine.triDrawer.AddVertex(v - screenCenter);

            var playerCenter = Center(Player.Instance.X, Player.Instance.Y);
            screenCenter += GridToPx(Player.Instance.X, Player.Instance.Y);
            var screenSize = new Vector2(Engine.SCREEN_DIM_IN_TILES * Engine.TILE_DIM_IN_PX);
            #endregion

            foreach (var curr in new List<Environment>(all.Values).FindAll(e => e.blockSight && Engine.OnScreen(e.Location)))
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

                    AddVert(top);
                    var _1 = top + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    AddVert(_1);
                    AddVert(bottom);

                    run = bottom.X - playerCenter.X;
                    rise = bottom.Y - playerCenter.Y;

                    AddVert(top);
                    var _2 = bottom + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    AddVert(_2);
                    AddVert(bottom);

                    AddVert(top + Vector2.UnitY / 2);
                    AddVert(_1);
                    AddVert(_2);
                }
                else
                {
                    top = tl; bottom = bl;

                    run = top.X - playerCenter.X;
                    rise = top.Y - playerCenter.Y;

                    var _1 = top + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    AddVert(_1);
                    AddVert(top);
                    AddVert(bottom);

                    run = bottom.X - playerCenter.X;
                    rise = bottom.Y - playerCenter.Y;

                    var _2 = bottom + new Vector2(run, rise) * new Vector2(Engine.SCREEN_DIM_IN_TILES);
                    AddVert(_2);
                    AddVert(top);
                    AddVert(bottom);

                    AddVert(_1);
                    AddVert(top + Vector2.UnitY / 2);
                    AddVert(_2);
                }
            }
        }
    }
}
