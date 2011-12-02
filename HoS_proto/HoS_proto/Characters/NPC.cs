using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Timers;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public class NPC : Person
    {
        public static NPC Instance { get; private set; }
        public List<string> Options { get; private set; }

        public NPC(int x, int y)
        {
            Instance = this;
            Location = new Point(x, y);
            Options = new List<string>();
            spritePath = "dc_caveman";
            name = "weird caveman";
        }

        public void addOption(string opt)
        {
            Options.Add(opt);
        }
        public bool isInRange(Player player)
        {
            double a = (double)(this.X - player.X);
            double b = (double)(this.Y - player.Y);

            if (Math.Sqrt(a * a + b * b) < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Update()
        {
            if (!isInRange(Player.Instance)) textBubble = null;
            else
            {
                var iSaid = LastInteraction(Player.Instance);
                var playerSaid = Player.Instance.LastInteraction(this);

                if (!iSaid && !playerSaid)
                {
                    Query(Player.Instance, Interaction.Atom.NOTHING);
                    return;
                }
                if (!playerSaid) return;
                if (iSaid.GUID > playerSaid.GUID) return;

                switch (Engine.rand.Next(5) + (playerSaid is Interaction.Query ? 3 : 0))
                {
                    case 0:
                    case 1:
                        Query(Player.Instance, Interaction.Atom.MUTUAL_HISTORY);
                        break;
                    case 2:

                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    default:
                        if ((playerSaid as Interaction.Query).SubjectAsAtom == Interaction.Atom.PLACE)
                        {
                            Enlist(Player.Instance, Quest.New(Verb.GO, Player.Instance, Environment.At(Point.Zero)));
                        }

                        Respond(Player.Instance, Engine.rand.Next(2) == 1);
                        break;
                }
            }
        }
    }
}
