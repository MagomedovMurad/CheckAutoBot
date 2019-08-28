using CheckAutoBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class RequestObjectCache
    {
        public int Id { get; set; }

        public int RequestObjectId { get; set; }

        public string Data { get; set; }

        public DataType DataType { get; set; }

        public DateTime DateTime { get; set; }
    }
}
