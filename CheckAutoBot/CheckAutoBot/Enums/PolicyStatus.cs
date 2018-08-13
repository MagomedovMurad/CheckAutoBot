using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.Enums
{
    
    public enum PolicyStatus
    {
        [EnumMember(Value = "Действует")]
        Valid,

        [EnumMember(Value = "Не действует")]
        NotValid
    }
}
