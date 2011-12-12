using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using System.Diagnostics;
using Util;

namespace HoS_proto
{
    public class Player : Person
    {
        public enum State
        {
            UNINITIALIZED, MOVING, TALKING
        }

        #region fields
        public static Player Instance { get; private set; }
        Timer timeSinceMovement = new Timer(1f / 60f * 3000f);
        bool moveDelayElapsed = true;
        State state = State.UNINITIALIZED;

        KeyboardState kbs, old_kbs;
        bool Pressed(Keys k) { return kbs.IsKeyDown(k) && old_kbs.IsKeyUp(k); }
        public bool Pausing { get; private set; }

        List<Notification> popups = new List<Notification>();
        #endregion

        public Player(int x, int y, Act.Controller ac) : base (x, y, ac)
        {
            Instance = this;
            timeSinceMovement.AutoReset = false;
            timeSinceMovement.Elapsed += (_, __) => moveDelayElapsed = true;
            spritePath = "dd_tinker";
            Pausing = true;
            Quirks = Quirk.TIGHT_LIPPED;
            name = "man";
        }
        public void GetName()
        {
            old_kbs = kbs;
            kbs = Keyboard.GetState();

            foreach (var key in new List<Keys>(Keyboard.GetState().GetPressedKeys()).FindAll(k => Pressed(k)))
            {
                switch (key)
                {
                    case Keys.Back:
                        if (name.Length > 0) name = name.Remove(name.Length - 1);
                        break;
                    case Keys.Enter:
                        Pausing = false;
                        break;
                    default:
                        if ((char)key > 'z' || (char)key < 'A') continue;

                        var c = key.ToString();
                        if (name.Length > 0) c = c.ToLower();
                        name += c;
                        break;
                }
            }
        }

        #region movement
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
            if (!moveDelayElapsed) return false;

            int moveDir = Direction();

            Point prevLoc = Location;

            if ((moveDir & LEFT) != 0) X -= 1;
            if ((moveDir & RIGHT) != 0) X += 1;
            if ((moveDir & UP) != 0) Y -= 1;
            if ((moveDir & DOWN) != 0) Y += 1;

            if (Environment.At(Location).blockMove) Location = prevLoc;

            if (Location == prevLoc) return false;
            else
            {
                quests.RemoveAll(a => a.verb == Verb.GO && a.primaryObject == Noun.NOTHING);
                textBubble = null;
                moveDelayElapsed = false;
                timeSinceMovement.Start();
                return true;
            }
        }
        #endregion

        bool Enter(State nextState)
        {
            switch (nextState)
            {
                case State.MOVING:
                    textBubble = null;
                    if (quests.Exists(a => a.verb == Verb.GO && a.primaryObject == Noun.NOTHING))
                    {
                        MakeTextBubble().Add("use direction keys, numpad, or vi keys to walk.");
                    }
                    break;

                case State.TALKING:
                    MakeTextBubble();
                    {
                        var prevStatement = LastInteraction(Listener);
                        if (prevStatement) textBubble.Add(prevStatement);
                    }

                    textBubble.Add("Ask", () =>
                    {
                        Query(Listener, Noun.NOTHING, Listener.LastInteraction(this));
                    }, Color.Yellow)
                    .Add("OK", () => Respond(Listener, true), Color.Green)
                    .Add("No", () => Respond(Listener, false), Color.Red)
                    .Add("Bye", () => Enter(State.MOVING), Color.Red)
                    ;

                    textBubble.GoNext();
                    quests.RemoveAll(a => a.verb == Verb.TALK && a.primaryObject == Listener);
                    break;
            }
            state = nextState;
            return true;
        }

        public override void Update()
        {
            base.Update();
            old_kbs = kbs;
            kbs = Keyboard.GetState();

            var nearestNPC = actController.ClosestPerson(this);
            switch (state)
            {
                case State.UNINITIALIZED:
                    quests.Add(actController.FirstCause(this, Verb.GO, Noun.NOTHING));
                    quests.Add(actController.FirstCause(this, Verb.TALK, nearestNPC));
                    Enter(State.MOVING);
                    break;

                case State.MOVING:
                    if (Adjacent(nearestNPC))
                    {
                        if (quests.Exists(a => a.verb == Verb.TALK && a.primaryObject == nearestNPC))
                        {
                            MakeTextBubble().Add("press space bar to talk");
                        }
                        if (Pressed(Keys.Space))
                        {
                            Enter(State.TALKING);
                            return;
                        }
                    }

                    Move();
                    break;

                case State.TALKING:
                    if ((Direction() & UP) != 0) textBubble.GoPrev();
                    else if ((Direction() & DOWN) != 0) textBubble.GoNext();

                    if (Pressed(Keys.Enter) || Pressed(Keys.Space))
                    {
                        textBubble.InvokeCurrent();
                        Enter(state);
                    }
                    break;
            }
        }

        public override void Draw()
        {
            if (quests.Count == 0) goto LAST_LINE;

            var q = quests[0];
            if (q.Happened)
            {
                popups.Add(new Notification("YOU DID SOMETHING! GJ", 13, 13));
                quests.Remove(q);
            }
            else
            {
                if (Engine.OnScreen(q.primaryObject.Location))
                {
                    if (q.verb == Verb.GO) Engine.DrawAtWorld("halo", q.primaryObject.Location.X, q.primaryObject.Location.Y);
                    goto LAST_LINE;
                }

                var dir = new Vector2(q.primaryObject.Location.X - X, q.primaryObject.Location.Y - Y);
                Debug.Assert(dir != Vector2.Zero);
                dir.Normalize();
                var rot = (float)Math.Asin(dir.X) + MathHelper.PiOver2;
                if (dir.Y < 0) rot *= -1;
                rot = MathHelper.TwoPi - rot;

                dir *= Engine.SCREEN_WIDTH_PX / 3;
                dir += new Vector2(Engine.SCREEN_WIDTH_PX / 2);

                Engine.DrawAtScreen("arrow", (int)dir.X, (int)dir.Y, Engine.TILE_DIM_IN_PX, Engine.TILE_DIM_IN_PX
                                   , Color.Gold, rot);
            }

        LAST_LINE:
            popups.ForEach(n =>
            {
                n.Draw();
                if (!n.Visible) popups.Remove(n);
            });
            base.Draw();
        }
    }
}
