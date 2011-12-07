using System.Collections.Generic;
using System.Linq;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    partial class Person
    {
        public enum Need
        {
            NOTHING, LEARN_TALK, LEARN_WALK, FOOD
        }

        #region fields, cantrips
        public readonly Act.Controller actController;

        protected string name = "";
        public override string ToString() { return name; }
        public Person Listener { get; private set; }
        public Quirk Quirks { get; protected set; }

        List<Interaction> memory = new List<Interaction>();
        protected List<Act> knowledge = new List<Act>();
        protected List<Act> intentions = new List<Act>();
        Dictionary<Act, Act> promises = new Dictionary<Act, Act>();

        bool AmbiguousListener
        {
            get
            {
                if (memory.Count < 2) return true;
                return memory.Last().Receiver != memory[memory.Count - 2].Receiver;
            }
        }

        public Interaction LastStatement(Person to)
        {
            // wtf thanks for the undocumented "derr I couldn't find one" exception you M$ retards
            if (!memory.Exists(intr => intr.Receiver == to)) return null;

            return memory.Last(intr => intr.Receiver == to);
        }

        public string Hail(Person who)
        {
            var rval = Quirks & Quirk.CASUAL ? "Hey, " : "";
            if (AmbiguousListener)
            {
                rval += who;
                rval = Helper.Capitalize(rval);
            }
            if (rval.Length > 0 && !rval.EndsWith(", ")) rval += ", ";
            return rval;
        }

        Dictionary<Need, bool> __backing_field_for_Needs;
        protected Dictionary<Need, bool> Needs
        {
            get
            {
                if (__backing_field_for_Needs == null)
                {
                    __backing_field_for_Needs = new Dictionary<Need, bool>();
                    foreach (var key in typeof(Need).GetEnumValues())
                    {
                        __backing_field_for_Needs[(Need)key] = false;
                    }
                }
                return __backing_field_for_Needs;
            }
        }
        #endregion

        protected void Query(Person other, Subject about)
        {
            Listener = other;

            Interaction.Query q;
            switch (about)
            {
                case Subject.INTERACTION:
                    q = Interaction.Query.Make(this, other, other.LastStatement(this));
                    break;
                case Subject.NOTHING:
                    q = Interaction.Query.Make(this, other, (Interaction)null);
                    break;
                case Subject.NEED:
                    var need = new List<Need>(Needs.Keys).Find(k => Needs[k]);
                    q = Interaction.Query.Make(this, other, need);
                    break;
                default:
                    Debug.Assert(false, "write another case.");
                    return;
            }
            memory.Add(q);

            ShowLastSentence(q);
        }

        protected void Respond(Person other, bool affirm)
        {
            Listener = other;

            var context = other.LastStatement(this);
            if (!context) context = new Interaction.Idle(other);

            Interaction a;
            if (!context.ExpectsResponse)
            {
                a = new Interaction.Reply.Comment(this, other, context, affirm ? Mood.NICE : Mood.MEAN);
            }
            else
            {
                if (affirm)
                {
                    a = new Interaction.Reply.Ok(this, other, context);
                    if (context is Interaction.Propose) intentions.Add((context as Interaction.Propose).quest);
                }
                else
                {
                    a = new Interaction.Reply.No(this, other, context);
                }
            }

            memory.Add(a);
            ShowLastSentence(a);
        }

        protected void Enlist(Person other)
        {
            Listener = other;

            var rationale = knowledge.Find(a => a.acter == this && a.verb == Verb.NEED);
            var goThere = actController.Because(rationale, other, Verb.GO, rationale.actedOn);
            Debug.Assert(rationale && goThere.cause == rationale);
            var please = actController.Because(goThere, this, Verb.LIKE, other);
            Debug.Assert(please.cause == goThere);

            Interaction askedForHelp = new Interaction.Propose(this, other, goThere);
            memory.Add(askedForHelp);

            ShowLastSentence(askedForHelp);
        }

        public virtual void Update()
        {
            if (!Listener) return;
            var statement = Listener.LastStatement(this);
            if (!statement) return;
            if (memory.Contains(statement)) return;

            memory.Add(statement);

        }
    }
}
