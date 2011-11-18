using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
                blockMove = true;
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
    }
}
