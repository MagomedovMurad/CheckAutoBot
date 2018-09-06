using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.Storage
{ 
    public enum RequestType
    {
        /// <summary>
        /// История регистрации
        /// </summary>
        [EnumMember(Value = "History")]
        History,

        /// <summary>
        /// ДТП
        /// </summary>
        [EnumMember(Value = "Dtp")]
        Dtp,

        /// <summary>
        /// Ограничения
        /// </summary>
        [EnumMember(Value = "Restricted")]
        Restricted,

        /// <summary>
        /// Розыск
        /// </summary>
        [EnumMember(Value = "Wanted")]
        Wanted,

        /// <summary>
        /// Залог
        /// </summary>
        [EnumMember(Value = "Pledge")]
        Pledge,

        /// <summary>
        /// Долги
        /// </summary>
        [EnumMember(Value = "PersonPledge")]
        PersonPledge
    }
}
