using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptchaSolver.Client
{
    class Program
    {
        private static IContainer _container { get; set; }
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Service>().As<IService>().SingleInstance();
            builder.RegisterType<RecaptchaV3Solver>().As<IRecaptchaV3Solver>().SingleInstance();
            builder.RegisterType<Controller>().As<IController>().SingleInstance();

            _container = builder.Build();
            var lifetimeScope = _container.BeginLifetimeScope();

            var service = lifetimeScope.Resolve<IService>();
            service.Start();

            Console.Read();
        }
    }
}
