using System;
using Microsoft.Xna.Framework;

namespace HoS_proto
{
    public class Quest
    {
        public readonly Verb verb;
        public readonly Person acter;
        Func<Point> _Location;
        public Point Location { get { return _Location(); } }

        Quest(Verb v, Person a) { verb = v; acter = a; }
        public static Quest New(Verb what, Person who, Environment where)
        {
            Quest rval = new Quest(what, who);
            rval._Location = () => where.Location;
            return rval;
        }

        public bool Completed
        {
            get
            {
                return acter.Location == Location;
            }
        }
    }
}