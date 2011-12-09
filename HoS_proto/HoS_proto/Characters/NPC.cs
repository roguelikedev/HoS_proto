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
            var hungry = actController.FirstCause(this, _Verb.NEED, Noun.FOOD);
            actController.Confirm(hungry);
            memory.Add(hungry.ToI);
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
                    Query(Player.Instance, Player.Instance.actController.FirstCause(Player.Instance, _Verb.IDLE, Noun.NOTHING));
                    return;
                }
                if (!playerSaid) return;
                if (iSaid.GUID > playerSaid.GUID) return;

                if (playerSaid.ExpectsResponse)
                {
                    Respond(Player.Instance, true);
                }
                else if (quests.Count > 0)
                {
                    var q = iSaid as Interaction.Query;
                    if (q && q.underlyingAct.Verb == _Verb.NEED)
                    {
                        if (playerSaid is Interaction.Reply.No) Respond(Player.Instance, false);
                        else Enlist(Player.Instance);
                    }
                    else Query(Player.Instance, quests[0]);
                }
                else Respond(Player.Instance, Engine.rand.Next(2) == 1);
            }
        }
    }
}
