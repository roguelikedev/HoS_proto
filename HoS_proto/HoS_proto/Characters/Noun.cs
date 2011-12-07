using Microsoft.Xna.Framework;

namespace HoS_proto
{
    public abstract class Noun
    {
        public Point Location { get; protected set; }
        public int X { get { return Location.X; } protected set { Location = new Point(value, Location.Y); } }
        public int Y { get { return Location.Y; } protected set { Location = new Point(Location.X, value); } }
        protected string spritePath;

        public virtual void Draw() { Engine.DrawAtWorld(spritePath, X, Y); }

        public override string ToString() { return spritePath; }
        public static implicit operator bool(Noun what) { return what != null; }
        public static implicit operator string(Noun what) { return what ? what.ToString() : "nothing"; }

        public static readonly Noun FOOD = new Null();
        class Null : Noun { }
    }
}
