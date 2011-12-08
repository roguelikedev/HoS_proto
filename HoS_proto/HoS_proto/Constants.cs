using System;

namespace HoS_proto
{
    public static class Constants
    {
        public static readonly Action NO_OP = () => { };
    }

    public enum Verb
    {
        IDLE, GO, TALK, GIVE, NEED, LIKE
    }

    public enum Mood
    {
        NEUTRAL, MEAN, NICE
    }

    public enum Subject
    {
        NOTHING, NEED, PERSON, INTERACTION, PLACE
    }
}
