using Util;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System;

namespace HoS_proto
{
    public class Act
    {
        public static readonly Act NO_ACT = new Act();

        #region fields
        public readonly Person subject;
        public readonly Verb verb;
        public readonly Noun primaryObject;
        public readonly Noun secondaryObject;
        public readonly Act parent;
        public bool Happened { get; private set; }

        public readonly ulong GUID;
        static uint nextGUID;

        System.Action<Act> Register;
        #endregion

        public Act RootCause { get { return parent ? parent.RootCause : this; } }

        #region conversions, constructors, code cancer
        public static implicit operator Color(Act a) { return Color.Gray; }
        public static implicit operator string(Act a) { return a.ToString(); }
        public static implicit operator bool(Act a) { return !object.ReferenceEquals(a, null) && a != NO_ACT; }

        public static bool operator ==(Act a, Act b) { return a.Equals(b); }
        public static bool operator !=(Act a, Act b) { return !(a == b); }
        public override bool Equals(object obj) { return base.Equals(obj); }
        public override int GetHashCode() { return GUID.GetHashCode(); }

        protected Act() { GUID = nextGUID++; }
        Act(Person s, Verb v, Noun o1, Noun o2, Act c, System.Action<Act> R) : this()
        {
            subject = s; verb = v; primaryObject = o1; secondaryObject = o2; parent = c; Register = R;
            Register(this);
        }
        #endregion

        /// <summary> She pours tequila. </summary>
        /// <param name="subject"> She </param>
        /// <param name="verb"> pours </param>
        /// <param name="_object"> tequila. </param>
        public Act Cause(Person subject, Verb verb, Noun _object) { return Cause(subject, verb, _object, Noun.NOTHING); }

        /// <summary> She pours tequila on him. </summary>
        /// <param name="subject"> She </param>
        /// <param name="verb"> pours </param>
        /// <param name="primaryObject"> tequila </param>
        /// <param name="secondaryObject"> him </param>
        public Act Cause(Person subject, Verb verb, Noun primaryObject, Noun secondaryObject)
        {
            return new Act(subject, verb, primaryObject, secondaryObject, this, Register);
        }

        public override string ToString()
        {
            if (object.ReferenceEquals(this, NO_ACT)) return "NO_ACT";

            var rval = subject.Hail(subject.Listener);
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
                        Cat(subject.Listener);
                        Cat("so ugly");
                    }
                    else
                    {
                        Cat(parent.subject);
                        Cat(parent.verb.ToString().ToLower());
                        Cat(parent.primaryObject);
                    }
                    rval += "?";
                    break;
                    #endregion
                case Verb.TALK:
                    if (!parent) Cat("what a quiet one you are.");
                    else
                    {
                        Debug.Assert(subject.Listener && subject.Listener == primaryObject, "if you don't know/remember why this assert is here, delete it.");
                        Cat("let's talk about");
                        Cat(secondaryObject);
                    }
                    break;

                case Verb.NEED:
                case Verb.LIKE:
                case Verb.GIVE:
                case Verb.GO:
                    Cat(subject);
                    Cat(verb.ToString().ToLower());
                    if (secondaryObject) Cat(secondaryObject);
                    Cat(primaryObject);

                    break;
                default:
                    Cat(subject);
                    Cat(verb.ToString().ToLower());
                    if (secondaryObject) Cat(secondaryObject);
                    Cat(primaryObject);

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
