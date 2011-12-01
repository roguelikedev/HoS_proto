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

        public readonly Acter sender;
        public readonly Acter receiver;
        protected abstract Color Color { get; }

        public static implicit operator string(Interaction interaction) { return interaction.ToString(); }
        public static implicit operator Color(Interaction interaction) { return interaction.Color; }
        public static implicit operator bool(Interaction interaction) { return interaction != null; }

        public virtual string ToVerb { get { return "do"; } }

        #region constructor spam
        protected Interaction(Acter from, Acter to)
        {
            sender = from; receiver = to;
        }

        partial class Employ
        {
            public Employ(Acter from, Acter to) : base(from, to) { }
        }
        partial class Propose
        {
            public Propose(Acter from, Acter to) : base(from, to) { }
        }
        #endregion

        public partial class Employ : Interaction
        {
            protected override Color Color
            {
                get { throw new NotImplementedException(); }
            }
            public override string ToVerb { get { return "use"; } }
        }

        public partial class Propose : Interaction
        {
            protected override Color Color
            {
                get { throw new NotImplementedException(); }
            }
            public override string ToVerb { get { return "offer"; } }
        }
    }
}
