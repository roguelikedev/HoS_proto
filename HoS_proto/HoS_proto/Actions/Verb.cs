using System;
using System.Collections.Generic;

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

        public string ToS
        {
            #region get
            get
            {
            #endregion
                var _ = typeof(Verb).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var __ = new List<System.Reflection.FieldInfo>(_);
                var rval = __.Find(i => value == (char)i.GetValue(new Verb(0))).Name;
                return rval;
                switch (value)
                {
                    case Verb.ASK_WHY:
                        return "asks why";
                    case Verb.ASK_FOR:
                        return "asks for";
                    case Verb.DECLINE:
                        return "declines";
                    case Verb.GIVE:
                        return "gives";
                    case Verb.GO:
                        return act.args.Who ? act.args.What ? "travels with"
                                                            : "meets"
                                            : "travels to"
                        ;
                    case Verb.IDLE:
                        return "stands around" + (act.args.Who ? " with" : "");
                    case Verb.TALK:
                    case Verb.NEED:
                    case Verb.LIKE:
                    //Cat(subject);
                    //Cat(verb.ToString().ToLower());
                    //Cat(primaryObject);
                    //if (secondaryObject) Cat(secondaryObject);
                    //break;
                    case Verb.AGREE:
                    //Cat(subject);
                    //Cat("agrees with");
                    //Cat(args.Who + ",");
                    //Cat(args.Last);
                    //Cat("is great");
                    //break;
                    case Verb.PROMISE:
                    //Cat(subject);
                    //Cat("will definitely bring");
                    //Cat(args.Who);
                    //Cat(args.What);
                    //break;
                    default:
                        return "what?";
                    //Cat("Unknown Act:");
                    //Cat("<" + subject);
                    //Cat(verb.ToString().ToLower());
                    //Cat(primaryObject);
                    //if (secondaryObject) Cat(secondaryObject);

                    //if (parent) Cat("(" + parent + ")>");
                    //else Cat("nothing>");
                }
            #region -
            }
            #endregion
        }
    }
}
