using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public abstract partial class Interaction
    {
        public partial class Propose : Interaction
        {
            public readonly Act quest;
            //public Propose(Person from, Person to, Act quest) : base(from, to) { this.quest = quest; }

            public override bool ExpectsResponse { get { return true; } }
            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "want"; } }

            public Act Parent { get { return quest.parent; } }

            public override string ToString()
            {
                var rval = acter.Quirks & Quirk.RUDE ? "" : "Please ";

                rval += quest;
                switch (quest.verb)
                {
                    case Verb.GO:
                        rval += " over there.";
                        break;
                    default:
                        Debug.Assert(false, "BARF");
                        break;
                }

                return rval;
            }
        }
    }
}
