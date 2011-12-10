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
        Person __Listener__;
        public Person Listener
        {
            get { return __Listener__ ? __Listener__ : Closest.Adjacent(this) ? Closest : null; }
            private set { __Listener__ = value; }
        }
        public Person Closest { get { return actController.ClosestPerson(this); } }
        public Quirk Quirks { get; protected set; }

        protected List<Act> quests = new List<Act>();
        protected List<Act> memory = new List<Act>();

        bool AmbiguousListener
        {
            get
            {
                if (memory.Count < 2) return true;
                return memory.Last().actedOn != memory[memory.Count - 2].actedOn;
            }
        }

        public Act LastInteraction(Person to)
        {
            // wtf thanks for the undocumented "derr I couldn't find one" exception you M$ retards
            if (!memory.Exists(intr => intr.actedOn == to)) return null;

            return memory.Last(intr => intr.actedOn == to);
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

        void Commit(Act what)
        {
            Debug.Assert(what);
            memory.Add(what);
            actController.Confirm(what);
            ShowLastSentence(what);
        }

        protected void Query(Person other, Act about)
        {
            Listener = other;
            Commit(about.Cause(this, Verb.ASK, other));
        }

        protected void Respond(Person other, bool affirm)
        {
            Listener = other;

            var context = other.LastInteraction(this);
            if (!context) context = Act.NO_ACT;

            Act a;
            if (context.acter == this)
            {
                if (affirm)
                {
                    a = context.Cause(this, Verb.PROMISE, other);
                    quests.Add(a);
                }
                else a = context.Cause(this, Verb.IDLE, other);
            }
            else a = context.Cause(this, Verb.TALK, other);

            Commit(a);
        }

        protected void Enlist(Person other)
        {
            Listener = other;

            var rationale = memory.Find(a => a.acter == this && a.verb == Verb.NEED);
            Debug.Assert(rationale.Happened);
            var goThere = rationale.Cause(other, Verb.GO, rationale.actedOn);

            Commit(goThere);
        }

        public virtual void Update()
        {
            if (!Listener) return;
            var statement = Listener.LastInteraction(this);
            if (!statement) return;
            if (memory.Count == 0 || memory.Last() == statement) return;

            memory.Add(statement);
        }
    }
}
