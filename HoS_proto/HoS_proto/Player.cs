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
    public class Player
    {
        public static Player Instance { get; private set; }
        public Point Location { get; private set; }
        public int X { get { return Location.X; } private set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } private set { Location = new Point(Location.X, value); } }

        public Player(int x, int y)
        {
            Instance = this;
            Location = new Point(x, y);
        }

        const int   STAY    = 0,
                    LEFT    = 1,
                    RIGHT   = 1 << 1,
                    UP      = 1 << 2,
                    DOWN    = 1 << 3
                    ;

        public void Update()
        {
            int moveDir = STAY;

            foreach (var key in Keyboard.GetState().GetPressedKeys())
            {
                switch (key)
                {
                    case Keys.H:
                        moveDir |= LEFT;
                        break;
                    case Keys.J:
                        moveDir |= DOWN;
                        break;
                    case Keys.K:
                        moveDir |= UP;
                        break;
                    case Keys.L:
                        moveDir |= RIGHT;
                        break;
                    case Keys.Y:
                        moveDir |= LEFT | UP;
                        break;
                    case Keys.U:
                        moveDir |= RIGHT | UP;
                        break;
                    case Keys.B:
                        moveDir |= LEFT | DOWN;
                        break;
                    case Keys.N:
                        moveDir |= RIGHT | DOWN;
                        break;
                }
            }

            Action<int, int> CancelOpposites = (dirA, dirB) =>
            {
                if ((moveDir & dirA & dirB) != 0) moveDir &= ~(dirA | dirB);
            };

            CancelOpposites(LEFT, RIGHT);
            CancelOpposites(UP, DOWN);

            if ((moveDir & LEFT) != 0) X -= 1;
            if ((moveDir & RIGHT) != 0) X += 1;
            if ((moveDir & UP) != 0) Y -= 1;
            if ((moveDir & DOWN) != 0) Y += 1;
        }

        public void Draw()
        {
            Engine.Draw("dd_tinker", X, Y);
        }
    }
}
