using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Storage
{
    public class RequestObject
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Data { get; set; }

        public string Vin { get; set; }

        public RequestObjectType Type {get; set;}

        public DateTime Date { get; set; }
    }
}
