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

        KeyboardState kbs, old_kbs;
        bool Pressed(Keys k) { return kbs.IsKeyDown(k) && old_kbs.IsKeyUp(k); }

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

            foreach (var key in new List<Keys>(Keyboard.GetState().GetPressedKeys()).FindAll(k => Pressed(k)))
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
                    case Keys.Home:
                        rval |= LEFT | UP;
                        break;
                    case Keys.U:
                    case Keys.PageUp:
                        rval |= RIGHT | UP;
                        break;
                    case Keys.B:
                    case Keys.End:
                        rval |= LEFT | DOWN;
                        break;
                    case Keys.N:
                    case Keys.PageDown:
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
            old_kbs = kbs;
            kbs = Keyboard.GetState();

            if (state == State.MOVING)
            {
                if (NPC.Instance.isInRange(this))
                {
                    Action AssignMenu = () =>
                    {
                        activeMenu = new Menu();
                        activeMenu.DrawBox = () =>
                        {
                            var origin = Engine.ToScreen(NPC.Instance.Location);
                            return new Rectangle(origin.X, origin.Y, Engine.TILE_DIM_IN_PX, Engine.TILE_DIM_IN_PX);
                        };
                    };

                    if (activeMenu == null)
                    {
                        AssignMenu();
                        activeMenu.Add("We need to [T]alk.", Constants.NO_OP);
                    }
                    if (Pressed(Keys.T))
                    {
                        state = State.MENU;
                        AssignMenu();
                        activeMenu.Add("Goto hell!", () => {
                            this.activeMenu = null;
                            this.state = State.MOVING;
                        });
                        activeMenu.Add("Talk about what?", () => activeMenu.Add("BARF", Constants.NO_OP));
                        return;
                    }
                }
                else activeMenu = null;

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

                if (Pressed(Keys.Enter)) activeMenu.Select();
            }
        }

        public void Draw()
        {
            Engine.DrawAtWorld("dd_tinker", X, Y);

            if (activeMenu != null)
            {
                activeMenu.Draw(0, 0);
            }
        }
    }
}
