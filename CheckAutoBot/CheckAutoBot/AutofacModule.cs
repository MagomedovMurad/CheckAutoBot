using Autofac;
using CheckAutoBot.Managers;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<ILogger>().AsSelf();
            builder.RegisterType<Gibdd>().AsSelf();
            builder.RegisterType<Rsa>().AsSelf();
            builder.RegisterType<Rucaptcha>().AsSelf();
            builder.RegisterType<Rucaptcha>().AsSelf();
        }
    }
}
