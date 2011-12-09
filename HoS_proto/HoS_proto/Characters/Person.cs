using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Util;
using System.Diagnostics;

namespace HoS_proto
{
    public abstract partial class Person : Noun
    {
        #region view
        protected Menu textBubble;

        public override void Draw()
        {
            base.Draw();
            if (textBubble != null) textBubble.Draw();
        }

        protected Menu MakeTextBubble()
        {
            textBubble = new Menu();
            textBubble.DrawBox = () =>
            {
                var origin = Location;
                var rval = new Rectangle(origin.X, origin.Y, Menu.FLEXIBLE, Menu.FLEXIBLE);

                #region don't literally talk over someone.
                if (Listener != null && Listener.X > X && Listener.X < X + 3)
                #endregion
                {
                    origin = Engine.ToScreen(origin);
                    rval.X = Menu.FLEXIBLE;
                    rval.Width = origin.X; // field named Width is used as a coordinate here.
                    rval.Y = origin.Y;
                }
                else
                {
                    origin.X++; // single tile offset to avoid hiding self.
                    rval.Location = Engine.ToScreen(origin);
                }

                return rval;
            };
            return textBubble;
        }

        void ShowLastSentence(Act interaction)
        {
            Debug.Assert(interaction.ToString().Length > 0);

            MakeTextBubble();
            if (this is Player) textBubble.Add(interaction);
            else if (this is NPC) textBubble.Add(interaction, Constant.NO_OP, interaction);
            else
            {
                Debug.Assert(false, "what class is THAT");
            }
        }
        #endregion

        #region object oriented overhead
        static List<Person> all = new List<Person>();
        public static void UpdateAll() { all.ForEach(a => a.Update()); }

        protected Person(int x, int y, Act.Controller ac)
        {
            Location = new Point(x, y);
            Quirks = Quirk.CASUAL;
            all.Add(this);
            actController = ac;
        }
        #endregion
    }
}
