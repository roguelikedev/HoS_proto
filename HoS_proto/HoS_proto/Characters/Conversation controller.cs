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
    partial class Person
    {
        public enum Need
        {
            NOTHING, LEARN_TALK, LEARN_WALK, FOOD
        }

        #region fields, cantrips
        protected string name = "";
        public override string ToString() { return name; }

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

        protected List<Quest> intentions = new List<Quest>();

        public string Hail(Person who)
        {
            var rval = Quirks & Quirk.CASUAL ? "Hey, " : "";
            if (AmbiguousListener)
            {
                rval += who;
                rval = Helper.Capitalize(rval);
            }
            if (rval.Length > 0 && !rval.EndsWith(", ")) rval += ", ";
            return rval;
        }

        Dictionary<Need, bool> __backing_field_for_Needs;
        protected Dictionary<Need, bool> Needs
        {
            get
            {
                if (__backing_field_for_Needs == null)
                {
                    __backing_field_for_Needs = new Dictionary<Need, bool>();
                    foreach (var key in typeof(Need).GetEnumValues())
                    {
                        __backing_field_for_Needs[(Need)key] = false;
                    }
                }
                return __backing_field_for_Needs;
            }
        }
        #endregion

        protected void Query(Person other, Interaction.Atom about)
        {
            Interactee = other;

            Interaction.Query q;
            switch (about)
            {
                case Interaction.Atom.INTERACTION:
                    q = Interaction.Query.Make(this, other, LastInteraction(other));
                    break;
                case Interaction.Atom.NOTHING:
                    q = Interaction.Query.Make(this, other, (Interaction)null);
                    break;
                case Interaction.Atom.NEED:
                    var need = new List<Need>(Needs.Keys).Find(k => Needs[k]);
                    q = Interaction.Query.Make(this, other, need);
                    break;
                default:
                    Debug.Assert(false, "write another case.");
                    return;
            }
            memory.Add(q);

            ShowLastSentence(q);
        }

        protected void Tell(Person other, Interaction.Atom about)
        {
            Interactee = other;

            Interaction a;
            switch (about)
            {
                case Interaction.Atom.PLACE:
                    var q = Interaction.Query.Make(this, this, Environment.At(0, 0));
                    a = new Interaction.Tell(this, other, q);
                    break;
                default:
                    Debug.Assert(false, "there's no code here!");
                    return;
            }

            memory.Add(a);
            ShowLastSentence(a);
        }

        protected void Respond(Person other, bool affirm)
        {
            Interactee = other;

            var context = other.LastInteraction(this);
            if (!context) context = new Interaction.Idle(other);

            Interaction a;
            if (!affirm) a = new Interaction.Response.No(this, other, context);
            else if (context is Interaction.Query)
            {
                a = new Interaction.Tell(this, other, context as Interaction.Query);
            }
            else
            {
                a = new Interaction.Response.Ok(this, other, context);
                if (context is Interaction.Propose) intentions.Add((context as Interaction.Propose).quest);
            }

            memory.Add(a);
            ShowLastSentence(a);
        }

        protected void Enlist(Person other, Quest why)
        {
            Interactee = other;

            Interaction askedForHelp = new Interaction.Propose(this, other, why);
            memory.Add(askedForHelp);

            ShowLastSentence(askedForHelp);
        }
    }
}
