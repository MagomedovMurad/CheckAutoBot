using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class Request
    {
        public int Id { get; set; }

        public int RequestObjectId { get; set; }

        public RequestType Type { get; set; }

        public virtual RequestObject RequestObject { get; set; }

        public bool IsCompleted { get; set; }
             
    }
}
