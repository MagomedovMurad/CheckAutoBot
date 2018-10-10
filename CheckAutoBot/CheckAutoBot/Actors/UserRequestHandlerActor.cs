using Akka.Actor;
using CheckAutoBot.EaistoModels;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Actors
{
    public class UserRequestHandlerActor : ReceiveActor
    {
        private IRepositoryFactory _repositoryFactory;
        private ICanSelectActor _actorSelector;
        private readonly ILogger _logger;
        private Random _random;


        private Rsa _rsaManager;
        private Gibdd _gibddManager;
        private ReestrZalogov _fnpManager;
        private Eaisto _eaistoManager;
        private Rucaptcha _rucaptchaManager;
        private KeyboardBuilder _keyboardBuilder;

        private List<CacheItem> _captchaCacheItems = new List<CacheItem>();

        public UserRequestHandlerActor(ILogger logger)
        {
            _repositoryFactory = new RepositoryFactory();
            _rsaManager = new Rsa();
            _gibddManager = new Gibdd();
            _fnpManager = new ReestrZalogov();
            _rucaptchaManager = new Rucaptcha();
            _keyboardBuilder = new KeyboardBuilder();
            _actorSelector = new ActorSelector();
            _eaistoManager = new Eaisto();
            _logger = logger;
            _random = new Random();

            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
        }

        private async Task<bool> UserRequestHandler(UserRequestMessage message)
        {
            var lastRequestObject = await GetLastUserRequestObject(message.UserId);

            var userRequest = new Request()
            {
                RequestObjectId = lastRequestObject.Id,
                Type = message.RequestType
            };

            var requestId = await AddUserRequest(userRequest);

            switch (message.RequestType)
            {
                case RequestType.History:
                    PreGetGibdd(lastRequestObject as Auto, requestId.Value, ActionType.History);
                    break;
                case RequestType.Dtp:
                    PreGetGibdd(lastRequestObject as Auto, requestId.Value, ActionType.Dtp);
                    break;
                case RequestType.Restricted:
                    PreGetGibdd(lastRequestObject as Auto, requestId.Value, ActionType.Restricted);
                    break;
                case RequestType.Wanted:
                    PreGetGibdd(lastRequestObject as Auto, requestId.Value, ActionType.Wanted);
                    break;
            }

            return true;
        }

        private async Task<bool> UserInputDataMessageHandler(UserInputDataMessage message)
        {
            try
            {
                RequestObject data;

                switch (message.Type)
                {
                    #region VIN
                    case InputDataType.Vin:
                        data = new Auto
                        {
                            Vin = message.Data,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;
                    #endregion VIN

                    #region LicensePlate
                    case InputDataType.LicensePlate:
                        data = new Auto
                        {
                            LicensePlate = message.Data,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;

                    #endregion LicensePlate

                    #region FullName
                    case InputDataType.FullName:

                        string[] personData = message.Data.Split(' ');
                        string lastName = personData[0].Replace('_', ' '); //Фамилия
                        string firstName = personData[1].Replace('_', ' '); //Имя
                        string middleName = personData[2].Replace('_', ' '); //Отчество
                        data = new Person
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            MiddleName = middleName,
                            Date = message.Date,
                            UserId = message.UserId,
                            MessageId = message.MessageId
                        };
                        break;
                    #endregion FullName

                    default: throw new InvalidOperationException($"Не найден обработчик для типа {message.Type}");
                        break;
                }

                await AddRequestObject(data);

                var keyboard = _keyboardBuilder.CreateKeyboard(new List<RequestType>(), message.Type);
                var accessToken = "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2";

                var sendMessageParams = new SendMessageParams()
                {
                    AccessToken = accessToken,
                    Keyboard = keyboard,
                    Message = $"{message.Data}.{Environment.NewLine}Выберите доступные действия...",
                    PeerId = message.UserId
                };

                Vk.Api.Messages.Send(sendMessageParams);

                //_actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(, Self);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> CaptchaResponseMessageHadler(CaptchaResponseMessage message)
        {
            _logger.Debug($"CaptchaResponseMessageHadler: Captcha={message.Value}");

            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
                return true;

            captchaItem.CaptchaWord = message.Value;

            var items = _captchaCacheItems.Where(x => x.RequestId == captchaItem.RequestId);

            var isNotCompleted = items.Any(x => string.IsNullOrEmpty(x.CaptchaWord));

            if (!isNotCompleted)
            {
                _logger.Debug($"All captchas completed");

                var request = await GetUserRequest(captchaItem.RequestId);

                switch (request.Type)
                {
                    case RequestType.History:
                        GetHistory(request.RequestObject as Auto, items);
                        break;
                    case RequestType.Dtp:
                        GetDtp(request.RequestObject as Auto, items);
                        break;
                }
            }

            return true;
        }

        private async void GetHistory(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            string vin = auto.Vin;

            if (string.IsNullOrEmpty(vin))
            {
                var diagnosticCard = GetDiagnosticCard(auto, cacheItems);
                vin = diagnosticCard.Vin;
            }
            var historyCacheItem = cacheItems.First(x => x.ActionType == ActionType.History);
            var gibddResponse = _gibddManager.GetHistory(vin, historyCacheItem.CaptchaWord, historyCacheItem.SessionId);

            #region Send to user
            var text = HistoryToMessageText(gibddResponse.RequestResult);

            var requestTypes = await GetRequestTypes(auto.Id).ConfigureAwait(false);

            var keyboard = _keyboardBuilder.CreateKeyboard(requestTypes, InputDataType.Vin);

            var message = new SendToUserMessage()
            {
                UserId = auto.UserId,
                Text = text,
                Photo = null,
                Keyboard = keyboard
            };

            var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);
            sender.Tell(message, Self);

            #endregion Send to user
        }



        private void PreGetGibdd(Auto auto, int userRequestId, ActionType actionType)
        {
            if (string.IsNullOrEmpty(auto.Vin))
            {
                var captchaResult = _eaistoManager.GetCaptcha();
                var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);
                AddCaptchaRequestToCache(userRequestId, captchaRequest.Id, ActionType.DiagnosticCard, captchaResult.SessionId);
            }

            var getDtpCaptchaResult = _gibddManager.GetCaptcha();
            var dtpCaptchaRequest = _rucaptchaManager.SendImageCaptcha(getDtpCaptchaResult.ImageBase64);
            AddCaptchaRequestToCache(userRequestId, dtpCaptchaRequest.Id, actionType, getDtpCaptchaResult.SessionId);
        }

        private void GetDtp(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            //string vin = auto.Vin;

            //if (string.IsNullOrEmpty(vin))
            //{
            //    var diagnosticCard = GetDiagnosticCard(auto, cacheItems);
            //    vin = diagnosticCard.Vin;
            //}
            //var dtpCacheItem = cacheItems.First(x => x.ActionType == ActionType.Dtp);
            //var gibddResponse = _gibddManager.GetDtp(vin, dtpCacheItem.CaptchaWord, dtpCacheItem.SessionId);

            //#region Send to user
            //var text = HistoryToMessageText(gibddResponse.RequestResult);

            //var message = new SendToUserMessage()
            //{
            //    UserId = auto.UserId,
            //    Text = text,
            //    Photo = null
            //};

            //var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);
            //sender.Tell(message, Self);

            //#endregion Send to user
        }

        #region Helpers

        private void AddCaptchaRequestToCache(int userRequestId, long captchaId, ActionType actionType, string sessionId = null)
        {
            var getPolicyNumberCacheItem = new CacheItem()
            {
                RequestId = userRequestId,
                CaptchaId = captchaId,
                ActionType = actionType,
                SessionId = sessionId,
                Date = DateTime.Now
            };
            _captchaCacheItems.Add(getPolicyNumberCacheItem);
        }

        private DiagnosticCard GetDiagnosticCard(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var phoneNumber = "+790" + _random.Next(10000000, 99999999);

            var diagnosticCardCacheItem = cacheItems.First(x => x.ActionType == ActionType.DiagnosticCard);
            var diagnosticCard = _eaistoManager.GetDiagnosticCard(diagnosticCardCacheItem.CaptchaWord, phoneNumber, diagnosticCardCacheItem.SessionId, licensePlate: auto.LicensePlate);

            return diagnosticCard;
        }

        private string HistoryToMessageText(HistoryResult history)
        {
            var text = $"Марка, модель:  {history.Vehicle.Model} \n" +
            $"Год выпуска: {history.Vehicle.Year} \n" +
            $"VIN:  {history.Vehicle.Vin} \n" +
            $"Кузов:  {history.Vehicle.BodyNumber} \n" +
            $"Цвет: {history.Vehicle.Color} \n" +
            $"Рабочий объем(см3):  {history.Vehicle.EngineVolume} \n" +
            $"Мощность(кВт/л.с.):  {history.Vehicle.PowerHp} \n" +
            $"Тип:  {history.Vehicle.TypeName} \n" +
            $"Категория: {history.Vehicle.Category}";

            var periods = history.OwnershipPeriodsEnvelop.OwnershipPeriods;
            foreach (var period in periods)
            {
                var ownerType = period.OwnerType == OwnerType.Natural ? "Физическое лицо" : "Юридическое лицо";
                var dateTo = period.To ?? "настоящее время";

                string ownerPeriod = $"{Environment.NewLine}" +
                    $"{Environment.NewLine}{ownerType}{Environment.NewLine}" +
                                     $"c: {period.From}{Environment.NewLine}" +
                                     $"по: {dateTo}{Environment.NewLine}" +
                                     $"Последняя операция: {period.LastOperation}";
                text += ownerPeriod;
            }

            return text;

        }

        //private string DtpToMessageText(DtpResult dtp)
        //{
        //    foreach (var accident in dtp.Accidents)
        //    {
        //        accident.
        //    }
        //}

        private string DtpToMessageHistory()
        {
            return $"";
        }

        #endregion Helpers

        #region DBQueries

        /// <summary>
        /// Запрос последнего объекта запроса пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        private async Task<RequestObject> GetLastUserRequestObject(int userId)
        {
            using (var rep = _repositoryFactory.CreateBotRepository())
            {
                return await rep.GetLastUserRequestObject(userId).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Добавление объекта запроса  
        /// </summary>
        /// <param name="requestObject">Объект запроса</param>
        /// <returns></returns>
        private async Task<bool> AddRequestObject(RequestObject requestObject)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.AddRequestObject(requestObject).ConfigureAwait(false);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Добавление запроса
        /// </summary>
        /// <param name="userRequest">Запрос пользователя</param>
        /// <returns></returns>
        private async Task<int?> AddUserRequest(Request userRequest)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.AddUserRequest(userRequest);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<Request> GetUserRequest(int requestId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetUserRequest(requestId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<IEnumerable<RequestType>> GetRequestTypes(int requestObjectId)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    return await rep.GetRequestTypes(requestObjectId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion DBQueries


    }
}
