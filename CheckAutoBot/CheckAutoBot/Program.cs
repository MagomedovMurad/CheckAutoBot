using Autofac;
using CheckAutoBot.Controllers;
using CheckAutoBot.DataSources;
using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.Infrastructure.Contracts;
using CheckAutoBot.Infrastructure.Converters;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Svg;
using CheckAutoBot.Utils;
using EasyNetQ;
using System;

namespace CheckAutoBot
{
    class Program
    {
        private static IContainer _container { get; set; }

        //RUMKEK938GV067592 mazda
        //XTA21150064291647 2115
        //XWB3K32EDCA235394 nexia
        //JMBLYV97W7J004216 залог
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            var bus = RabbitHutch.CreateBus("host=localhost");
            var vkApiManager = new VkApiManager("374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2");
            var logger = new CustomLogger(vkApiManager);


            #region controllers
            builder.RegisterType<DataRequestController>().As<IDataRequestController>().SingleInstance();
            builder.RegisterType<FrameNumberController>().As<IFrameNumberController>().SingleInstance();
            builder.RegisterType<GroupEventsController>().As<IGroupEventsController>().SingleInstance();
            builder.RegisterType<InputDataController>().As<IInputDataController>().SingleInstance();
            builder.RegisterType<LicensePlateController>().As<ILicensePlateController>().SingleInstance();
            builder.RegisterType<MessagesSenderController>().As<IMessagesSenderController>().SingleInstance();
            builder.RegisterType<PrivateMessagesController>().As<IPrivateMessagesController>().SingleInstance();
            builder.RegisterType<RequestedCaptchasCacheController>().As<IRequestedCaptchasCacheController>().SingleInstance();
            builder.RegisterType<RequestedDataCacheController>().As<IRequestedDataCacheController>().SingleInstance();
            builder.RegisterType<UserRequestController>().As<IUserRequestController>().SingleInstance();
            builder.RegisterType<VinCodeController>().As<IVinCodeController>().SingleInstance();
            builder.RegisterType<YandexMoneyController>().As<IYandexMoneyController>().SingleInstance();
            #endregion

            #region IDataSource
            builder.RegisterType<VechiclePassportDataSource>().As<IDataSource>().SingleInstance();
            builder.RegisterType<OwnershipPeriodsDataSource>().As<IDataSource>().SingleInstance();
            #endregion

            #region IDataSourceWithCaptcha
            builder.RegisterType<EaistoDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            builder.RegisterType<RsaDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            builder.RegisterType<GeneralInfoDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            builder.RegisterType<DtpDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            builder.RegisterType<RestrictedDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            builder.RegisterType<WantedDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            builder.RegisterType<PledgeDataSource>().As<IDataSourceWithCaptcha>().As<IDataSource>().SingleInstance();
            #endregion

            #region Managers
            builder.RegisterType<EaistoManager>().AsSelf();
            builder.RegisterType<FnpManager>().AsSelf();
            builder.RegisterType<GibddManager>().AsSelf();
            builder.RegisterType<RsaManager>().AsSelf();
            builder.RegisterType<RucaptchaManager>().AsSelf();
            #endregion

            #region IDataConverters
            builder.RegisterType<VechiclePassportDataConverter>().As<IDataConverter>();
            builder.RegisterType<DtpDataConverter>().As<IDataConverter>();
            builder.RegisterType<OwnershipPeriodsConverter>().As<IDataConverter>();
            builder.RegisterType<PledgeConverter>().As<IDataConverter>();
            builder.RegisterType<RestrictedDataConverter>().As<IDataConverter>();
            builder.RegisterType<WantedConverter>().As<IDataConverter>();
            #endregion

            builder.RegisterType<SvgBuilder>().AsSelf();
            

            builder.RegisterType<Service>().As<IService>().SingleInstance();
            builder.RegisterType<DbQueryExecutor>().AsSelf().SingleInstance();

            builder.RegisterType<Server>().As<IServer>().SingleInstance();
            builder.RegisterType<LicensePlateControllerCache>().As<ILicensePlateControllerCache>().SingleInstance();

            builder.RegisterInstance(bus).As<IBus>().SingleInstance();
            builder.RegisterInstance(logger).As<ICustomLogger>().SingleInstance();
            builder.RegisterInstance(vkApiManager).AsSelf().SingleInstance();

            builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();

            _container = builder.Build();
            var lifetimeScope = _container.BeginLifetimeScope();


            var service = lifetimeScope.Resolve<IService>();
            service.Start();

            Console.Read();


            //VkApiManager vkApiManager = new VkApiManager("374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2");
            //

            //var captchaCacheManager = new CaptchaCacheManager(bus, logger);

            //IRepositoryFactory repositoryFactory = new RepositoryFactory();
            //DbQueryExecutor queryExecutor = new DbQueryExecutor(repositoryFactory, logger);

            //ActorSystem actorSystem = ActorSystem.Create("TestSystem");
            //var server = actorSystem.ActorOf(Props.Create(() => new ServerActor(logger, bus)), ActorsPaths.ServerActor.Name);
            //var groupEventsHandlerActor = actorSystem.ActorOf(Props.Create(() => new GroupEventsHandlerActor(logger)), ActorsPaths.GroupEventsHandlerActor.Name);
            //var privateMessageHandlerActor = actorSystem.ActorOf(Props.Create(() => new PrivateMessageHandlerActor(queryExecutor, logger)), ActorsPaths.PrivateMessageHandlerActor.Name);
            //var privateMessageSenderActor = actorSystem.ActorOf(Props.Create(() => new PrivateMessageSenderActor(logger, vkApiManager)), ActorsPaths.PrivateMessageSenderActor.Name);
            //var userRequestHandlerActor = actorSystem.ActorOf(Props.Create(() => new UserRequestHandlerActor(logger, queryExecutor, captchaCacheManager)), ActorsPaths.UserRequestHandlerActor.Name);
            //var inputDataHandlerActor = actorSystem.ActorOf(Props.Create(() => new InputDataHandlerActor(logger, queryExecutor, vkApiManager)), ActorsPaths.InputDataHandlerActor.Name);
            //var licensePlateHandlerActor = actorSystem.ActorOf(Props.Create(() => new LicensePlateHandlerActor(queryExecutor, logger, captchaCacheManager)), ActorsPaths.LicensePlateHandlerActor.Name);
            //var yandexMoneyRequestHandlerActor = actorSystem.ActorOf(Props.Create(() => new YandexMoneyRequestHandlerActor(queryExecutor)), ActorsPaths.YandexMoneyRequestHandlerActor.Name);
            //var subscribersActionsHandlerActor = actorSystem.ActorOf(Props.Create(() => new SubscribersActionsHandlerActor()), ActorsPaths.SubscribersActionsHandlerActor.Name);
            //var vinCodeHandlerActor = actorSystem.ActorOf(Props.Create(() => new VinCodeHandlerActor(logger, queryExecutor, captchaCacheManager)), ActorsPaths.VinCodeHandlerActor.Name);

            //server.Tell(new StartServerMessage());
            //Console.ReadKey();
        }
    }
}
