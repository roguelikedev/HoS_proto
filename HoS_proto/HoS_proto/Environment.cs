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
using System.Diagnostics;

namespace HoS_proto
{
    public class Environment
    {
        public const string GRASS = "grass",
                            DIRT  = "dirt"
                            ;
        static Dictionary<Point, Environment> all = new Dictionary<Point, Environment>();

        int x, y;
        string type;

        public Environment(int x, int y, string type)
        {
            this.x = x; this.y = y; this.type = type;
            all[new Point(x, y)] = this;
        }

        void Draw()
        {
            Engine.Draw(type, x, y);
        }

        public static void DrawAll()
        {
            foreach (var curr in all.Values) curr.Draw();
        }
    }
}
