using Util;
using System.Diagnostics;
using System.Collections.Generic;

namespace HoS_proto
{
    public class Act
    {
        public readonly Noun subj;
        public readonly Verb verb;
        public readonly Noun obj;
        public readonly Noun indirectObj;
        bool happened;

        Act(Noun s, Verb v, Noun o) { subj = s; verb = v; obj = o; }

        public override string ToString()
        {
            var rval = new List<string>();
            rval.Add(subj.ToString());
            rval.Add(verb.ToString().ToLower());
            if (indirectObj) rval.Add(indirectObj.ToString());
            rval.Add(obj.ToString());
            return string.Join(" ", rval);
        }

        public static implicit operator bool(Act a) { return a != null && a.happened; }

        public class Controller
        {
            List<Act> all;
            public Controller() { }

            public Act MakeAct(Noun subject, Verb verb, Noun _object)
            {
                Debug.Assert(verb != Verb.GIVE, "that's a ternary verb.");
                var rval = new Act(subject, verb, _object);
                all.Add(rval);
                return rval;
            }

            public void Confirm(Act hasHappened)
            {
                Debug.Assert(all.Contains(hasHappened), "i don't know if that's correct behavior.");
                hasHappened.happened = true;
            }

            public void Predicate(Act preCondition, Act consequence)
            {

            }
        }
    }
}
