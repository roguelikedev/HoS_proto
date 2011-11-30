using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public abstract class Interaction
    {
        public enum Atom
        {
            NOTHING, FOOD, SOMEONE, MUTUAL_KNOWLEDGE
        }
        public static Dictionary<Atom, bool> progress = new Dictionary<Atom, bool>();

        public readonly Acter sender;
        public readonly Acter receiver;
        protected abstract Color Color { get; }

        public static implicit operator string(Interaction interaction) { return interaction.ToString(); }
        public static implicit operator Color(Interaction interaction) { return interaction.Color; }
        public static implicit operator bool(Interaction interaction) { return interaction != null; }

        public virtual string ToVerb { get { return "do"; } }

        #region constructor spam
        protected Interaction(Acter from, Acter to)
        {
            sender = from; receiver = to;
        }
        partial class Response
        {
            public Response(Acter from, Acter to, Propose context) : base(from, to) { this.context = context; }
            partial class No { public No(Acter from, Acter to, Propose context) : base(from, to, context) { } }
            partial class Ok { public Ok(Acter from, Acter to, Propose context) : base(from, to, context) { } }
        }
        partial class Query : Interaction
        {
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
                
                subjectAsAtom = Atom.MUTUAL_KNOWLEDGE;
            }

        }
        partial class Employ
        {
            public Employ(Acter from, Acter to) : base(from, to) { }
        }
        partial class Propose
        {
            public Propose(Acter from, Acter to) : base(from, to) { }
        }
        #endregion

        public abstract partial class Response : Interaction
        {
            public readonly Propose context;
            public override string ToVerb { get { return "answer"; } }

            public partial class No : Response
            {
                protected override Color Color { get { return Color.Red; } }
                public override string ToVerb { get { return "disagree"; } }
            }
            public partial class Ok : Response
            {
                protected override Color Color { get { return Color.Green; } }
                public override string ToVerb { get { return "agree"; } }
            }
        }

        public partial class Query : Interaction
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
                    case Atom.MUTUAL_KNOWLEDGE:
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
        }

        public partial class Employ : Interaction
        {
            protected override Color Color
            {
                get { throw new NotImplementedException(); }
            }
            public override string ToVerb { get { return "use"; } }
        }

        public partial class Propose : Interaction
        {
            protected override Color Color
            {
                get { throw new NotImplementedException(); }
            }
            public override string ToVerb { get { return "offer"; } }
        }
    }
}
