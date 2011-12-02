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
            #region ugliness
            Atom __backing_field_for_SubjectAsAtom = Atom.NOTHING;
            Person __backing_field_for_SubjectAsActer;
            Interaction __backing_field_for_SubjectAsInteraction;
            Exister __backing_field_for_SubjectAsExister;
            #endregion

            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "ask"; } }

            public Query(Person from, Person to, Atom subject) : base(from, to) { SubjectAsAtom = subject; }

            public Interaction SubjectAsInteraction
            {
                get { return __backing_field_for_SubjectAsInteraction; }
                set
                {
                    __backing_field_for_SubjectAsInteraction = value;
                    if ((sender == SubjectAsInteraction.sender || sender == SubjectAsInteraction.receiver)
                        && (receiver == SubjectAsInteraction.receiver || receiver == SubjectAsInteraction.sender))
                    {
                        if (SubjectAsAtom == Atom.NOTHING) SubjectAsAtom = Atom.MUTUAL_HISTORY;
                        if (!SubjectAsActer)
                        {
                            if (sender.Quirks & Quirk.EGOTISTICAL) SubjectAsActer = sender;
                            else if (sender.Quirks & Quirk.OUTGOING) SubjectAsActer = receiver;
                        }
                    }
                }
            }
            public Atom SubjectAsAtom
            {
                get { return __backing_field_for_SubjectAsAtom; }
                private set
                {
                    Debug.Assert(SubjectAsAtom == Atom.NOTHING);
                    if (value == Atom.MUTUAL_HISTORY && !SubjectAsInteraction)
                    {
                        var lastInteraction = receiver.LastInteraction(sender);
                        if (!lastInteraction) lastInteraction = sender.LastInteraction(receiver);
                        if (lastInteraction) SubjectAsInteraction = lastInteraction;
                    }
                    __backing_field_for_SubjectAsAtom = value;
                }
            }
            public Person SubjectAsActer
            {
                get { return __backing_field_for_SubjectAsActer; }
                set
                {
                    __backing_field_for_SubjectAsActer = value;
                    if (SubjectAsAtom == Atom.NOTHING) SubjectAsAtom = Atom.PERSON;
                    if (SubjectAsExister == null) SubjectAsExister = value;
                }
            }
            public Exister SubjectAsExister
            {
                get { return __backing_field_for_SubjectAsExister; }
                set
                {
                    __backing_field_for_SubjectAsExister = value;
                    if (SubjectAsAtom == Atom.NOTHING) SubjectAsAtom = (value is Person)
                                                                     ? Atom.PERSON
                                                                     : ((value is Environment)
                                                                        ? Atom.PLACE
                                                                        : Atom.NOTHING
                                                                        )
                                                                     ;
                }
            }

            public override string ToString()
            {
                var rval = sender.Hail(receiver);

                switch (SubjectAsAtom)
                {
                    case Atom.PERSON:
                        rval += SubjectAsActer;
                        break;
                    case Atom.NOTHING:
                        if (sender.Quirks & Quirk.TIGHT_LIPPED) rval = rval.Replace(", ", "...");
                        else rval += "how're you doing";
                        break;
                    case Atom.FOOD:
                        rval += "where is the apple grove";
                        break;
                    case Atom.MUTUAL_HISTORY:
                        if (SubjectAsInteraction)
                        {
                            rval += "what did ";
                            rval += ProOrProperNoun(SubjectAsInteraction.sender);
                            rval += " mean by " + SubjectAsInteraction.ToVerb + "ing ";
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
        }
    }
}
