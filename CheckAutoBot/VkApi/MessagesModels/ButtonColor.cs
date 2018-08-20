using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VkApi.MessagesModels
{
    public enum ButtonColor
    {
        /// <summary>
        /// Cиняя кнопка, обозначает основное действие. #5181B8 
        /// </summary>
        [EnumMember(Value = "primary")]
        Primary,

        /// <summary>
        /// Oбычная белая кнопка. #FFFFFF 
        /// </summary>
        [EnumMember(Value = "default")]
        Default,

        /// <summary>
        /// Опасное действие, или отрицательное действие (отклонить, удалить и тд). #E64646 
        /// </summary>
        [EnumMember(Value = "negative")]
        Negative,

        /// <summary>
        /// Согласиться, подтвердить. #4BB34B
        /// </summary>
        [EnumMember(Value = "positive")]
        Positive
    }
}
