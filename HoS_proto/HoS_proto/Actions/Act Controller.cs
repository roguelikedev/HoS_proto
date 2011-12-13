using Util;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System;

namespace HoS_proto
{
    partial class Act
    {
        public class Controller
        {
            #region fields, properties
            Dictionary<Act, Act> dependencies = new Dictionary<Act, Act>();
            List<Act> Allocated { get { return new List<Act>(dependencies.Keys); } }
            List<Act> History { get { return Allocated.FindAll(a => a.Happened); } }
            List<Act> TalkedAbout { get { return new List<Act>(Allocated.FindAll(a => !a.Happened).Except<Act>(Promises)); } }
            List<Act> Promises { get { return new List<Act>(dependencies.Values).FindAll(a => a); } }  // NO_ACT => false
            #endregion

            void Register(Act what)
            {
                dependencies[what] = NO_ACT;
                var parent = what.parent;
                if (parent && !parent.Happened) dependencies[parent] = what;
                acters.Add(what.subject);
                if (what.secondaryObject is Person) acters.Add(what.secondaryObject as Person);
            }

            public Controller() { if (NO_ACT.Register == null) NO_ACT.Register = Register; }
            public Act FirstCause(Person subject, Verb verb, Noun _object)
            {
                return FirstCause(subject, verb, _object, Noun.NOTHING);
            }

            public Act FirstCause(Person subject, Verb verb, Noun primaryObject, Noun secondaryObject)
            {
                Debug.Assert(primaryObject || verb != Verb.GIVE, "that's a ternary verb.");
                return NO_ACT.Cause(subject, verb, primaryObject, secondaryObject);
            }

            public void Confirm(Act hasHappened)
            {
                Debug.Assert(Allocated.Contains(hasHappened));
                Debug.Assert(!hasHappened.parent || History.Contains(hasHappened.parent));
                Debug.Assert(TalkedAbout.Contains(hasHappened));

                hasHappened.Happened = true;        // Past now finds the current Act, Present doesn't.
                hasHappened.TimeStamp = nextTimeStamp++;
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
                        act.Happened = act.subject.Adjacent(act.primaryObject);
                        break;
                }
            }

            public void Update()
            {
                TalkedAbout.ForEach(Update);
            }

            // this almost certainly doesn't belong here...
            HashSet<Person> acters = new HashSet<Person>();
            public Person ClosestPerson(Person whoIsAsking)
            {
                var tmp = new List<Person>(acters);
                tmp.Remove(whoIsAsking);
                return tmp.OrderBy(p =>
                {
                    var delta = new Vector2(p.X - whoIsAsking.X, p.Y - whoIsAsking.Y );
                    delta *= delta;
                    return delta.Length();
                }).First();
            }
        }
    }
}
