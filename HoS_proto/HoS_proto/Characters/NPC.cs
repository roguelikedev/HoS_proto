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
            var please = hungry.Cause(this, Verb.TALK, Player.Instance, Noun.FOOD);
            quests.Add(please);
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

            var quest = quests.Find(q => q.primaryObject == Player.Instance);

            if (playerSaid.verb == Verb.ASK_ABOUT || !quest)
            {
                Respond(Player.Instance, Engine.rand.Next(2) == 1);
                return;
            }

            if (iSaid.Descendant(quest)) Enlist(Player.Instance);
            else
            {
                Commit(quest);
                quests.Remove(quest);
            }
        }
    }
}
