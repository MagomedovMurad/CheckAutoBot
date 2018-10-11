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

        #region Handlers

        private async Task<bool> UserRequestHandler(UserRequestMessage message)
        {
            var lastRequestObject = await GetLastUserRequestObject(message.UserId);

            var userRequest = new Request()
            {
                RequestObjectId = lastRequestObject.Id,
                Type = message.RequestType
            };

            var requestId = await SaveUserRequest(userRequest);

            if (lastRequestObject is Auto auto)
            {
                bool hasVin = auto.Vin != null;


                switch (message.RequestType)
                {
                    case RequestType.History:
                        PreGetFromGibdd(hasVin, requestId.Value, ActionType.History);
                        break;
                    case RequestType.Dtp:
                        PreGetFromGibdd(hasVin, requestId.Value, ActionType.Dtp);
                        break;
                    case RequestType.Restricted:
                        PreGetFromGibdd(hasVin, requestId.Value, ActionType.Restricted);
                        break;
                    case RequestType.Wanted:
                        PreGetFromGibdd(hasVin, requestId.Value, ActionType.Wanted);
                        break;
                    case RequestType.Pledge:
                        PreGetFromFnp(hasVin, requestId.Value, ActionType.Pledge);
                        break;
                }
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

                    default:
                        throw new InvalidOperationException($"Не найден обработчик для типа {message.Type}");
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
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
                return true;

            captchaItem.CaptchaWord = message.Value;

            var items = _captchaCacheItems.Where(x => x.RequestId == captchaItem.RequestId);

            var isNotCompleted = items.Any(x => string.IsNullOrEmpty(x.CaptchaWord));

            if (!isNotCompleted)
            {
                var request = await GetUserRequest(captchaItem.RequestId);

                if (request.RequestObject is Auto auto)
                {
                    switch (captchaItem.CurrentActionType)
                    {
                        case ActionType.VinByDiagnosticCard:
                            GetVinByDiagnosticCard(items, auto.LicensePlate);
                            break;
                        case ActionType.PolicyInfo:
                        case ActionType.PolicyNumber:
                            GetVinByPolicy(items, auto.LicensePlate);
                            break;
                        case ActionType.History:
                            await GetHistory(auto, items);
                            break;
                    }

                }

            }

            return true;
        }

        #endregion Handlers


        #region PreGet
        private void PreGetVinByDiagnosticCard(int userRequestId, ActionType targetActionType)
        {
            var captchaResult = _eaistoManager.GetCaptcha();
            var captchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);
            AddCaptchaRequestToCache(userRequestId, captchaRequest.Id, ActionType.VinByDiagnosticCard, targetActionType, captchaResult.SessionId);
        }

        private void PreGetVinByPolicy(int userRequestId, ActionType targetActionType)
        {
            var captchaRequest1 = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl);
            AddCaptchaRequestToCache(userRequestId, captchaRequest1.Id, ActionType.VinByPolicy, targetActionType);
            var captchaRequest2 = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl);
            AddCaptchaRequestToCache(userRequestId, captchaRequest2.Id, ActionType.VinByPolicy, targetActionType);
        }

        private void PreGetFromGibdd(bool hasVin, int userRequestId, ActionType targetActionType)
        {
            if (hasVin)
            {
                var getDtpCaptchaResult = _gibddManager.GetCaptcha();
                var dtpCaptchaRequest = _rucaptchaManager.SendImageCaptcha(getDtpCaptchaResult.ImageBase64);
                AddCaptchaRequestToCache(userRequestId, dtpCaptchaRequest.Id, targetActionType, targetActionType, getDtpCaptchaResult.SessionId);
            }
            else
            {
                PreGetVinByDiagnosticCard(userRequestId, targetActionType);
            }
        }

        private void PreGetFromFnp(bool hasVin, int userRequestId, ActionType targetActionType)
        {
            if (hasVin)
            {
                var getDtpCaptchaResult = _fnpManager.GetCaptcha();
                var dtpCaptchaRequest = _rucaptchaManager.SendImageCaptcha(getDtpCaptchaResult.ImageBase64);
                AddCaptchaRequestToCache(userRequestId, dtpCaptchaRequest.Id, targetActionType, targetActionType, getDtpCaptchaResult.SessionId);
            }
            else
            {
                PreGetVinByDiagnosticCard(userRequestId, targetActionType);
            }
        }

        #endregion PreGet


        #region Get

        private async void GetVinByDiagnosticCard(IEnumerable<CacheItem> cacheItems, string licensePlate)
        {
            var phoneNumber = "+790" + _random.Next(10000000, 99999999);

            var diagnosticCardCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.DiagnosticCard);
            var diagnosticCard = _eaistoManager.GetDiagnosticCard(diagnosticCardCacheItem.CaptchaWord, phoneNumber, diagnosticCardCacheItem.SessionId, licensePlate: licensePlate);

            if (diagnosticCard == null)
            {
                PreGetVinByPolicy(diagnosticCardCacheItem.RequestId, diagnosticCardCacheItem.TargetActionType);
            }
            else
            {
                var request = await GetUserRequest(diagnosticCardCacheItem.RequestId);
                await UpdateVinCode(request.RequestObject.Id, diagnosticCard.Vin);
                PreGetFromGibdd(true, diagnosticCardCacheItem.RequestId, diagnosticCardCacheItem.TargetActionType);
            }
        }

        private async void GetVinByPolicy(IEnumerable<CacheItem> cacheItems, string licensePlate)
        {
            var policyNumberCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.PolicyNumber);
            var policyResponse = _rsaManager.GetPolicy(policyNumberCacheItem.CaptchaWord, DateTime.Now, lp: licensePlate);

            var policy = policyResponse.Policies.FirstOrDefault();

            var policyInfoCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.PolicyInfo);
            var vechicleResponse = _rsaManager.GetPolicyInfo(policy.Serial, policy.Number, DateTime.Now, policyInfoCacheItem.CaptchaWord);

            if (vechicleResponse != null)
            {
                var request = await GetUserRequest(policyInfoCacheItem.RequestId);
                await UpdateVinCode(request.RequestObject.Id, vechicleResponse.Vin);

                PreGetFromGibdd(true, policyInfoCacheItem.RequestId, policyInfoCacheItem.TargetActionType);
            }
        }

        private async Task GetHistory(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var historyCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.History);
            var gibddResponse = _gibddManager.GetHistory(auto.Vin, historyCacheItem.CaptchaWord, historyCacheItem.SessionId);

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

        private void GetDtp(Auto auto, IEnumerable<CacheItem> cacheItems)
        {

        }

        #endregion Get

        #region Helpers

        private void AddCaptchaRequestToCache(int userRequestId, long captchaId, ActionType currentActionType, ActionType targetActionType, string sessionId = null)
        {
            var getPolicyNumberCacheItem = new CacheItem()
            {
                RequestId = userRequestId,
                CaptchaId = captchaId,
                CurrentActionType = currentActionType,
                TargetActionType = targetActionType,
                SessionId = sessionId,
                Date = DateTime.Now
            };
            _captchaCacheItems.Add(getPolicyNumberCacheItem);
        }

        private DiagnosticCard GetDiagnosticCard(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var phoneNumber = "+790" + _random.Next(10000000, 99999999);

            var diagnosticCardCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.DiagnosticCard);
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
        private async Task<int?> SaveUserRequest(Request userRequest)
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

        /// <summary>
        /// Получить запрос пользователя
        /// </summary>
        /// <param name="requestId">Идентификатор пользователя</param>
        /// <returns></returns>
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

        /// <summary>
        /// Получить типы выполненных запросов
        /// </summary>
        /// <param name="requestObjectId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Обновление вин кода авто
        /// </summary>
        /// <param name="requestObjectId">Идентификатор объекта запроса</param>
        /// <param name="vin">Вин код</param>
        /// <returns></returns>
        private async Task UpdateVinCode(int requestObjectId, string vin)
        {
            try
            {
                using (var rep = _repositoryFactory.CreateBotRepository())
                {
                    await rep.UpdateVinCode(requestObjectId, vin).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        #endregion DBQueries


    }
}
