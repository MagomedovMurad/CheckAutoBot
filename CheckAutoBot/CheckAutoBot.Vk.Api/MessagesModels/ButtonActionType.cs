using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.Vk.Api.MessagesModels
{
    public enum ButtonActionType
    {
        /// <summary>
        /// Текст
        /// </summary>
        [EnumMember(Value = "text")]
        Text,

        /// <summary>
        /// Текст
        /// </summary>
        [EnumMember(Value = "callback")]
        Callback
    }
}
