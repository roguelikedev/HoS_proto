using System;

namespace HoS_proto
{
    public static class Constants
    {
        public static Action NO_OP = () => { };
    }

    public enum Verb
    {
        GO, GET, TALK
    }

    public abstract partial class Interaction
    {
        public enum Atom
        {
            NOTHING, NEED, PERSON, LAST_STATEMENT, PLACE
        }
    }
}
