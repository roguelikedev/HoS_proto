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
        public abstract class Response : Interaction
        {
            public readonly Interaction context;
            public override string ToVerb { get { return "answer"; } }
            Response(Person from, Person to, Interaction context) : base(from, to) { this.context = context; }

            public class No : Response
            {
                protected override Color Color { get { return Color.Red; } }
                public override string ToVerb { get { return "disagree"; } }
                public No(Person from, Person to, Interaction context) : base(from, to, context) { }

                public override string ToString()
                {
                    var rval = sender.Quirks & Quirk.BLUNT ? "" : "Sorry, but ";
                    if (context is Query) rval += "I don't know.";
                    else if (context is Propose)
                    {
                        rval += sender.Quirks & Quirk.BLUNT ? "Do it yourself." : "I'm too busy.";
                    }
                    else
                    {
                        rval += "I don't like how you're " + context.ToVerb + "ing.";
                    }

                    return rval;
                }
            }
            public class Ok : Response
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
                    else
                    {
                        rval += "I admire the way you " + context.ToVerb + ".";
                    }

                    return rval;
                }
            }
        }
    }
}
