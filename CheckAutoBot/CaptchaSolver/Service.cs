using Autofac;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaptchaSolver
{
    public interface IService
    {
        void Start();
        void Stop();
    }
    class Service : IService
    {
        private ICacheController _cacheController;
        private ILifetimeScope _lifetimeScope;
        public Service(ICacheController cacheController, ILifetimeScope lifetimeScope)
        {
            _cacheController = cacheController;
            _lifetimeScope = lifetimeScope;
        }
        public void Start()
        {
            var solver = new Solver(_cacheController);
            var server = new Server(_cacheController, solver);

            server.Start();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
