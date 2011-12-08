using Util;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace HoS_proto
{
    public class Act
    {
        static readonly Act NO_ACT = new Act();

        #region fields
        public readonly Noun acter;
        public readonly Verb verb;
        public readonly Noun actedOn;
        public readonly Noun other;
        public readonly Act cause;
        public bool Happened { get; private set; }

        public readonly ulong GUID;
        static uint nextGUID;

        System.Action<Act> Register;
        #endregion

        #region cantrips, conversions, clutter
        public override string ToString()
        {
            if (object.ReferenceEquals(this, NO_ACT)) return "NO_ACT";

            var rval = new List<string>();
            rval.Add(acter.ToString());
            rval.Add(verb.ToString().ToLower());
            if (other) rval.Add(other.ToString());
            rval.Add(actedOn.ToString());
            return string.Join(" ", rval);
        }

        public static implicit operator bool(Act a) { return a != null && a != NO_ACT; }

        public static bool operator ==(Act a, Act b)
        {
            if (object.ReferenceEquals(b, null)) return false;
            return a.ToString() == b.ToString();
        }
        public static bool operator !=(Act a, Act b) { return !(a == b); }
        public override bool Equals(object obj) { return obj is Act ? this == obj as Act : false; }
        public override int GetHashCode() { return ToString().GetHashCode(); }

        Act() { GUID = nextGUID++; }
        Act(Noun s, Verb v, Noun o, Noun io, Act c, System.Action<Act> R) : this()
        {
            acter = s; verb = v; actedOn = o; other = io; cause = c; Register = R;
            Register(this);
        }
        #endregion

        public Act Cause(Noun subject, Verb verb, Noun _object) { return Cause(subject, verb, _object, Noun.NOTHING); }

        public Act Cause(Noun subject, Verb verb, Noun _object, Noun indirectObject)
        {
            return new Act(subject, verb, _object, indirectObject, this, Register);
        }

        public class Controller
        {
            #region fields, properties
            Dictionary<Act, Act> dependencies = new Dictionary<Act, Act>();
            List<Act> Allocated { get { return new List<Act>(dependencies.Keys); } }
            List<Act> Past { get { return Allocated.FindAll(a => a.Happened); } }
            List<Act> Present { get { return new List<Act>(Allocated.FindAll(a => !a.Happened).Except<Act>(Future)); } }
            List<Act> Future { get { return new List<Act>(dependencies.Values).FindAll(a => a != NO_ACT); } }
            #endregion

            void Register(Act what)
            {
                if (Allocated.Contains(what)) return;

                dependencies[what] = NO_ACT;
                var parent = what.cause;
                if (parent && !parent.Happened) dependencies[parent] = what;
            }

            public Controller() { }
            public Act FirstCause(Noun subject, Verb verb, Noun _object)
            {
                return FirstCause(subject, verb, _object, Noun.NOTHING);
            }

            public Act FirstCause(Noun subject, Verb verb, Noun _object, Noun indirectObject)
            {
                Debug.Assert(indirectObject || verb != Verb.GIVE, "that's a ternary verb.");

                return new Act(subject, verb, _object, indirectObject, NO_ACT, Register);
            }

            public void Confirm(Act hasHappened)
            {
                Debug.Assert(Allocated.Contains(hasHappened));
                Debug.Assert(!Future.Contains(hasHappened));

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
                Present.ForEach(Update);
            }
        }
    }
}
