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
            //Reply(Person from, Person to, Interaction context)
            //    : base(context.underlyingAct.Cause(from, Verb.TALK, to))
            //{
            //    this.context = context;
            //}

            public class No : Reply
            {
                protected override Color Color { get { return Color.Red; } }
                public override string ToVerb { get { return "disagree"; } }
                //public No(Person from, Person to, Interaction context) : base(from, to, context) { }

                public override string ToString()
                {
                    var rval = acter.Quirks & Quirk.BLUNT ? "" : "Sorry, but ";
                    if (context is Query)
                    {
                        var q = context as Query;
                        rval += q.parent.verb == HoS_proto.Verb.NEED
                                               ? "I don't have any " + q.parent.actedOn + "."
                                               : "I don't know.";
                    }
                    else if (context is Propose)
                    {
                        rval += acter.Quirks & Quirk.BLUNT ? "Do it yourself." : "I'm too busy.";
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
                //public Ok(Person from, Person to, Interaction context) : base(from, to, context) { }

                public override string ToString()
                {
                    var rval = acter.Quirks & Quirk.GENEROUS ? "Of course " : "";

                    if (context is Propose)
                    {
                        rval += acter.Quirks & Quirk.TIGHT_LIPPED ? "Okay." : "I'll do it.";
                    }
                    else if (context is Query)
                    {
                        #region Directions lambda
                        Func<Noun, string> Directions = ex =>
                        {
                            var _rval = "You can probably find " + ex + " ";
                            if (ex.Location.Y < acter.Location.Y - 3) _rval += "north";
                            if (ex.Location.Y > acter.Location.Y + 3) _rval += "south";
                            if (ex.Location.X < acter.Location.X - 3) _rval += "west";
                            if (ex.Location.X > acter.Location.X + 3) _rval += "east";
                            _rval += " of here.";
                            return _rval;
                        };
                        #endregion

                        var youAsked = context as Query;
                        switch (youAsked.parent.verb)
                        {
                            case Verb.IDLE:
                                rval += acter.Quirks & Quirk.TIGHT_LIPPED ? "*grunt*" : "Oh, you know.";
                                break;

                            //case _Verb.:
                            //    Debug.Assert(youAsked.AboutPerson);
                            //    rval += ProOrProperNoun(youAsked.AboutPerson) + ", what a";
                            //    if (youAsked.AboutPerson.Quirks & Quirk.EGOTISTICAL)
                            //    {
                            //        rval += youAsked.AboutPerson == Acter ? " cool" : " egotistical";
                            //    }
                            //    if (youAsked.AboutPerson.Quirks & Quirk.TIGHT_LIPPED) rval += " quiet";
                            //    rval += " person.\n";

                            //    rval += Directions(youAsked.AboutPerson);
                            //    break;

                            //case Subject.PLACE:
                            //    rval += Directions(youAsked.AboutNoun);
                            //    break;

                            //case Verb.TALK:
                            //    if (youAsked.parent.acter != acter)
                            //    {
                            //        rval += "how would I know?  Ask ";
                            //        rval += youAsked.parent.acter;
                            //    }
                            //    else
                            //    {
                            //        var reason = youAsked.parent.parent as Interaction;
                            //        if (reason) rval += "because " + reason + ".";
                            //        else rval += acter.Hail(actedOn as Person) + "I like doing things like that.";
                            //    }

                            //    break;

                            case Verb.NEED:
                                rval += "I have loads of " + youAsked.actedOn + ".";
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
