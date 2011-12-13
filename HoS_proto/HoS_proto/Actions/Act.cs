using Util;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System;

namespace HoS_proto
{
    public partial class Act
    {
        public static readonly Act NO_ACT = new Act();
        static int Arity(Verb verb)
        {
            switch (verb)
            {
                case Verb.AGREE:
                case Verb.ARGUE:
                case Verb.ASK_WHY:
                case Verb.ASK_FOR:
                case Verb.GIVE:
                    return 3;
                default:
                    return 2;
            }
        }

        #region fields
        public readonly Person subject;
        public readonly Verb verb;
        public readonly IVerbArguments args;
        public readonly Noun primaryObject;
        public readonly Noun secondaryObject;

        public readonly Act parent;
        public bool Happened { get; private set; }

        readonly ulong GUID;
        static ulong nextGUID;

        public const ulong NEVER_HAPPENED = 0;
        static ulong nextTimeStamp = NEVER_HAPPENED + 1;
        public ulong TimeStamp { get; private set; }

        System.Action<Act> Register;
        #endregion

        #region accessors
        public interface IVerbArguments
        {
            Noun First { get; }
            Noun Second { get; }
            Noun Last { get; }
            Person Who { get; }
            Noun What { get; }
        }
        class VerbArguments : IVerbArguments
        {
            public VerbArguments(Act client) { this.client = client; }

            readonly Act client;
            public Noun First { get { return client.primaryObject; } }
            public Noun Second { get { return client.secondaryObject; } }
            public Noun Last { get { return Second ? Second : First; } }

            Noun FilterObjects(Func<Noun, Noun> Pred)
            {
                var po = Pred(First);
                var so = Pred(Second);

                Debug.Assert(!so || !po || so == po, "ambiguity alert.");
                return po ? po : so;
            }
            public Person Who { get { return (Person)FilterObjects(n => n == client.subject ? null : n as Person); } }
            public Noun What { get { return FilterObjects(n => n is Person ? null : n); } }
        }

        public Act RootCause { get { return parent ? parent.RootCause : this; } }

        public bool Descendant(Act rootCause)
        {
            Debug.Assert(this != rootCause, "not how the algorithm was intended to work.");
            if (parent == rootCause) return true;
            return parent ? parent.Descendant(rootCause) : false;
        }
        #endregion

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
            //Debug.Assert(Arity(v) == 2 || o2, v.ToString() + " is a ternary verb.");
            subject = s; verb = v; primaryObject = o1; secondaryObject = o2; parent = c; Register = R;
            args = new VerbArguments(this);
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
                rval += str.Replace('_', ' ');
            };

            switch (verb)
            {
                case Verb.ASK_WHY:
                    #region squish
                    Cat("why");
                    if (parent)
                    {
                        Cat(parent.subject);
                        Cat(parent.verb.ToString().ToLower());
                        Cat(parent.primaryObject);
                        if (parent.secondaryObject)
                        {
                            switch (parent.verb)
                            {
                                case Verb.ASK_WHY:
                                case Verb.TALK:
                                    Cat("about");
                                    break;
                                case Verb.GIVE:
                                case Verb.GO:
                                    Cat("to");
                                    break;
                            }
                            Cat(parent.secondaryObject);
                        }
                    }
                    rval += "?";
                    break;
                    #endregion
                case Verb.TALK:
                    #region squish
                    if (parent && parent.verb == Verb.ASK_WHY) Cat("because");

                    if (subject.Listener && subject.Listener == primaryObject)
                    {
                        Cat(secondaryObject);
                        if (subject == secondaryObject) Cat("is amazing");
                    }
                    else if (parent)
                    {
                        Cat(parent.subject);
                        Cat(parent.verb.ToString());
                        if (parent.secondaryObject) Cat("to " + parent.primaryObject + " about");
                        Cat(parent.secondaryObject);
                    }
                    else Cat("I'm so confused.");
                    break;
                    #endregion
                case Verb.NEED:
                case Verb.LIKE:
                case Verb.GIVE:
                case Verb.GO:
                    Cat(subject);
                    Cat(verb.ToString().ToLower());
                    if (secondaryObject) Cat(secondaryObject);
                    Cat(primaryObject);
                    break;
                case Verb.AGREE:
                    Cat(subject);
                    Cat("agrees with");
                    Cat(args.Who);
                    Cat(", ");
                    Cat(args.Last);
                    Cat("is great.");
                    break;
                case Verb.ARGUE:
                    Cat(subject);
                    Cat("thinks");
                    Cat(args.Who);
                    Cat("is wrong and");
                    Cat(args.Last);
                    Cat("sucks");
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
    }
}
