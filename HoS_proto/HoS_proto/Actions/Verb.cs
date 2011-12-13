using System;
using System.Collections.Generic;
using System.Reflection;

namespace HoS_proto
{
    public class Verb
    {
        public const char IDLE  = (char)1,
                          GO    = (char)2,
                          SAY   = (char)3,
                          GIVE  = (char)4,
                          NEED  = (char)5,
                          LIKE  = (char)6,
                          AGREE = (char)7,
                          ARGUE = (char)8,
                          VOW   = (char)9,
                          QUERY = (char)10,
                          BEG   = (char)11
                          ;
       public const char PROMISE = VOW,
                         TALK    = SAY,
                         ASK_WHY = QUERY,
                         ASK_FOR = BEG,
                         REQUEST = BEG,
                         DECLINE = ARGUE
                         ;
        #region filth
        readonly char value;
        Verb(int v) { value = (char)v; }

        public static implicit operator char(Verb v) { return v.value; }
        public static implicit operator Verb(int i) { return new Verb(i); }

        readonly Act act = Act.NO_ACT;

        Verb(Act a) : this(a.verb) { act = a; }
        public static Verb New(Act act)
        {
            return new Verb(act);
        }
        #endregion

        public override string ToString() { return ToS; }
        public string ToS
        {
            #region get
            get
            {
            #endregion
                // get name of constant value corresponding to my own.
                var _ = new List<FieldInfo>(typeof(Verb).GetFields(BindingFlags.Static | BindingFlags.Public));
                return _.Find(i => value == (char)i.GetValue(new Verb(0))).Name.ToLower().Replace('_', ' ');
            #region -
            }
            #endregion
        }
    }
}
