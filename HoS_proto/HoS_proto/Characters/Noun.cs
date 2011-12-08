using Microsoft.Xna.Framework;
using System;

namespace HoS_proto
{
    public abstract class Noun
    {
        public Point Location { get; protected set; }
        public int X { get { return Location.X; } protected set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } protected set { Location = new Point(Location.X, value); } }
        public bool Adjacent(Noun to)
        {
            double a = (double)(this.X - to.X);
            double b = (double)(this.Y - to.Y);

            if (Math.Sqrt(a * a + b * b) < 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected string spritePath;

        public virtual void Draw() { Engine.DrawAtWorld(spritePath, X, Y); }

        public override string ToString() { return spritePath; }
        public static implicit operator bool(Noun what) { return what != null && what != NOTHING; }
        public static implicit operator string(Noun what) { return what ? what.ToString() : "nothing"; }

        public static readonly Noun NOTHING = new Null("NOTHING_NOUN"),
                                    FOOD = new Null("food")
                                    ;
        class Null : Noun { public Null(string t) { spritePath = t; } }
    }
}
