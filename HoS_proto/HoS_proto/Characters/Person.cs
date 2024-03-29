using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Util;
using System.Diagnostics;
using System;

namespace HoS_proto
{
    public partial class Person : Noun
    {
        public static readonly Person NO_ONE = new Person();
        Person() { Debug.Assert(object.ReferenceEquals(NO_ONE, null)); }

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

        protected Person(int x, int y, Act.Controller ac)
        {
            Location = new Point(x, y);
            Quirks = Quirk.CASUAL;
            all.Add(this);
            actController = ac;
        }
        #endregion

        public static void ForEach(Action<Person> Lambda) { all.ForEach(p => Lambda(p)); }
    }
}
