using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;

namespace HoS_proto
{
    public class Player : Acter
    {
        public enum State
        {
            MOVING, TALKING
        }
        enum Has
        {
            SPOKEN, WALKED
        }
        Dictionary<Has, bool> Done
        {
            get
            {
                if (__backing_field_for_Done == null)
                {
                    __backing_field_for_Done = new Dictionary<Has, bool>();
                    foreach (var key in typeof(Has).GetEnumValues())
                    {
                        __backing_field_for_Done[(Has)key] = false;
                    }
                }
                return __backing_field_for_Done;
            }
        }

        #region fields
        public static Player Instance { get; private set; }
        Timer timeSinceMovement = new Timer(1f / 60f * 3000f);
        bool moveDelayElapsed = true;
        public State state;

        KeyboardState kbs, old_kbs;
        bool Pressed(Keys k) { return kbs.IsKeyDown(k) && old_kbs.IsKeyUp(k); }
        public bool Pausing { get; private set; }

        Dictionary<Has, bool> __backing_field_for_Done;
        #endregion

        public Player(int x, int y)
        {
            Instance = this;
            Location = new Point(x, y);
            timeSinceMovement.AutoReset = false;
            timeSinceMovement.Elapsed += (_, __) => moveDelayElapsed = true;
            spritePath = "dd_tinker";
            Pausing = true;
            Quirks = Quirk.TIGHT_LIPPED;
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
                        if (name.Length > 0) name = name.Remove(name.Length - 2);
                        break;
                    case Keys.Enter:
                        Pausing = false;
                        break;
                    default:
                        var c = key.ToString();
                        if (name.Length > 1) c = c.ToLower();
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
                Done[Has.WALKED] = true;
                moveDelayElapsed = false;
                timeSinceMovement.Start();
                return true;
            }
        }
        #endregion

        #region state machine
        bool Enter(State nextState)
        {
            if (state == nextState) return false;

            switch (nextState)
            {
                case State.MOVING:
                    if (!Done[Has.WALKED])
                    {
                        if (textBubble == null) MakeTextBubble();
                        textBubble.Add("use direction keys, numpad, or vi keys to walk.");
                    }
                    break;

                case State.TALKING:
                    MakeTextBubble().Add("Ask", () =>
                    {
                        var subject = NPC.Instance.LastInteraction(this);
                        if (subject)
                        {
                            Query(NPC.Instance, Interaction.Atom.MUTUAL_HISTORY);
                        }
                        else
                        {
                            Query(NPC.Instance, Interaction.Atom.NOTHING);
                        }
                    });
                    
                    Done[Has.SPOKEN] = true;
                    break;
            }
            state = nextState;
            return true;
        }
        bool ExitState()
        {
            switch (state)
            {
                case State.MOVING:
                    textBubble = null;
                    break;
                case State.TALKING:
                    textBubble = null;
                    break;
            }
            return true;
        }

        public override void Update()
        {
            old_kbs = kbs;
            kbs = Keyboard.GetState();

            switch (state)
            {
                case State.MOVING:
                    if (NPC.Instance.isInRange(this))
                    {
                        if (!Done[Has.SPOKEN]) MakeTextBubble().Add("press space bar to talk");

                        if (Pressed(Keys.Space))
                        {
                            ExitState();
                            Enter(State.TALKING);
                            return;
                        }
                    }
                    else textBubble = null;

                    Move();
                    break;

                case State.TALKING:
                    if ((Direction() & UP) != 0) textBubble.GoPrev();
                    else if ((Direction() & DOWN) != 0) textBubble.GoNext();

                    if (Pressed(Keys.Enter) || Pressed(Keys.Space)) textBubble.Select();
                    break;
            }
        }
        #endregion
    }
}
