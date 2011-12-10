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
            var gimme = hungry.Cause(Player.Instance, Verb.GIVE, Noun.FOOD);
            var please = gimme.Cause(this, Verb.TALK, Player.Instance, Noun.FOOD);
            quests.Add(please);
        }

        public override void Update()
        {
            base.Update();

            if (!Adjacent(Player.Instance)) textBubble = null;
            else
            {
                var iSaid = LastInteraction(Player.Instance);
                var playerSaid = Player.Instance.LastInteraction(this);

                #region sanitize *Said
                if (!iSaid && !playerSaid)
                {
                    Query(Player.Instance, Act.NO_ACT);
                    return;
                }
                if (!playerSaid) return;
                if (iSaid.GUID > playerSaid.GUID) return;
                #endregion

                var quest = quests.Find(q => q.other == Player.Instance);

                if (playerSaid.verb == Verb.ASK || !quest)
                {
                    Respond(Player.Instance, Engine.rand.Next(2) == 1);
                    return;
                }
                ;
                if (iSaid.DescendantOf(quest)) Enlist(Player.Instance);
                else Query(Player.Instance, quest);
            }
        }
    }
}
