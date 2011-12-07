using Util;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace HoS_proto
{
    public class Act
    {
        static readonly Act NO_ACT = new Act();

        public readonly Noun acter;
        public readonly Verb verb;
        public readonly Noun actedOn;
        public readonly Noun other;
        public bool Happened { get; private set; }

        public readonly ulong GUID;
        static uint nextGUID;

        Act() { GUID = nextGUID++; }
        Act(Noun s, Verb v, Noun o, Noun io) : this() { acter = s; verb = v; actedOn = o; other = io; }

        public override string ToString()
        {
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
            return a.acter == b.acter && a.verb == b.verb && a.actedOn == b.actedOn && a.other == b.other;
        }
        public static bool operator !=(Act a, Act b) { return !(a == b); }
        public override bool Equals(object obj) { return obj is Act ? this == obj as Act : false; }
        public override int GetHashCode() { return GUID.GetHashCode(); }

        public class Controller
        {
            Dictionary<Act, Act> dependencies = new Dictionary<Act, Act>();
            List<Act> Allocated { get { return new List<Act>(dependencies.Keys); } }
            List<Act> Past { get { return Allocated.FindAll(a => a.Happened); } }
            List<Act> Present { get { return new List<Act>(Allocated.FindAll(a => !a.Happened).Except<Act>(Future)); } }
            List<Act> Future { get { return new List<Act>(dependencies.Values).FindAll(a => a != NO_ACT); } }

            public Controller() { }

            public Act MakeAct(Noun subject, Verb verb, Noun _object)
            {
                Debug.Assert(verb != Verb.GIVE, "that's a ternary verb.");
                return MakeAct(subject, verb, _object, Noun.NOTHING);
            }

            public Act MakeAct(Noun subject, Verb verb, Noun _object, Noun indirectObject)
            {
                var rval = new Act(subject, verb, _object, indirectObject);
                if (!Allocated.Contains(rval)) dependencies[rval] = NO_ACT;
                return rval;
            }

            public void Confirm(Act hasHappened)
            {
                Debug.Assert(Present.Contains(hasHappened));
                Debug.Assert(!Past.Contains(hasHappened));
                Debug.Assert(!Future.Contains(hasHappened));

                hasHappened.Happened = true;        // Past now finds the current Act, Present doesn't.
                dependencies[hasHappened] = NO_ACT; // Present now finds the consequent Act, Future doesn't.
            }

            public void Predicate(Act preCondition, Act consequence)
            {
                Debug.Assert(dependencies.ContainsKey(preCondition));
                Debug.Assert(dependencies.ContainsKey(consequence));
                Debug.Assert(dependencies[preCondition] == NO_ACT);
                Debug.Assert(dependencies[consequence] == NO_ACT);

                dependencies[preCondition] = consequence;
            }
        }
    }
}
