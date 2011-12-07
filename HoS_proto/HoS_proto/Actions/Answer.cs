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
            Reply(Person from, Person to, Interaction context) : base(from, to) { this.context = context; }

            public class No : Reply
            {
                protected override Color Color { get { return Color.Red; } }
                public override string ToVerb { get { return "disagree"; } }
                public No(Person from, Person to, Interaction context) : base(from, to, context) { }

                public override string ToString()
                {
                    var rval = sender.Quirks & Quirk.BLUNT ? "" : "Sorry, but ";
                    if (context is Query)
                    {
                        var q = context as Query;
                        rval += q.AboutNeed == Person.Need.NOTHING
                                             ? "I don't know."
                                             : "I don't have any " + q.AboutNeed.ToString() + ".";
                    }
                    else if (context is Propose)
                    {
                        rval += sender.Quirks & Quirk.BLUNT ? "Do it yourself." : "I'm too busy.";
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
                    var rval = sender.Quirks & Quirk.GENEROUS ? "Of course " : "";

                    if (context is Propose)
                    {
                        rval += sender.Quirks & Quirk.TIGHT_LIPPED ? "Okay." : "I'll do it.";
                    }
                    else if (context is Query)
                    {
                        #region Directions lambda
                        Func<Noun, string> Directions = ex =>
                        {
                            var _rval = "You can probably find " + ex + " ";
                            if (ex.Location.Y < sender.Location.Y - 3) _rval += "north";
                            if (ex.Location.Y > sender.Location.Y + 3) _rval += "south";
                            if (ex.Location.X < sender.Location.X - 3) _rval += "west";
                            if (ex.Location.X > sender.Location.X + 3) _rval += "east";
                            _rval += " of here.";
                            return _rval;
                        };
                        #endregion

                        var question = context as Query;
                        switch (question.Subject)
                        {
                            case Subject.NOTHING:
                                rval += sender.Quirks & Quirk.TIGHT_LIPPED ? "*grunt*" : "Oh, you know.";
                                break;

                            case Subject.PERSON:
                                Debug.Assert(question.AboutPerson);
                                rval += ProOrProperNoun(question.AboutPerson) + ", what a";
                                if (question.AboutPerson.Quirks & Quirk.EGOTISTICAL)
                                {
                                    rval += question.AboutPerson == sender ? " cool" : " egotistical";
                                }
                                if (question.AboutPerson.Quirks & Quirk.TIGHT_LIPPED) rval += " quiet";
                                rval += " person.\n";

                                rval += Directions(question.AboutPerson);
                                break;

                            case Subject.PLACE:
                                rval += Directions(question.AboutNoun);
                                break;

                            case Subject.INTERACTION:
                                var originalQuestion = question.AboutInteraction as Query;
                                Debug.Assert(originalQuestion);

                                if (originalQuestion.sender != sender)
                                {
                                    rval += "how would I know?  Ask ";
                                    rval += originalQuestion.sender;
                                }
                                else if (originalQuestion.Subject == Subject.NEED)
                                {
                                    rval += "because I need " + originalQuestion.AboutNeed.ToString() + ".";
                                }
                                else rval += sender.Hail(receiver) + "you're confusing me.";
                                break;

                            case Subject.NEED:
                                rval += "I have loads of " + question.AboutNeed + ".";
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
