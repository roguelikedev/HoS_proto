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
        public class Tell : Interaction
        {
            protected override Color Color { get { return Color.Green; } }
            public override string ToVerb { get { return "say"; } }
            Query context;

            public Tell(Person from, Person to, Query about) : base(from, to) { context = about; }

            public override string ToString()
            {
                Func<Exister, string> Directions = ex =>
                {
                    var _rval = "You can probably find " + ex;
                    if (ex.Location.Y < sender.Location.Y - 3) _rval += "north";
                    if (ex.Location.Y > sender.Location.Y + 3) _rval += "south";
                    if (ex.Location.X < sender.Location.X - 3) _rval += "west";
                    if (ex.Location.X > sender.Location.X + 3) _rval += "east";
                    _rval += " of here.";
                    return _rval;
                };

                var rval = "";
                switch (context.SubjectAsAtom)
                {
                    case Atom.NOTHING:
                        rval += sender.Quirks & Quirk.TIGHT_LIPPED ? "*grunt*" : "Oh, you know.";
                        break;
                    case Atom.PERSON:
                        Debug.Assert(context.SubjectAsActer);
                        rval += ProOrProperNoun(context.SubjectAsActer) + ", what a";
                        if (context.SubjectAsActer.Quirks & Quirk.EGOTISTICAL)
                        {
                            rval += context.SubjectAsActer == sender ? " cool" : " egotistical";
                        }
                        if (context.SubjectAsActer.Quirks & Quirk.TIGHT_LIPPED) rval += " quiet";
                        rval += " person.\n";

                        rval += Directions(context.SubjectAsActer);
                        break;

                    case Atom.PLACE:
                        rval += Directions(context.SubjectAsExister);
                        break;

                    case Atom.MUTUAL_HISTORY:
                        rval += sender.Hail(receiver) + "you're confusing me.";
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
