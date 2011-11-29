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
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public class Player
    {
        public enum State
        {
            MOVING, MENU
        }

        public static Player Instance { get; private set; }
        public Point Location { get; private set; }
        public int X { get { return Location.X; } private set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } private set { Location = new Point(Location.X, value); } }
        Timer timeSinceMovement = new Timer(1f / 60f * 3000f);
        bool moveDelayElapsed = true;
        public State state;
        Menu activeMenu;

        public Player(int x, int y)
        {
            Instance = this;
            Location = new Point(x, y);
            timeSinceMovement.AutoReset = false;
            timeSinceMovement.Elapsed += (_, __) => moveDelayElapsed = true;
        }

        const int   STAY    = 0,
                    LEFT    = 1,
                    RIGHT   = 1 << 1,
                    UP      = 1 << 2,
                    DOWN    = 1 << 3
                    ;

        int Direction()
        {
            int rval = STAY;

            foreach (var key in Keyboard.GetState().GetPressedKeys())
            {
                switch (key)
                {
                    case Keys.Left:
                    case Keys.H:
                        rval |= LEFT;
                        break;
                    case Keys.Down:
                    case Keys.J:
                        rval |= DOWN;
                        break;
                    case Keys.Up:
                    case Keys.K:
                        rval |= UP;
                        break;
                    case Keys.Right:
                    case Keys.L:
                        rval |= RIGHT;
                        break;
                    case Keys.Y:
                        rval |= LEFT | UP;
                        break;
                    case Keys.U:
                        rval |= RIGHT | UP;
                        break;
                    case Keys.B:
                        rval |= LEFT | DOWN;
                        break;
                    case Keys.N:
                        rval |= RIGHT | DOWN;
                        break;
                }
            }

            Action<int, int> CancelOpposites = (dirA, dirB) =>
            {
                if ((rval & dirA & dirB) != 0) rval &= ~(dirA | dirB);
            };

            CancelOpposites(LEFT, RIGHT);
            CancelOpposites(UP, DOWN);

            return rval;
        }

        bool Move()
        {
            int moveDir = Direction();

            Point prevLoc = Location;

            if ((moveDir & LEFT) != 0) X -= 1;
            if ((moveDir & RIGHT) != 0) X += 1;
            if ((moveDir & UP) != 0) Y -= 1;
            if ((moveDir & DOWN) != 0) Y += 1;

            if (Environment.At(Location).blockMove) Location = prevLoc;

            return Location != prevLoc;
        }

        public void Update(GameTime gt)
        {
            if (state == State.MOVING)
            {


                if (moveDelayElapsed && Move())
                {
                    moveDelayElapsed = false;
                    timeSinceMovement.Start();
                }
            }
            else if (state == State.MENU)
            {
                Debug.Assert(activeMenu != null);

                if ((Direction() & UP) != 0) activeMenu.GoPrev();
                else if ((Direction() & DOWN) != 0) activeMenu.GoNext();

                if (Keyboard.GetState().IsKeyDown(Keys.Enter)) activeMenu.Select();
            }
        }

        public void Draw()
        {
            Engine.DrawAtWorld("dd_tinker", X, Y);

            if (NPC.Instance.isInRange(this))
            {
                Action<string, int> Say = (str, ndx) => Engine.WriteAtWorld(str,
                            NPC.Instance.Location.X + 1,
                            NPC.Instance.Location.Y + ndx, 1);

                if (Keyboard.GetState().IsKeyDown(Keys.T))
                {
                    for (int i = 1; i <= NPC.Instance.Options.Count; i++)
                    {
                        Say(i + "." + NPC.Instance.Options[i - 1], i);
                    }
                }
                else
                {
                    Say("Press T to talk.", 0);
                }
            }

        }
    }
}
