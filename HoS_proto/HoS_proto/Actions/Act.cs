using Util;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace HoS_proto
{
    public class Act
    {
        public static readonly Act NO_ACT = new Act();

        #region fields
        public readonly Person acter;
        public readonly Verb verb;
        public readonly Noun actedOn;
        public readonly Noun other;
        public readonly Act parent;
        public Act RootCause { get { return parent ? parent.RootCause : this; } }
        public bool Happened { get; private set; }

        public readonly ulong GUID;
        static uint nextGUID;

        System.Action<Act> Register;
        #endregion

        #region cantrips, conversions, clutter
        public static implicit operator Color(Act a) { return Color.Gray; }
        public static implicit operator string(Act a) { return a.ToString(); }
        public static implicit operator bool(Act a) { return !object.ReferenceEquals(a, null) && a != NO_ACT; }

        public static bool operator ==(Act a, Act b) { return a.Equals(b); }
        public static bool operator !=(Act a, Act b) { return !(a == b); }
        public override bool Equals(object obj) { return base.Equals(obj); }
        public override int GetHashCode() { return GUID.GetHashCode(); }

        protected Act() { GUID = nextGUID++; }
        Act(Person s, Verb v, Noun o, Noun io, Act c, System.Action<Act> R) : this()
        {
            acter = s; verb = v; actedOn = o; other = io; parent = c; Register = R;
            Register(this);
        }
        #endregion

        /// <summary> She pours tequila. </summary>
        /// <param name="subject"> She </param>
        /// <param name="verb"> pours </param>
        /// <param name="_object"> tequila. </param>
        public Act Cause(Person subject, Verb verb, Noun _object) { return Cause(subject, verb, Noun.NOTHING, _object); }

        /// <summary> [you] Give me food. </summary>
        /// <param name="subject"> you </param>
        /// <param name="verb"> Give </param>
        /// <param name="indirectObject"> me </param>
        /// <param name="_object"> food. </param>
        public Act Cause(Person subject, Verb verb, Noun indirectObject, Noun _object)
        {
            return new Act(subject, verb, _object, indirectObject, this, Register);
        }

        public override string ToString()
        {
            if (object.ReferenceEquals(this, NO_ACT)) return "NO_ACT";

            var rval = acter.Hail(acter.Listener);
            System.Action<string> Cat = str =>
            {
                if (rval.Length > 0 && !rval.EndsWith(" ")) rval += " ";
                rval += str;
            };

            switch (verb)
            {
                case Verb.ASK:
                    #region squish
                    Cat("why");
                    if (!parent)
                    {
                        Cat(acter.Listener);
                        Cat("so ugly");
                    }
                    else
                    {
                        Cat(parent.acter);
                        Cat(parent.verb.ToString().ToLower());
                        Cat(parent.actedOn);
                    }
                    rval += "?";
                    break;
                    #endregion
                case Verb.TALK:
                    if (!parent) Cat("what a quiet one you are.");
                    else
                    {
                        Cat("just keep on");
                        Cat(verb.ToString().ToLower() + "ing");
                    }
                    break;

                case Verb.NEED:
                case Verb.LIKE:
                case Verb.GIVE:
                case Verb.GO:
                    Cat(acter);
                    Cat(verb.ToString().ToLower());
                    if (other) Cat(other);
                    Cat(actedOn);

                    break;
                default:
                    Cat(acter);
                    Cat(verb.ToString().ToLower());
                    if (other) Cat(other);
                    Cat(actedOn);

                    if (parent) Cat("(" + parent + ")");
                    else Cat("nothing");
                    break;
            }

            if (!rval.EndsWith("?")) rval += ".";
            return rval;
        }

        public bool DescendantOf(Act rootCause)
        {
            Debug.Assert(this != rootCause, "not how the algorithm was intended to work.");
            if (parent == rootCause) return true;
            return parent ? parent.DescendantOf(rootCause) : false;
        }

        public class Controller
        {
            #region fields, properties
            Dictionary<Act, Act> dependencies = new Dictionary<Act, Act>();
            List<Act> Allocated { get { return new List<Act>(dependencies.Keys); } }
            List<Act> History { get { return Allocated.FindAll(a => a.Happened); } }
            List<Act> TalkedAbout { get { return new List<Act>(Allocated.FindAll(a => !a.Happened).Except<Act>(Promises)); } }
            List<Act> Promises { get { return new List<Act>(dependencies.Values).FindAll(a => a != NO_ACT); } }
            #endregion

            void Register(Act what)
            {
                Debug.Assert(!Allocated.Contains(what));

                dependencies[what] = NO_ACT;
                var parent = what.parent;
                if (parent && !parent.Happened) dependencies[parent] = what;
            }

            public Controller() { if (NO_ACT.Register == null) NO_ACT.Register = Register; }
            public Act FirstCause(Person subject, Verb verb, Noun _object)
            {
                return FirstCause(subject, verb, Noun.NOTHING, _object);
            }

            public Act FirstCause(Person subject, Verb verb, Noun indirectObject, Noun _object)
            {
                Debug.Assert(indirectObject || verb != Verb.GIVE, "that's a ternary verb.");
                return new Act(subject, verb, _object, indirectObject, NO_ACT, Register);
            }

            public void Confirm(Act hasHappened)
            {
                Debug.Assert(Allocated.Contains(hasHappened));
                Debug.Assert(!hasHappened.parent || History.Contains(hasHappened.parent));
                Debug.Assert(TalkedAbout.Contains(hasHappened));

                hasHappened.Happened = true;        // Past now finds the current Act, Present doesn't.
                dependencies[hasHappened] = NO_ACT; // Present now finds the consequent Act, Future doesn't.
            }

            public Act Consequence(Act preCondition)
            {
                return dependencies[preCondition];
            }

            void Update(Act act)
            {
                switch (act.verb)
                {
                    case Verb.GO:
                        act.Happened = act.acter.Adjacent(act.actedOn);
                        break;
                }
            }

            public void Update()
            {
                TalkedAbout.ForEach(Update);
            }
        }
    }
}
