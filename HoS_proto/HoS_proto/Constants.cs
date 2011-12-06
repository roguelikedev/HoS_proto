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

    public enum Mood
    {
        NEUTRAL, MEAN, NICE
    }

    public enum Subject
    {
        NOTHING, NEED, PERSON, INTERACTION, PLACE
    }
}
