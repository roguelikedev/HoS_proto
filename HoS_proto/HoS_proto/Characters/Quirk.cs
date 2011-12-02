using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HoS_proto;
using System.Collections.Generic;
using System.Diagnostics;

namespace HoS_proto
{
    public class Quirk
    {
        public const int
            VERY                = 1,
            CASUAL              = 1 << 1,
            TIGHT_LIPPED        = 1 << 2,
            GENEROUS            = 1 << 3,
            EGOTISTICAL         = 1 << 4,
            OUTGOING            = 1 << 5,
            BLUNT               = 1 << 6,
            RUDE                = BLUNT | EGOTISTICAL | TIGHT_LIPPED
            ;
        int value;
        Quirk(int v) { value = v; }
        public static implicit operator int(Quirk q) { return q.value; }
        public static implicit operator Quirk(int i) { return new Quirk(i); }
        public static implicit operator bool(Quirk self) { return self.value != 0; }
        public static Quirk operator &(Quirk a, Quirk b) { return a.value & b.value; }
        public static Quirk operator |(Quirk a, Quirk b) { return a.value | b.value; }
        public static bool operator ==(Quirk _this, Quirk that) { return _this.value == that.value; }
        #region shut up, compiler
        public static bool operator !=(Quirk _this, Quirk that) { return _this.value != that.value; }
        public override bool Equals(object obj)
        {
            if (obj is Quirk) return this == obj as Quirk;
            return base.Equals(obj);
        }
        public override int GetHashCode() { return value.GetHashCode(); }
        #endregion
    }
}