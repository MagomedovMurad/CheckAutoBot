using Autofac;
using EasyNetQ;
using System;

namespace CaptchaSolver
{
    class Program
    {
        private static IContainer _container { get; set; }
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<CacheController>().As<ICacheController>().SingleInstance();
            builder.RegisterType<Service>().As<IService>().SingleInstance();

            _container = builder.Build();
            var lifetimeScope = _container.BeginLifetimeScope();

            var service = lifetimeScope.Resolve<IService>();
            service.Start();

            Console.Read();
        }
    }
}
