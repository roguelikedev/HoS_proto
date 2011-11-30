using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public abstract class Interaction
    {
        public enum Key
        {
            NOTHING, HAS_EVER_SPOKEN
        }
        public static Dictionary<Key, bool> progress = new Dictionary<Key, bool>();

        public readonly Acter sender;
        public readonly Acter receiver;

        #region constructor spam
        protected Interaction(Acter from, Acter to)
        {
            sender = from; receiver = to;
        }
        partial class Response
        {
            public Response(Acter from, Acter to, Propose context) : base(from, to) { this.context = context; }
            partial class No { public No(Acter from, Acter to, Propose context) : base(from, to, context) { } }
            partial class Ok { public Ok(Acter from, Acter to, Propose context) : base(from, to, context) { } }
        }
        partial class Query : Interaction
        {
            public Query(Acter from, Acter to) : base(from, to) { }
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


        public abstract partial class Response : Interaction
        {
            public readonly Propose context;


            public partial class No : Response
            {
            }
            public partial class Ok : Response
            {
            }
        }

        public partial class Query : Interaction
        {
            Key subjectAsKey = Key.NOTHING;
            Acter subjectAsActer;

            public override string ToString()
            {
                var rval = sender.Hail(receiver) + ", ";

                return rval;
            }
        }

        public partial class Employ : Interaction
        {
        }

        public partial class Propose : Interaction
        {
        }
    }
}
