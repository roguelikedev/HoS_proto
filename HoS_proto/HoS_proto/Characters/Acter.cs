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

    public abstract class Person : Exister
    {
        #region fields, cantrips
        protected string name = "";
        public override string ToString() { return name; }
        protected Menu textBubble;

        public Person Interactee { get; private set; }
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

        public Interaction LastInteraction(Person with)
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
        static List<Person> all = new List<Person>();
        public static void UpdateAll() { all.ForEach(a => a.Update()); }
        public static implicit operator bool(Person who) { return who != null; }
        public static implicit operator string(Person who) { return who ? who.ToString() : "no one"; }

        protected Person()
        {
            Quirks = Quirk.CASUAL;
            all.Add(this);
        }
        #endregion

        public string Hail(Person who)
        {
            var rval = Quirks & Quirk.CASUAL ? "Hey, " : "";
            if (AmbiguousListener)
            {
                rval += who;
                rval = char.ToUpper(rval[0]).ToString() + (rval.Length > 1 ? rval.Substring(1) : "");
            }
            if (rval.Length > 0 && !rval.EndsWith(", ")) rval += ", ";
            return rval;
        }

        protected void Query(Person other, Interaction.Atom about)
        {
            Interactee = other;

            var q = new Interaction.Query(this, other, about);
            memory.Add(q);

            ShowLastSentence(q);
        }

        protected void Respond(Person other, bool affirm)
        {
            Interactee = other;

            var context = other.LastInteraction(this);
            if (!context) context = new Interaction.Idle(other);

            Interaction a = Interaction.Response.Make(this, other, context, affirm);
            memory.Add(a);

            ShowLastSentence(a);
        }

        protected void Enlist(Person other, Quest why)
        {
            Interactee = other;

            var quest = Quest.New(Verb.GO, other, Environment.At(0, 0));
            Interaction askedForHelp = new Interaction.Propose(this, other, quest);
            memory.Add(askedForHelp);

            ShowLastSentence(askedForHelp);
        }
    }
}
