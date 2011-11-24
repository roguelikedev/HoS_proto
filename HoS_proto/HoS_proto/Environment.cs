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
            Func<int, int, Vector2> Center = (x, y) =>
            {
                var rval = new Vector2(x * Engine.TILE_DIM_IN_PX, y * Engine.TILE_DIM_IN_PX);
                rval += new Vector2(Engine.TILE_DIM_IN_PX / 2);
                return rval;
            };
            foreach (var curr in new List<Environment>(all.Values).FindAll(e => e.blockSight))
            {
                Engine.triDrawer.AddVertex(Center(curr.x, curr.y));
                var playerCenter = Center(Player.Instance.X, Player.Instance.Y);
                Engine.triDrawer.AddVertex(new Vector2(1, 1));
                Engine.triDrawer.AddVertex(new Vector2(320, 0));
            }
        }
    }
}
