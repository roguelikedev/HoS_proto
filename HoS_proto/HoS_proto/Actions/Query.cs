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

            public static Query Make(Person from, Person to, Act about)
            {
                throw new Exception();
                //return new Query(about.Cause(from, Verb.TALK, to));
            }

            public override string ToString()
            {
                var rval = acter.Hail(actedOn as Person);

                switch (parent.verb)
                {
                    case Verb.IDLE:
                        if (acter.Quirks & Quirk.TIGHT_LIPPED) rval = rval.Replace(", ", "...");
                        else rval += "how're you doing";
                        break;
                    case Verb.NEED:
                        rval += "do you have any ";
                        rval += parent.actedOn ? "good stories" : parent.actedOn;
                        break;
                    //case Verb.TALK:
                    //    rval += "why did ";
                    //    rval += ProOrProperNoun(parent.acter as Person) + " ";
                    //    rval += parent.verb;
                    //    rval += " that?";
                    //    break;
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
