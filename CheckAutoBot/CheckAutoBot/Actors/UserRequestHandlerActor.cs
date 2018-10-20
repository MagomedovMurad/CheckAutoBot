using Akka.Actor;
using CheckAutoBot.EaistoModels;
using CheckAutoBot.Enums;
using CheckAutoBot.Exceptions;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.PledgeModels;
using CheckAutoBot.RsaModels;
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


        private RsaManager _rsaManager;
        private GibddManager _gibddManager;
        private FnpManager _fnpManager;
        private EaistoManager _eaistoManager;
        private RucaptchaManager _rucaptchaManager;

        private List<CacheItem> _captchaCacheItems = new List<CacheItem>();

        public UserRequestHandlerActor(ILogger logger)
        {
            _repositoryFactory = new RepositoryFactory();
            _rsaManager = new RsaManager();
            _gibddManager = new GibddManager();
            _fnpManager = new FnpManager();
            _rucaptchaManager = new RucaptchaManager();
            _eaistoManager = new EaistoManager();
            _random = new Random();
            _actorSelector = new ActorSelector();

            _logger = logger;

            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x =>  CaptchaResponseMessageHadler(x));
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

                var text = $"{message.Data}{Environment.NewLine}Выберите доступные действия...";
                var msg = new SendToUserMessage(null, message.UserId, text, null);

               _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path).Tell(msg, Self);

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

            var isNotCompleted = items.Any(x => string.IsNullOrWhiteSpace(x.CaptchaWord));

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
                        case ActionType.Dtp:
                            await GetDtp(auto, items);
                            break;
                        case ActionType.Restricted:
                            await GetRestricted(auto, items);
                            break;
                        case ActionType.Wanted:
                            await GetWanted(auto, items);
                            break;
                        case ActionType.Pledge:
                            await GetPledge(auto, items);
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
            AddCaptchaRequestToCache(userRequestId, captchaRequest1.Id, ActionType.PolicyNumber, targetActionType);
            var captchaRequest2 = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl);
            AddCaptchaRequestToCache(userRequestId, captchaRequest2.Id, ActionType.PolicyInfo, targetActionType);
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

            var diagnosticCardCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.VinByDiagnosticCard);
            var diagnosticCard = _eaistoManager.GetDiagnosticCard(diagnosticCardCacheItem.CaptchaWord, phoneNumber, diagnosticCardCacheItem.SessionId, licensePlate: licensePlate);

            if (diagnosticCard == null)
            {
                PreGetVinByPolicy(diagnosticCardCacheItem.RequestId, diagnosticCardCacheItem.TargetActionType);
            }
            else
            {
                var request = await GetUserRequest(diagnosticCardCacheItem.RequestId);
                await UpdateVinCode(request.RequestObject.Id, diagnosticCard.Vin);
                if (diagnosticCardCacheItem.TargetActionType == ActionType.Pledge)
                    PreGetFromFnp(true, diagnosticCardCacheItem.RequestId, diagnosticCardCacheItem.TargetActionType);
                else
                    PreGetFromGibdd(true, diagnosticCardCacheItem.RequestId, diagnosticCardCacheItem.TargetActionType);
            }
        }

        private async void GetVinByPolicy(IEnumerable<CacheItem> cacheItems, string licensePlate)
        {
            var policyNumberCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.PolicyNumber);
            var policyResponse = _rsaManager.GetPolicy(policyNumberCacheItem.CaptchaWord, DateTime.Now, lp: licensePlate);

            VechicleResponse vechicleResponse = null;

            if (policyResponse != null)
            {
                var policy = policyResponse.Policies.FirstOrDefault();
                var policyInfoCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.PolicyInfo);
                vechicleResponse = _rsaManager.GetVechicleInfo(policy.Serial, policy.Number, DateTime.Now, policyInfoCacheItem.CaptchaWord);
            }

            if (vechicleResponse != null)
            {
                var request = await GetUserRequest(policyNumberCacheItem.RequestId);
                await UpdateVinCode(request.RequestObject.Id, vechicleResponse.Vin);

                if (policyNumberCacheItem.TargetActionType == ActionType.Pledge)
                    PreGetFromFnp(true, policyNumberCacheItem.RequestId, policyNumberCacheItem.TargetActionType);
                else
                    PreGetFromGibdd(true, policyNumberCacheItem.RequestId, policyNumberCacheItem.TargetActionType);

            }
            else
            {
                //К сожалению не удалось найти информацию по номеру ... . Попробуйте выполнить поиск по vin коду
            }
        }

        private async Task GetHistory(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            try
            {
                var historyCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.History);
                var historyResult = _gibddManager.GetHistory(auto.Vin, historyCacheItem.CaptchaWord, historyCacheItem.SessionId);

                SendHistoryToSender(historyResult, auto);
            }
            catch (InvalidCaptchaException ex)
            {

            }
            catch (InvalidOperationException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private async Task GetDtp(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var dtpCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.Dtp);
            var dtpResult = _gibddManager.GetDtp(auto.Vin, dtpCacheItem.CaptchaWord, dtpCacheItem.SessionId);

            SendDtpToSender(dtpResult, auto);
        }

        private async Task GetRestricted(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var restrictedCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.Restricted);
            var restrictedResult = _gibddManager.GetRestrictions(auto.Vin, restrictedCacheItem.CaptchaWord, restrictedCacheItem.SessionId);

            SendRestrictedToSender(restrictedResult, auto);
        }
        private async Task GetWanted(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var wantedCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.Wanted);
            var wantedResponse = _gibddManager.GetWanted(auto.Vin, wantedCacheItem.CaptchaWord, wantedCacheItem.SessionId);

            SendWantedToSender(wantedResponse, auto);
        }

        private async Task GetPledge(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            try
            {
                var pledgeCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.Pledge);
                var pledgeResponse = _fnpManager.GetPledges(auto.Vin, pledgeCacheItem.CaptchaWord, pledgeCacheItem.SessionId);

                SendPledgesToSender(pledgeResponse, auto);
            }
            catch (InvalidCaptchaException ex)
            {
            }
            catch (Exception ex)
            {
            }
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

        private void SendHistoryToSender(HistoryResult history, RequestObject requestobject)
        {
            var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            if (history == null)
            {
                var text = "В базе ГИБДД не найдены сведения о регистрации транспортного средства";
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
            else
            {
                var text = HistoryToMessageText(history);
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
        }

        private void SendDtpToSender(DtpResult dtp, RequestObject requestobject)
        {
            var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            if (dtp == null)
            {
                var text = "В базе ГИБДД не найдены сведения о дорожно-транспортных происшествиях";
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
            else
            {
                foreach (var accident in dtp.Accidents)
                {
                    var incidentImage = _gibddManager.GetIncidentImage(accident.DamagePoints);
                    var text = AccidentToMessageText(accident);
                    var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, incidentImage);
                    sender.Tell(message, Self);
                }
            }
        }

        private void SendRestrictedToSender(RestrictedResult result, RequestObject requestobject)
        {
            var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            if (result == null)
            {
                var text = "В базе ГИБДД не найдены сведения о наложении ограничений";
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
            else
            {
                foreach (var restricted in result.Restricteds)
                {
                    var text = RestrictedToMessageText(restricted);
                    var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                    sender.Tell(message, Self);
                }
            }
        }

        private void SendWantedToSender(WantedResult result, RequestObject requestobject)
        {
            var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            if (result == null)
            {
                var text = "В базе ГИБДД не найдены сведения о розыске транспортного средства";
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
            else
            {
                foreach (var wanted in result.Wanteds)
                {
                    var text = WantedToMessageText(wanted);
                    var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                    sender.Tell(message, Self);
                }
            }
        }

        private void SendPledgesToSender(PledgeResponse response, RequestObject requestobject)
        {
            var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);

            if (response == null)
            {
                var text = "В базе ФНП не найдены сведения о нахождении транспортного средства в залоге";
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
            else
            {
                var text = string.Join(Environment.NewLine, response.Pledges.Select(x => PledgeToText(x)));
                var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
                sender.Tell(message, Self);
            }
        }

        private void SendIfVinNotFoudToSender()
        {
            //var sender = _actorSelector.ActorSelection(Context, ActorsPaths.PrivateMessageSenderActor.Path);
            //var text = "В базе ФНП не найдены сведения о нахождении транспортного средства в залоге";
            //var message = new SendToUserMessage(requestobject.Id, requestobject.UserId, text, null);
            //sender.Tell(message, Self);
        }

        private string HistoryToMessageText(HistoryResult history)
        {
            var text = $"Марка, модель:  {history.Vehicle.Model} \n" +
            $"Год выпуска: {history.Vehicle.Year} \n" +
            $"VIN:  {history.Vehicle.Vin} \n" +
            $"Кузов:  {history.Vehicle.BodyNumber} \n" +
            $"Цвет: {history.Vehicle.Color} \n" +
            $"Рабочий объем(см3):  {history.Vehicle.EngineVolume} \n" +
            $"Мощность(кВт/л.с.):  {history.Vehicle.PowerKwt?? "н.д."}/{history.Vehicle.PowerHp} \n" +
            $"Тип:  {history.Vehicle.TypeName} \n" +
            $"Категория: {history.Vehicle.Category}";

            var periods = history.OwnershipPeriodsEnvelop.OwnershipPeriods;
            foreach (var period in periods)
            {
                var ownerType = period.OwnerType == OwnerType.Natural ? "Физическое лицо" : "Юридическое лицо";
                var stringDateTo = period.To.ToString("dd.MM.yyyy");
                var dateTo = stringDateTo == "01.01.0001" ? "настоящее время" : stringDateTo;

                string ownerPeriod = $"{Environment.NewLine}" +
                    $"{Environment.NewLine}{ownerType}{Environment.NewLine}" +
                                     $"c: {period.From.ToString("dd.MM.yyyy")}{Environment.NewLine}" +
                                     $"по: {dateTo}{Environment.NewLine}" +
                                     $"Последняя операция: {period.LastOperation}";
                text += ownerPeriod;
            }

            return text;

        }

        private string AccidentToMessageText(Accident accident)
        {
            return $"Информация о происшествии №{accident.AccidentNumber} {Environment.NewLine}" +
                    $"Дата и время происшествия: {accident.AccidentDateTime} {Environment.NewLine}" +
                    $"Тип происшествия: {accident.AccidentType} {Environment.NewLine}" +
                    $"Регион происшествия: {accident.RegionName} {Environment.NewLine}" +
                    $"Марка ТС: {accident.VehicleMark} {Environment.NewLine}" +
                    $"Модель ТС: {accident.VehicleModel} {Environment.NewLine}" +
                    $"Год выпуска ТС: {accident.VehicleYear}";
        }

        private string RestrictedToMessageText(Restricted restricted)
        {
           return $"Информация о наложении ограничения{Environment.NewLine}" +
                  $"Марка, модель ТС: {restricted.TsModel}{Environment.NewLine}" +
                  $"Год выпуска ТС: {restricted.VechicleYear}{Environment.NewLine}" +
                  $"Дата наложения ограничения: {restricted.RestrictedDate}{Environment.NewLine}" +
                  $"Регион инициатора ограничения: {restricted.RegionName}{Environment.NewLine}" +
                  $"Кем наложено ограничение: {restricted.InitiatorType}{Environment.NewLine}" +
                  $"Вид ограничения: {restricted.RestrictedType}{Environment.NewLine}" +
                  $"Основание ограничения: {restricted.RestrictedFoundations}{Environment.NewLine}" +
                  $"Телефон инициатора: {restricted.InitiatorPhone?? "не указан"}";
        }

        private string WantedToMessageText(Wanted wanted)
        {
            return $"Информация о постановке в розыск{Environment.NewLine}" +
                   $"Марка, модель: {wanted.VechicleModel}{Environment.NewLine}" +
                   $"Год выпуска: {wanted.VechicleYear}{Environment.NewLine}" +
                   $"Дата объявления в розыск: {wanted.Date}{Environment.NewLine}" +
                   $"Регион инициатора розыска: {wanted.RegionIniciator}";
        }

        private string PledgeToText(PledgeListItem pledge)
        {
            var text = $"Уведомление о возникновении залога №{pledge.ReferenceNumber} {Environment.NewLine}";
            text += $"Дата регистрации: {pledge.RegisterDate}{Environment.NewLine}";
            text += $"Залогодатель: {string.Join(Environment.NewLine, pledge.Pledgors.Select(x => PledgorToText(x)))}";
            text += $"Залогодержатель: {string.Join(Environment.NewLine, pledge.Pledgees.Select(x => PledgeeToText(x)))}";
            text += Environment.NewLine;

            return text;
        }

        private string PledgorToText(Pledgor pledgor)
        {
            var text = pledgor.Type == SubjectType.Person ? "Физическое лицо" : "Юридическое лицо";
            return text += Environment.NewLine;
        }

        private string PledgeeToText(Pledgee pledgee)
        {
            var text = pledgee.Type == SubjectType.Organization ? "Юридическое лицо" : "Физическое лицо";
            return text += Environment.NewLine + pledgee.Name + Environment.NewLine;
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
