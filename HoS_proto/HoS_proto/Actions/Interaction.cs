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
        #region fields, properties, conversions
        public readonly Act underlyingAct;
        public Person Sender { get { return underlyingAct.acter as Person; } }
        public Person Receiver { get { return underlyingAct.actedOn as Person; } }
        public ulong GUID { get { return underlyingAct.GUID; } }

        public static implicit operator string(Interaction interaction) { return interaction.ToString(); }
        public static implicit operator Color(Interaction interaction) { return interaction.Color; }
        public static implicit operator bool(Interaction interaction) { return interaction != null; }

        protected abstract Color Color { get; }
        #endregion

        public virtual bool ExpectsResponse { get { return false; } }
        public virtual string ToVerb { get { return "do"; } }

        string ProOrProperNoun(Person who)
        {
            if (who == Sender) return "I";
            else if (who == Receiver) return "you";
            else return who;
        }

        protected Interaction(Person from, Person to) : this(from.actController.FirstCause(from, Verb.TALK, to)) { }
        protected Interaction(Act act) { underlyingAct = act; }

        #region kruft
        public partial class Utilize : Interaction
        {
            public Utilize(Person from, Person to) : base(from, to) { }

            protected override Color Color
            {
                get { throw new NotImplementedException(); }
            }
            public override string ToVerb { get { return "use"; } }
        }

        public class Idle : Interaction
        {
            protected override Color Color { get { return Color.Gray; } }
            public Idle(Person from) : base(from, from) { }
            public override string ToVerb { get { return "wait"; } }
        }
        #endregion
    }
}
