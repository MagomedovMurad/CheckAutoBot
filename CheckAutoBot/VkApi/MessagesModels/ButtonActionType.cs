using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VkApi.MessagesModels
{
    public enum ButtonActionType
    {
        /// <summary>
        /// Текст
        /// </summary>
        [EnumMember(Value = "text")]
        Text
    }
}
