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
    public abstract class Exister
    {
        public Point Location { get; protected set; }
        public int X { get { return Location.X; } protected set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } protected set { Location = new Point(Location.X, value); } }
        protected string spritePath;

        public virtual void Draw() { Engine.DrawAtWorld(spritePath, X, Y); }

        public override string ToString() { return spritePath; }
        public static implicit operator bool(Exister who) { return who != null; }
    }

    public abstract partial class Person : Exister
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
                if (Interactee != null && Interactee.X > X && Interactee.X < X + 3)
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

        void ShowLastSentence(Interaction interaction)
        {
            MakeTextBubble();
            if (this is Player) textBubble.Add(interaction);
            else if (this is NPC) textBubble.Add(interaction, Constants.NO_OP, interaction);
            else
            {
                Debug.Assert(false, "what class is THAT");
            }
        }
        #endregion

        public abstract void Update();

        #region object oriented overhead
        static List<Person> all = new List<Person>();
        public static void UpdateAll() { all.ForEach(a => a.Update()); }
        public static implicit operator string(Person who) { return who ? who.ToString() : "no one"; }

        protected Person()
        {
            Quirks = Quirk.CASUAL;
            all.Add(this);
        }
        #endregion
    }
}
