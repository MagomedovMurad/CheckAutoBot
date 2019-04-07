using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public enum SearchMode
    {
        [EnumMember(Value = "onlyActual")]
        OnlyActual,

        [EnumMember(Value = "allChanges")]
        All
    }
}
