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
            public readonly Propose context;
            public override string ToVerb { get { return "answer"; } }
            public Response(Acter from, Acter to, Propose context) : base(from, to) { this.context = context; }

            public class No : Response
            {
                protected override Color Color { get { return Color.Red; } }
                public override string ToVerb { get { return "disagree"; } }
                public No(Acter from, Acter to, Propose context) : base(from, to, context) { }
            }
            public class Ok : Response
            {
                protected override Color Color { get { return Color.Green; } }
                public override string ToVerb { get { return "agree"; } }
                public Ok(Acter from, Acter to, Propose context) : base(from, to, context) { }
            }
        }
    }
}
