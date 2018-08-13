using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.Enums
{
    public enum OwnerType
    {
        [EnumMember(Value = "Natural")]
        Natural,

        [EnumMember(Value = "Legal")]
        Legal
    }
}
