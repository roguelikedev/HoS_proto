using System.Collections.Generic;
using System.Linq;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    partial class Person
    {
        #region fields, cantrips
        public readonly Act.Controller actController;

        protected string name = "";
        public override string ToString() { return name; }
        public Person Listener { get; private set; }
        public Quirk Quirks { get; protected set; }

        protected List<Act> quests = new List<Act>();

        protected List<IAct> memory = new List<IAct>();

        bool AmbiguousListener
        {
            get
            {
                if (memory.Count < 2) return true;
                return memory.Last().ActedOn != memory[memory.Count - 2].ActedOn;
            }
        }

        public Interaction LastStatement(Person to)
        {
            // wtf thanks for the undocumented "derr I couldn't find one" exception you M$ retards
            if (!memory.Exists(intr => intr.ActedOn == to)) return null;

            var rval = memory.Last(intr => intr.ActedOn == to);
            Debug.Assert(rval is Interaction);
            return rval as Interaction;
        }

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
        #endregion

        void Commit(Interaction what)
        {
            Debug.Assert(what);
            memory.Add(what.ToI);
            actController.Confirm(what.underlyingAct);
            ShowLastSentence(what);
        }

        protected void Query(Person other, Act about)
        {
            Listener = other;
            Commit(Interaction.Query.Make(this, other, about));
        }

        protected void Respond(Person other, bool affirm)
        {
            Listener = other;

            var context = other.LastStatement(this);
            if (!context) context = new Interaction.Idle(other);

            Interaction a;
            if (!context.ExpectsResponse)
            {
                a = new Interaction.Reply.Comment(this, other, context, affirm ? Mood.NICE : Mood.MEAN);
            }
            else
            {
                if (affirm)
                {
                    a = new Interaction.Reply.Ok(this, other, context);
                    if (context is Interaction.Propose) quests.Add((context as Interaction.Propose).quest);
                }
                else
                {
                    a = new Interaction.Reply.No(this, other, context);
                }
            }

            Commit(a);
        }

        protected void Enlist(Person other)
        {
            Listener = other;

            var rationale = memory.Find(a => a.Acter == this && a.Verb == _Verb.NEED) as Act;
            var goThere = rationale.Cause(other, _Verb.GO, rationale.ActedOn);
            var please = goThere.Cause(this, _Verb.LIKE, other);

            Commit(new Interaction.Propose(this, other, goThere));
        }

        public virtual void Update()
        {
            if (!Listener) return;
            var statement = Listener.LastStatement(this);
            if (!statement) return;
            if (memory.Contains(statement)) return;

            memory.Add(statement);
        }
    }
}
