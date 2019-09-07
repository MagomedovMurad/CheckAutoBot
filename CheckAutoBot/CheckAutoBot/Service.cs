using Autofac;
using CheckAutoBot.Utils;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot
{
    public interface IService
    {
        void Start();
        void Stop();
    }

    public class Service: IService
    {
        private ICustomLogger _customLogger;
        private IBus _bus;
        ILifetimeScope _lifetimeScope;

        private IServer _server;

        public Service(ICustomLogger customLogger, IBus bus, ILifetimeScope lifetimeScope)
        {
            _customLogger = customLogger;
            _bus = bus;
            _lifetimeScope = lifetimeScope;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                //_server = new Server(_customLogger, _bus, );
                _server = _lifetimeScope.Resolve<IServer>();
                _server.Start();
            });
        }

        public void Stop()
        {

        }

    }
}
