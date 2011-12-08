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
        partial class Reply
        {
            public class Comment : Reply
            {
                protected override Color Color { get { return Color.Yellow; } }
                public override string ToVerb { get { return "say"; } }
                readonly Mood mood;

                public Comment(Person from, Person to, Interaction about, Mood mood)
                    : base(from, to, about)
                {
                    this.mood = mood;
                }

                public override string ToString()
                {
                    var rval = "";
                    switch (mood)
                    {
                        case Mood.NEUTRAL:
                            rval += "I see that you can " + context.ToVerb;
                            break;
                        case Mood.MEAN:
                            rval += "I don't like how you're " + context.ToVerb + "ing";
                            break;
                        case Mood.NICE:
                            rval += "I admire the way you " + context.ToVerb;
                            break;
                    }
                    rval += ".";
                    return rval;
                }
            }
        }
    }
}
