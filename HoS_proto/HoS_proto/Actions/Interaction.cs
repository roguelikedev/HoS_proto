using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public abstract partial class Interaction : Act
    {
        #region fields, properties, conversions
        public Verb Verb { get { return Verb.SAY; } }

        protected Interaction() { throw new Exception(); }

        public static implicit operator string(Interaction interaction) { return interaction.ToString(); }
        public static implicit operator Color(Interaction interaction) { return interaction.Color; }
        public static implicit operator bool(Interaction interaction) { return interaction != null; }

        protected abstract Color Color { get; }
        #endregion

        public virtual bool ExpectsResponse { get { return false; } }
        public virtual string ToVerb { get { return "do"; } }

        string ProOrProperNoun(Person who)
        {
            if (who == acter) return "I";
            else if (who == actedOn) return "you";
            else return who;
        }

        public class Idle : Interaction
        {
            protected override Color Color { get { return Color.Gray; } }
            //public Idle(Person who) : base(who.actController.FirstCause(who, _Verb.IDLE, Environment.At(who.Location))) { }
            public override string ToVerb { get { return "wait"; } }
        }
    }
}
