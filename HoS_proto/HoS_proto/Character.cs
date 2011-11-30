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
    public class Quirk
    {
        #region squish
        public const int
            VERY                = 1,
            CASUAL              = 1 << 1,
            TIGHT_LIPPED        = 1 << 2,
            GENEROUS            = 1 << 3,
            EGOTISTICAL         = 1 << 4,
            OUTGOING            = 1 << 5
            ;
        int value;
        Quirk(int v) { value = v; }
        public static implicit operator int(Quirk q) { return q.value; }
        public static implicit operator Quirk(int i) { return new Quirk(i); }
        public static implicit operator bool(Quirk self) { return self.value != 0; }
        public static Quirk operator &(Quirk a, Quirk b) { return a.value & b.value; }
        public static bool operator ==(Quirk _this, Quirk that) { return _this.value == that.value; }
        #region shut up, compiler
        public static bool operator !=(Quirk _this, Quirk that) { return _this.value != that.value; }
        public override bool Equals(object obj)
        {
            if (obj is Quirk) return this == obj as Quirk;
            return base.Equals(obj);
        }
        public override int GetHashCode() { return value.GetHashCode(); }
        #endregion
        #endregion
    }

    public abstract class Acter
    {
        #region fields
        public Point Location { get; protected set; }
        public int X { get { return Location.X; } protected set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } protected set { Location = new Point(Location.X, value); } }

        protected string name;
        public override string ToString() { return name == null ? "man" : name; }
        protected string spritePath;
        protected Menu textBubble;

        public Acter Interactee { get; private set; }
        public Quirk Quirks { get; private set; }
        List<Interaction> memory = new List<Interaction>();
        bool AmbiguousListener
        {
            get
            {
                if (memory.Count < 2) return true;
                return memory.Last().receiver != memory[memory.Count - 2].receiver;
            }
        }
        #endregion

        #region view
        public virtual void Draw() 
        {
            Engine.DrawAtWorld(spritePath, X, Y);
            if (textBubble != null) textBubble.Draw();
        }

        protected Menu MakeTextBubble()
        {
            textBubble = new Menu();
            textBubble.DrawBox = () =>
            {
                var origin = Location;
                var rval = new Rectangle(origin.X, origin.Y, Menu.FLEXIBLE, Menu.FLEXIBLE);

                #region if conversation partner's standing to my right, speech bubble goes left.
                if (Interactee != null && Interactee.X > X && Interactee.X < X + 3)
                #endregion
                {
                    origin = Engine.ToScreen(origin);
                    rval.X = Menu.FLEXIBLE;
                    rval.Width = origin.X; // field named Width is used as a coordinate here.
                    rval.Y = origin.Y;
                }
                else
                {
                    origin.X++; // single tile offset to avoid hiding self.
                    rval.Location = Engine.ToScreen(origin);
                }

                return rval;
            };
            return textBubble;
        }
        #endregion

        public abstract void Update();

        #region object oriented overhead
        static List<Acter> all = new List<Acter>();
        protected Acter()
        {
            Quirks = Quirk.CASUAL;
            all.Add(this);
        }
        public static void UpdateAll() { all.ForEach(a => a.Update()); }
        public static implicit operator bool(Acter who) { return who != null; }
        public static implicit operator string(Acter who) { return who ? who.ToString() : "no one"; }
        #endregion

        public string Hail(Acter who)
        {
            var rval = Quirks & Quirk.CASUAL ? "Hey" : "";
            if (AmbiguousListener) rval += ", " + who;
            return rval;
        }

        protected void Query(Acter who, Interaction.Atom about)
        {
            Interactee = who;

            var q = new Interaction.Query(this, who, about);
            memory.Add(q);

            if (textBubble == null) MakeTextBubble();
            textBubble.Add(q, Constants.NO_OP);
        }

        public Interaction LastInteraction(Acter with)
        {
            return memory.Last(intr => intr.receiver == with);
        }
    }

    public class Player : Acter
    {
        public enum State
        {
            MOVING, MENU
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
        }

        #region controller
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
                            Done[Has.SPOKEN] = true;
                            state = State.MENU;
                            return;
                        }
                    }
                    else textBubble = null;

                    if (moveDelayElapsed && Move())
                    {
                        Done[Has.WALKED] = true;
                        moveDelayElapsed = false;
                        timeSinceMovement.Start();
                    }
                    else if (!Done[Has.WALKED])
                    {
                        if (textBubble == null) MakeTextBubble();
                        textBubble.Add("use direction keys, numpad, or vi keys to walk.");
                    }
                    break;

                case State.MENU:
                    if (textBubble == null) MakeTextBubble();

                    var context = NPC.Instance.LastInteraction(this);
                    Debug.Assert(context != null, "it works like this now, i may want to know when that changes");

                    //Query(NPC.Instance, 
                            //MakeTextBubble();
                            //textBubble.Add("Goto hell!", () =>
                            //{
                            //    this.textBubble = null;
                            //    this.state = State.MOVING;
                            //});
                            //textBubble.Add("Talk about what?", () => textBubble.Add("BARF", Constants.NO_OP));
                            //return;
                    //textBubble.Add(


                    if ((Direction() & UP) != 0) textBubble.GoPrev();
                    else if ((Direction() & DOWN) != 0) textBubble.GoNext();

                    if (Pressed(Keys.Enter) || Pressed(Keys.Space)) textBubble.Select();
                    break;
            }
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
        #endregion
    }

    public class NPC : Acter
    {
        #region squish
        public static NPC Instance { get; private set; }
        public List<string> Options { get; private set; }

        public NPC(int x, int y)
        {
            Instance = this;
            Location = new Point(x, y);
            Options = new List<string>();
            spritePath = "dc_caveman";
        }

        public void addOption(string opt)
        {
            Options.Add(opt);
        }
        public bool isInRange(Player player)
        {
            double a = (double)(this.X - player.X);
            double b = (double)(this.Y - player.Y);

            if (Math.Sqrt(a * a + b * b) < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public override void Update()
        {
            if (!isInRange(Player.Instance)) textBubble = null;
            else if (textBubble == null)
            {
                Query(Player.Instance, Interaction.Atom.NOTHING);
            }
        }
    }
}
