using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public abstract partial class Interaction : IAct
    {
        #region fields, properties, conversions
        public readonly Act underlyingAct;
        public Person Acter { get { return underlyingAct.Acter; } }
        public Noun ActedOn { get { return underlyingAct.ActedOn; } }
        public _Verb Verb { get { return HoS_proto._Verb.TALK; } }
        public virtual IAct Parent { get { return underlyingAct.Parent; } }
        public ulong GUID { get { return underlyingAct.GUID; } }
        public Noun Other { get { return underlyingAct.Other; } }

        public static implicit operator string(Interaction interaction) { return interaction.ToString(); }
        public static implicit operator Color(Interaction interaction) { return interaction.Color; }
        public static implicit operator bool(Interaction interaction) { return interaction != null; }
        public IAct ToI { get { return (IAct)this; } }

        protected abstract Color Color { get; }
        #endregion

        public virtual bool ExpectsResponse { get { return false; } }
        public virtual string ToVerb { get { return "do"; } }

        string ProOrProperNoun(Person who)
        {
            if (who == Acter) return "I";
            else if (who == ActedOn) return "you";
            else return who;
        }

        protected Interaction(Person from, Person to) : this(from.actController.FirstCause(from, _Verb.TALK, to)) { }
        protected Interaction(Act act) { underlyingAct = act; }

        #region kruft
        //public partial class Utilize : Interaction
        //{
        //    public Utilize(Person from, Person to) : base(from, to) { }

        //    protected override Color Color
        //    {
        //        get { throw new NotImplementedException(); }
        //    }
        //    public override string ToVerb { get { return "use"; } }
        //}

        public class Idle : Interaction
        {
            protected override Color Color { get { return Color.Gray; } }
            public Idle(Person who) : base(who.actController.FirstCause(who, _Verb.IDLE, Environment.At(who.Location))) { }
            public override string ToVerb { get { return "wait"; } }
        }
        #endregion
    }
}
