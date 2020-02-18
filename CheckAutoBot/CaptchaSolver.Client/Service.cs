using Autofac;
using CaptchaSolver.Client;
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
        private ILifetimeScope _lifetimeScope;
        public Service(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }
        public void Start()
        {
            var solver = _lifetimeScope.Resolve<IRecaptchaV3Solver>();
            var server = new Server(solver);
            var controller = _lifetimeScope.Resolve<IController>();

            server.Start();
            controller.Start();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
