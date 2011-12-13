using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HoS_proto
{
    public class NPC : Person
    {
        public NPC(int x, int y, Act.Controller ac) : base (x, y, ac)
        {
            spritePath = "dc_caveman";
            name = "weird caveman";
            var hungry = actController.FirstCause(this, Verb.NEED, Noun.FOOD);
            actController.Confirm(hungry);
            memory.Add(hungry);
        }

        public override void Update()
        {
            base.Update();

            if (!Adjacent(Player.Instance))
            {
                textBubble = null;
                return;
            }

            var iSaid = LastInteraction(Player.Instance);
            var playerSaid = Player.Instance.LastInteraction(this);

            #region sanitize *Said
            if (!iSaid && !playerSaid)
            {
                Query(Player.Instance, Noun.NOTHING, Act.NO_ACT);
                return;
            }
            if (!playerSaid) return;
            Debug.Assert(iSaid != playerSaid);
            if (iSaid.TimeStamp > playerSaid.TimeStamp) return;
            #endregion

            var hungry = memory.Find(a => a.subject == this && a.verb == Verb.NEED);

            if (playerSaid.verb == Verb.ASK_WHY || !hungry)
            {
                Respond(Player.Instance, Engine.rand.Next(2) == 1);
                return;
            }

            if (iSaid.Descendant(hungry))
            {
                Enlist(Player.Instance, hungry);
            }
            else
            {
                Commit(hungry.Cause(this, Verb.TALK, Player.Instance, hungry.args.What));
            }
        }
    }
}
