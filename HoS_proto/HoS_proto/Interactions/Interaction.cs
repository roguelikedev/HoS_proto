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
        public enum Atom
        {
            NOTHING, FOOD, SOMEONE, MUTUAL_HISTORY
        }
        public static Dictionary<Atom, bool> progress = new Dictionary<Atom, bool>();

        public readonly Person sender;
        public readonly Person receiver;
        protected abstract Color Color { get; }

        public static implicit operator string(Interaction interaction) { return interaction.ToString(); }
        public static implicit operator Color(Interaction interaction) { return interaction.Color; }
        public static implicit operator bool(Interaction interaction) { return interaction != null; }

        public virtual string ToVerb { get { return "do"; } }
        string ProOrProperNoun(Person who)
        {
            if (who == sender) return "I";
            else if (who == receiver) return "you";
            else return who;
        }

        public readonly ulong GUID;
        static uint nextGUID;
        protected Interaction(Person from, Person to)
        {
            sender = from; receiver = to;
            GUID = nextGUID++;
        }

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
    }
}
