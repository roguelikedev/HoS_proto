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
        public class Answer : Interaction
        {
            protected override Color Color { get { return Color.Green; } }
            public override string ToVerb { get { return "answer"; } }
            Query context;

            public Answer(Person from, Person to, Query about) : base(from, to) { context = about; }

            public override string ToString()
            {
                var rval = "";
                switch (context.SubjectAsAtom)
                {
                    case Atom.NOTHING:
                        rval += sender.Quirks & Quirk.TIGHT_LIPPED ? "*grunt*" : "Oh, you know.";
                        break;
                    case Atom.SOMEONE:
                        Debug.Assert(context.SubjectAsActer);
                        rval += ProOrProperNoun(context.SubjectAsActer) + ", what a";
                        if (context.SubjectAsActer.Quirks & Quirk.EGOTISTICAL)
                        {
                            rval += context.SubjectAsActer == sender ? " cool" : " egotistical";
                        }
                        if (context.SubjectAsActer.Quirks & Quirk.TIGHT_LIPPED) rval += " quiet";
                        rval += " person.";

                        rval += "\nYou can probably find " + context.SubjectAsActer + " ";
                        if (context.SubjectAsActer.Location.Y < sender.Location.Y - 3) rval += "north";
                        if (context.SubjectAsActer.Location.Y > sender.Location.Y + 3) rval += "south";
                        if (context.SubjectAsActer.Location.X < sender.Location.X - 3) rval += "west";
                        if (context.SubjectAsActer.Location.X > sender.Location.X + 3) rval += "east";
                        rval += " of here.";

                        break;

                    case Atom.MUTUAL_HISTORY:
                        rval += "I don't know anything about that!!!";
                        break;
                    case Atom.FOOD:
                        rval += "I don't know anything about that!!!";
                        break;
                }
                return rval;
            }
        }
    }
}
