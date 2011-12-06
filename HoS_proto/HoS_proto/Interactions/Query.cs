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
        public class Query : Interaction
        {
            #region ugliness
            Atom __backing_field_for_SubjectAsAtom = Atom.NOTHING;
            Interaction __backing_field_for_SubjectAsInteraction;
            Noun __backing_field_for_SubjectAsExister;
            Person.Need __backing_field_for_SubjectAsNeed;
            #endregion

            public override bool ExpectsResponse { get { return true; } }
            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "ask"; } }

            Query(Person from, Person to) : base(from, to) { }
            public static Query Make(Person from, Person to, Interaction about)
            {
                var rval = new Query(from, to);
                rval.SubjectAsInteraction = about;
                return rval;
            }
            public static Query Make(Person from, Person to, Noun about)
            {
                var rval = new Query(from, to);
                rval.SubjectAsExister = about;
                return rval;
            }
            public static Query Make(Person from, Person to, Person.Need about)
            {
                var rval = new Query(from, to);
                rval.SubjectAsNeed = about;
                return rval;
            }

            public Interaction SubjectAsInteraction
            {
                get { return __backing_field_for_SubjectAsInteraction; }
                private set
                {
                    SubjectAsAtom = value ? Atom.INTERACTION : Atom.NOTHING;
                    __backing_field_for_SubjectAsInteraction = value;
                }
            }
            public Atom SubjectAsAtom
            {
                get { return __backing_field_for_SubjectAsAtom; }
                private set
                {
                    Debug.Assert(SubjectAsAtom == Atom.NOTHING, "all SubjectAs* fields are deliberately readonly.");
                    __backing_field_for_SubjectAsAtom = value;
                }
            }
            public Person SubjectAsActer
            {
                get { return SubjectAsExister ? SubjectAsExister as Person : null; }
            }
            public Noun SubjectAsExister
            {
                get { return __backing_field_for_SubjectAsExister; }
                set
                {
                    if (!value) SubjectAsAtom = Atom.NOTHING;
                    else if (value is Person)
                    {
                        SubjectAsAtom = Atom.PERSON;
                    }
                    else if (value is Environment)
                    {
                        SubjectAsAtom = Atom.PLACE;
                    }
                    else
                    {
                        Debug.Assert(false, "unknown subclass of Exister: " + value.ToString());
                    }

                    __backing_field_for_SubjectAsExister = value;
                }
            }
            public Person.Need SubjectAsNeed
            {
                get { return __backing_field_for_SubjectAsNeed; }
                private set
                {
                    SubjectAsAtom = Atom.NEED;
                    __backing_field_for_SubjectAsNeed = value;
                }
            }

            public override string ToString()
            {
                var rval = sender.Hail(receiver);

                switch (SubjectAsAtom)
                {
                    case Atom.PERSON:
                        Debug.Assert(SubjectAsActer);
                        rval += SubjectAsActer;
                        break;
                    case Atom.NOTHING:
                        if (sender.Quirks & Quirk.TIGHT_LIPPED) rval = rval.Replace(", ", "...");
                        else rval += "how're you doing";
                        break;
                    case Atom.NEED:
                        rval += "do you have any ";
                        rval += SubjectAsNeed == Person.Need.NOTHING ? "good stories" : SubjectAsNeed.ToString();
                        break;
                    case Atom.INTERACTION:
                        Debug.Assert(SubjectAsInteraction);
                        rval += "why did ";
                        rval += ProOrProperNoun(SubjectAsInteraction.sender);
                        rval += SubjectAsInteraction.ToVerb;
                        rval += " that?";
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                rval += "?";
                return rval;
            }
        }
    }
}
