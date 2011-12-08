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
        public abstract partial class Reply : Interaction
        {
            public readonly Interaction context;
            Reply(Person from, Person to, Interaction context)
                : base(context.underlyingAct.Cause(from, Verb.TALK, to))
            {
                this.context = context;
            }

            public class No : Reply
            {
                protected override Color Color { get { return Color.Red; } }
                public override string ToVerb { get { return "disagree"; } }
                public No(Person from, Person to, Interaction context) : base(from, to, context) { }

                public override string ToString()
                {
                    var rval = Sender.Quirks & Quirk.BLUNT ? "" : "Sorry, but ";
                    if (context is Query)
                    {
                        var q = context as Query;
                        rval += q.AboutNeed == Person.Need.NOTHING
                                             ? "I don't know."
                                             : "I don't have any " + q.AboutNeed.ToString() + ".";
                    }
                    else if (context is Propose)
                    {
                        rval += Sender.Quirks & Quirk.BLUNT ? "Do it yourself." : "I'm too busy.";
                    }
                    else
                    {
                        Debug.Assert(false, "'No' is only compatible with Query or Propose.");
                    }

                    return rval;
                }
            }
            public class Ok : Reply
            {
                protected override Color Color { get { return Color.Green; } }
                public override string ToVerb { get { return "agree"; } }
                public Ok(Person from, Person to, Interaction context) : base(from, to, context) { }

                public override string ToString()
                {
                    var rval = Sender.Quirks & Quirk.GENEROUS ? "Of course " : "";

                    if (context is Propose)
                    {
                        rval += Sender.Quirks & Quirk.TIGHT_LIPPED ? "Okay." : "I'll do it.";
                    }
                    else if (context is Query)
                    {
                        #region Directions lambda
                        Func<Noun, string> Directions = ex =>
                        {
                            var _rval = "You can probably find " + ex + " ";
                            if (ex.Location.Y < Sender.Location.Y - 3) _rval += "north";
                            if (ex.Location.Y > Sender.Location.Y + 3) _rval += "south";
                            if (ex.Location.X < Sender.Location.X - 3) _rval += "west";
                            if (ex.Location.X > Sender.Location.X + 3) _rval += "east";
                            _rval += " of here.";
                            return _rval;
                        };
                        #endregion

                        var youAsked = context as Query;
                        switch (youAsked.Subject)
                        {
                            case Subject.NOTHING:
                                rval += Sender.Quirks & Quirk.TIGHT_LIPPED ? "*grunt*" : "Oh, you know.";
                                break;

                            case Subject.PERSON:
                                Debug.Assert(youAsked.AboutPerson);
                                rval += ProOrProperNoun(youAsked.AboutPerson) + ", what a";
                                if (youAsked.AboutPerson.Quirks & Quirk.EGOTISTICAL)
                                {
                                    rval += youAsked.AboutPerson == Sender ? " cool" : " egotistical";
                                }
                                if (youAsked.AboutPerson.Quirks & Quirk.TIGHT_LIPPED) rval += " quiet";
                                rval += " person.\n";

                                rval += Directions(youAsked.AboutPerson);
                                break;

                            case Subject.PLACE:
                                rval += Directions(youAsked.AboutNoun);
                                break;

                            case Subject.INTERACTION:
                                if (youAsked.AboutInteraction.Sender != Sender)
                                {
                                    rval += "how would I know?  Ask ";
                                    rval += youAsked.AboutInteraction.Sender;
                                }
                                else
                                {
                                    var reason = youAsked.AboutInteraction.Reason;
                                    if (reason) rval += "because " + reason + ".";
                                    else rval += Sender.Hail(Receiver) + "I like doing things like that.";
                                }

                                break;

                            case Subject.NEED:
                                rval += "I have loads of " + youAsked.AboutNeed + ".";
                                break;
                        }
                    }
                    else
                    {
                        Debug.Assert(false, "did you mean to use Interaction.Comment?");
                    }

                    return rval;
                }
            }
        }
    }
}
