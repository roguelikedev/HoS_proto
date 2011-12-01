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
            Atom subjectAsAtom = Atom.NOTHING;
            Acter subjectAsActer;
            Interaction subjectAsInteraction;

            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "ask"; } }

            public override string ToString()
            {
                var rval = sender.Hail(receiver) + ", ";

                switch (subjectAsAtom)
                {
                    case Atom.SOMEONE:
                        rval += subjectAsActer;
                        break;
                    case Atom.NOTHING:
                        if (sender.Quirks & Quirk.TIGHT_LIPPED) rval = rval.Replace(", ", "...");
                        else rval += "how're you doing";
                        break;
                    case Atom.FOOD:
                        rval += "where is the apple grove";
                        break;
                    case Atom.MUTUAL_HISTORY:
                        if (subjectAsInteraction)
                        {
                            rval += "what did ";

                            Func<Acter, string> ProOrProperNoun = who =>
                            {
                                if (who == sender) return "I";
                                else if (who == receiver) return "you";
                                else return who;
                            };
                            rval += ProOrProperNoun(subjectAsInteraction.sender);

                            rval += " mean by " + subjectAsInteraction.ToVerb + "ing ";
                            rval += "that?";
                        }
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                rval += "?";
                return rval;
            }



            public Query(Acter from, Acter to, Atom subject)
                : base(from, to)
            {
                subjectAsAtom = subject;
            }
            public Query(Acter from, Acter to, Acter subject)
                : base(from, to)
            {
                subjectAsActer = subject;
                subjectAsAtom = Atom.SOMEONE;
            }
            public Query(Acter from, Acter to, Interaction subject)
                : base(from, to)
            {
                subjectAsInteraction = subject;

                if (from.Quirks & Quirk.EGOTISTICAL)
                {
                    if (subject.receiver == from || subject.sender == from) subjectAsActer = from;
                }
                else if (from.Quirks & Quirk.OUTGOING)
                {
                    if (subject.receiver != from) subjectAsActer = subject.receiver;
                    else if (subject.sender != from) subjectAsActer = subject.sender;
                }

                subjectAsAtom = Atom.MUTUAL_HISTORY;
            }
        }
    }
}
