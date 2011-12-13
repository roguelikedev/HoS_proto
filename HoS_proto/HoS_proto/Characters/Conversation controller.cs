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

        bool AmbiguousListener { get { return LastInteraction(Listener) != memory.Last(); } }

        public Act LastInteraction(Person to)
        {
            // wtf thanks for the undocumented "derr I couldn't find one" exception you M$ retards
            if (!memory.Exists(a => a.subject == this && a.primaryObject == to)) return null;

            // double thanks for not supporting conversion from Predicate<Act> to Func<Act, bool>
            // fuck code reuse anyway
            return memory.Last(a => a.subject == this && a.primaryObject == to);
        }

        public string Hail(Person who)
        {
            //var rval = Quirks & Quirk.CASUAL ? "Hey, " : "";
            var rval = "";
            if (AmbiguousListener)
            {
                rval += who;
                rval = Helper.Capitalize(rval);
            }
            if (rval.Length > 0 && !rval.EndsWith(", ")) rval += ", ";
            return rval;
        }
        #endregion

        #region private helpers
        protected void Commit(Act what)
        {
            Debug.Assert(what);
            memory.Add(what);
            actController.Confirm(what);
            ShowLastSentence(what);
        }

        Act Babble() { return actController.FirstCause(this, Verb.TALK, Listener, this); }
        Act Babble(Act ignoredContext)
        {
            if (!ignoredContext) return Babble();
            return ignoredContext.Cause(this, Verb.TALK, Listener, this);
        }
        #endregion

        protected void Query(Person other, Noun about, Act because)
        {
            Listener = other;
            Commit(because.Cause(this, Verb.ASK_ABOUT, other, about));
        }

        protected void Respond(Person other, bool affirm)
        {
            Listener = other;

            var context = other.LastInteraction(this);
            if (!context) context = Act.NO_ACT;

            Act a;
            switch (context.verb)
            {
                case Verb.ASK_FOR:
                    Debug.Assert(false, "write me.");
                    if (affirm)
                    {
                        a = context.Cause(this, Verb.PROMISE, other);
                        quests.Add(a);
                    }
                    else a = context.Cause(this, Verb.IDLE, other);
                    break;
                case Verb.TALK:
                    a = context.Cause(this, affirm ? Verb.LIKE : Verb.HATE, context.secondaryObject);
                    break;
                default:
                    a = Babble(context);
                    break;
            }

            Commit(a);
        }

        protected void Enlist(Person other)
        {
            Listener = other;

            var rationale = memory.Find(a => a.subject == this && a.verb == Verb.NEED);
            Debug.Assert(rationale.Happened);
            var gimme = rationale.Cause(other, Verb.GIVE, this, rationale.primaryObject);

            Commit(gimme);
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
