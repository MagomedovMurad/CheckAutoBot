using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.PledgeModels
{
    public enum SubjectType
    {
        [EnumMember(Value = "person")]
        Person,

        [EnumMember(Value = "org")]
        Organization
    }
}
