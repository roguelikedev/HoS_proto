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
            Needs[Need.FOOD] = true;
            var hungry = actController.FirstCause(this, Verb.NEED, Noun.FOOD);
            actController.Confirm(hungry);
            knowledge.Add(hungry);
        }

        public override void Update()
        {
            base.Update();

            if (!Adjacent(Player.Instance)) textBubble = null;
            else
            {
                var iSaid = LastStatement(Player.Instance);
                var playerSaid = Player.Instance.LastStatement(this);

                if (!iSaid && !playerSaid)
                {
                    Query(Player.Instance, Subject.NOTHING);
                    return;
                }
                if (!playerSaid) return;
                if (iSaid.GUID > playerSaid.GUID) return;

                if (playerSaid.ExpectsResponse)
                {
                    Respond(Player.Instance, true);
                }
                else if (Needs.Count > 0)
                {
                    if (iSaid is Interaction.Query)
                    {
                        if (playerSaid is Interaction.Reply.No) Respond(Player.Instance, false);
                        else Enlist(Player.Instance);
                    }
                    else Query(Player.Instance, Subject.NEED);
                }
                else Respond(Player.Instance, Engine.rand.Next(2) == 1);
            }
        }
    }
}
