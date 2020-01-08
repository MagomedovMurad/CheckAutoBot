using System;
using System.Collections.Generic;
using System.Text;

namespace CaptchaSolver.Models
{
    public class Request
    {
        public string Id { get; set; }

        public byte[] Page { get; set; }

        public string Pingback { get; set; }
    }
}
