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
            OUTGOING            = 1 << 5,
            BLUNT               = 1 << 6
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

    public enum Verb
    {
        BRING
    }

    public enum Motive
    {
        FOOD
    }

    public class Goal
    {
        Func<Point> _Location;

        public Goal(Verb why, Func<Point> Where)
        {

        }
    }

    public abstract class Exister
    {
        public Point Location { get; protected set; }
        public int X { get { return Location.X; } protected set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } protected set { Location = new Point(Location.X, value); } }
        protected string spritePath;

        public virtual void Draw()
        {
            Engine.DrawAtWorld(spritePath, X, Y);
        }

        public override string ToString()
        {
            return spritePath;
        }
    }

    public abstract class Acter : Exister
    {
        #region fields, cantrips
        protected string name;
        public override string ToString() { return name == null ? "man" : name; }
        protected Menu textBubble;

        public Acter Interactee { get; private set; }
        public Quirk Quirks { get; protected set; }
        List<Interaction> memory = new List<Interaction>();
        bool AmbiguousListener
        {
            get
            {
                if (memory.Count < 2) return true;
                return memory.Last().receiver != memory[memory.Count - 2].receiver;
            }
        }

        public Interaction LastInteraction(Acter with)
        {
            // wtf thanks for the undocumented "derr I couldn't find one" exception you M$ retards
            if (!memory.Exists(intr => intr.receiver == with)) return null;

            return memory.Last(intr => intr.receiver == with);
        }
        #endregion

        #region view
        public override void Draw()
        {
            base.Draw();
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

        void ShowLastSentence(Interaction interaction)
        {
            MakeTextBubble();
            if (this is Player) textBubble.Add(interaction);
            else if (this is NPC) textBubble.Add(interaction, Constants.NO_OP, interaction);
            else
            {
                Debug.Assert(false, "what class is THAT");
            }
        }
        #endregion

        public abstract void Update();

        #region object oriented overhead
        static List<Acter> all = new List<Acter>();
        public static void UpdateAll() { all.ForEach(a => a.Update()); }
        public static implicit operator bool(Acter who) { return who != null; }
        public static implicit operator string(Acter who) { return who ? who.ToString() : "no one"; }

        protected Acter()
        {
            Quirks = Quirk.CASUAL;
            all.Add(this);
        }
        #endregion

        public string Hail(Acter who)
        {
            var rval = Quirks & Quirk.CASUAL ? "Hey, " : "";
            if (AmbiguousListener)
            {
                rval += who;
                rval = char.ToUpper(rval[0]).ToString() + (rval.Length > 1 ? rval.Substring(1) : "");
            }
            if (rval.Length > 0) rval += ", ";
            return rval;
        }

        protected void Query(Acter who, Interaction.Atom about)
        {
            Interactee = who;

            var q = new Interaction.Query(this, who, about);
            memory.Add(q);

            ShowLastSentence(q);
        }

        protected void Respond(Acter who, bool affirm)
        {
            Interactee = who;

            var context = who.LastInteraction(this);
            if (!context) context = new Interaction.Idle(who);

            Interaction a = Interaction.Response.Make(this, who, context, affirm);
            memory.Add(a);

            ShowLastSentence(a);
        }

        protected void Enlist(Acter who, Goal why)
        {


        }
    }
}
