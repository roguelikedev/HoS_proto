using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    partial class Interaction
    {
        public class Query : Interaction
        {
            public override bool ExpectsResponse { get { return true; } }
            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "ask"; } }

            Query(Act about) : base(about) { }
            public static Query Make(Person from, Person to, Act about)
            {
                return new Query(about.Cause(from, _Verb.TALK, to));
            }

            public override string ToString()
            {
                var rval = Acter.Hail(ActedOn as Person);

                var context = underlyingAct.Parent;
                switch (context.Verb)
                {
                    case _Verb.IDLE:
                        if (Acter.Quirks & Quirk.TIGHT_LIPPED) rval = rval.Replace(", ", "...");
                        else rval += "how're you doing";
                        break;
                    case _Verb.NEED:
                        rval += "do you have any ";
                        rval += context.ActedOn ? "good stories" : context.ActedOn;
                        break;
                    case _Verb.TALK:
                        rval += "why did ";
                        rval += ProOrProperNoun(context.Acter as Person) + " ";
                        rval += context.Verb;
                        rval += " that?";
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                rval += "?";
                return rval;
            }
        }
    }
}
