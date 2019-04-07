using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.FnpModels
{
    public enum HistoryItemType
    {
        [EnumMember(Value = "Creation")]
        Creation,

        [EnumMember(Value = "Change")]
        Change,

        [EnumMember(Value = "Exclusion")]
        Exclusion
    }
}
