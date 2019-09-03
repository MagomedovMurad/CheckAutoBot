using CheckAutoBot.Utils;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public class Service
    {
        private ICustomLogger _customLogger;
        private IBus _bus;

        public Service(ICustomLogger customLogger, IBus bus)
        {
            _customLogger = customLogger;
            _bus = bus;
        }

        private IServer _server { get; set; }

        public void Start()
        {

        }

        public void Stop()
        {

        }


        private void Test()
        {
            _server = new Server(,);
        }

    }
}
