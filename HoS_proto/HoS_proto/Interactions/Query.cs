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
        string ProOrProperNoun(Acter who)
        {
            if (who == sender) return "I";
            else if (who == receiver) return "you";
            else return who;
        }

        public class Query : Interaction
        {
            #region ugliness
            Atom __backing_field_for_SubjectAsAtom = Atom.NOTHING;
            Acter __backing_field_for_SubjectAsActer;
            Interaction __backing_field_for_SubjectAsInteraction;
            #endregion

            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "ask"; } }

            public Query(Acter from, Acter to, Atom subject) : base(from, to) { SubjectAsAtom = subject; }

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
            public Acter SubjectAsActer
            {
                get { return __backing_field_for_SubjectAsActer; }
                set
                {
                    __backing_field_for_SubjectAsActer = value;
                    if (SubjectAsAtom == Atom.NOTHING) SubjectAsAtom = Atom.SOMEONE;
                }
            }

            public override string ToString()
            {
                var rval = sender.Hail(receiver) + ", ";

                switch (SubjectAsAtom)
                {
                    case Atom.SOMEONE:
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

        public class Answer : Interaction
        {
            protected override Color Color { get { return Color.Green; } }
            public override string ToVerb { get { return "answer"; } }
            Query context;

            public Answer(Acter from, Acter to, Query about) : base(from, to) { context = about; }

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
