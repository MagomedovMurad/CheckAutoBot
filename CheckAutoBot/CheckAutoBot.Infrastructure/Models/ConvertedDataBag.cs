using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Models
{
    public class ConvertedDataBag
    {
        #region Ctors
        public ConvertedDataBag(string message, byte[] picture)
        {
            Message = message;
            Picture = picture;
        }

        public ConvertedDataBag(string message)
        {
            Message = message;
        }

        public ConvertedDataBag()
        {
        }
        #endregion Ctors

        public string Message { get; set; }

        public byte[] Picture { get; set; }
    }
}
