using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HoS_proto
{
    public class NPC : Person
    {
        public static NPC Instance { get; private set; }

        public NPC(int x, int y, Act.Controller ac) : base (x, y, ac)
        {
            Instance = this;
            spritePath = "dc_caveman";
            name = "weird caveman";
            var hungry = actController.FirstCause(this, Verb.NEED, Noun.FOOD);
            actController.Confirm(hungry);
            memory.Add(hungry);
        }

        public override void Update()
        {
            base.Update();

            if (!Adjacent(Player.Instance)) textBubble = null;
            else
            {
                var iSaid = LastInteraction(Player.Instance);
                var playerSaid = Player.Instance.LastInteraction(this);

                if (!iSaid && !playerSaid)
                {
                    Query(Player.Instance, Act.NO_ACT);
                    return;
                }
                if (!playerSaid) return;
                if (iSaid.GUID > playerSaid.GUID) return;

                if (playerSaid.verb == Verb.ASK)
                {
                    Respond(Player.Instance, true);
                }
                else if (quests.Count > 0)
                {
                    if (iSaid.RootCause.verb == Verb.NEED) Enlist(Player.Instance);
                    else Query(Player.Instance, quests[0]);
                }
                else Respond(Player.Instance, Engine.rand.Next(2) == 1);
            }
        }
    }
}
