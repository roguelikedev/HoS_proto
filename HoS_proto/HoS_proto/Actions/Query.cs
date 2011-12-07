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
            Subject __backing_field_for_Subject = Subject.NOTHING;
            Interaction __backing_field_for_AboutInteraction;
            Noun __backing_field_for_AboutNoun;
            Person.Need __backing_field_for_AboutNeed;
            #endregion

            public override bool ExpectsResponse { get { return true; } }
            protected override Color Color { get { return Color.Yellow; } }
            public override string ToVerb { get { return "ask"; } }

            Query(Person from, Person to) : base(from, to) { }
            public static Query Make(Person from, Person to, Interaction about)
            {
                var rval = new Query(from, to);
                rval.AboutInteraction = about;
                return rval;
            }
            public static Query Make(Person from, Person to, Noun about)
            {
                var rval = new Query(from, to);
                rval.AboutNoun = about;
                return rval;
            }
            public static Query Make(Person from, Person to, Person.Need about)
            {
                var rval = new Query(from, to);
                rval.AboutNeed = about;
                return rval;
            }

            public Subject Subject
            {
                get { return __backing_field_for_Subject; }
                private set
                {
                    Debug.Assert(Subject == Subject.NOTHING, "all SubjectAs* fields are deliberately readonly.");
                    __backing_field_for_Subject = value;
                }
            }
            public Person AboutPerson
            {
                get { return AboutNoun ? AboutNoun as Person : null; }
            }
            public Noun AboutNoun
            {
                get { return __backing_field_for_AboutNoun; }
                set
                {
                    if (!value) Subject = Subject.NOTHING;
                    else if (value is Person)
                    {
                        Subject = Subject.PERSON;
                    }
                    else if (value is Environment)
                    {
                        Subject = Subject.PLACE;
                    }
                    else
                    {
                        Debug.Assert(false, "unknown subclass of Exister: " + value.ToString());
                    }

                    __backing_field_for_AboutNoun = value;
                }
            }
            public Person.Need AboutNeed
            {
                get { return __backing_field_for_AboutNeed; }
                private set
                {
                    Subject = Subject.NEED;
                    __backing_field_for_AboutNeed = value;
                }
            }
            public Interaction AboutInteraction
            {
                get { return __backing_field_for_AboutInteraction; }
                private set
                {
                    Subject = value ? Subject.INTERACTION : Subject.NOTHING;
                    __backing_field_for_AboutInteraction = value;
                }
            }

            public override string ToString()
            {
                var rval = Sender.Hail(Receiver);

                switch (Subject)
                {
                    case Subject.PERSON:
                        Debug.Assert(AboutPerson);
                        rval += AboutPerson;
                        break;
                    case Subject.NOTHING:
                        if (Sender.Quirks & Quirk.TIGHT_LIPPED) rval = rval.Replace(", ", "...");
                        else rval += "how're you doing";
                        break;
                    case Subject.NEED:
                        rval += "do you have any ";
                        rval += AboutNeed == Person.Need.NOTHING ? "good stories" : AboutNeed.ToString();
                        break;
                    case Subject.INTERACTION:
                        Debug.Assert(AboutInteraction);
                        rval += "why did ";
                        rval += ProOrProperNoun(AboutInteraction.Sender) + " ";
                        rval += AboutInteraction.ToVerb;
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
