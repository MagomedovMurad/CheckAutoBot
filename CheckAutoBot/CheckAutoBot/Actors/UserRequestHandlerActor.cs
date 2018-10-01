using Akka.Actor;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Messages;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;
using CheckAutoBot.Vk.Api.MessagesModels;
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
        private Rsa _rsaManager;
        private Gibdd _gibddManager;
        private ReestrZalogov _fnpManager;
        private Rucaptcha _rucaptchaManager;
        private KeyboardBuilder _keyboardBuilder;
        private ICanSelectActor _actorSelector;

        private List<CacheItem> _captchaCacheItems = new List<CacheItem>();

        public UserRequestHandlerActor()
        {
            _repositoryFactory = new RepositoryFactory();
            _rsaManager = new Rsa();
            _gibddManager = new Gibdd();
            _fnpManager = new ReestrZalogov();
            _rucaptchaManager = new Rucaptcha();
            _keyboardBuilder = new KeyboardBuilder();
            _actorSelector = new ActorSelector();

            ReceiveAsync<UserRequestMessage>(x => UserRequestHandler(x));
            ReceiveAsync<UserInputDataMessage>(x => UserInputDataMessageHandler(x));
            ReceiveAsync<CaptchaResponseMessage>(x => CaptchaResponseMessageHadler(x));
        }

        private async Task<bool> UserRequestHandler(UserRequestMessage message)
        {
            var lastUserRequestObject = await GetLastUserRequestObject(message.UserId);

            var userRequest = new Request()
            {
                RequestObjectId = lastUserRequestObject.Id,
                Type = message.RequestType
            };

            var requestId = await AddUserRequest(userRequest);

            switch (message.RequestType)
            {
                case RequestType.History:
                    PreGetHistory(lastUserRequestObject as Auto, requestId.Value);
                    break;
                case RequestType.Dtp:
                    PreGetDtp(lastUserRequestObject as Auto);
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
            var captchaItem = _captchaCacheItems.FirstOrDefault(x => x.CaptchaId == message.CaptchaId);

            if (captchaItem == null)
                return true;

            captchaItem.CaptchaWord = message.Value;

            var items = _captchaCacheItems.Where(x => x.RequestId == captchaItem.RequestId);

            var isNotCompleted = items.Any(x => string.IsNullOrEmpty(x.CaptchaWord));

            if (!isNotCompleted)
            {
                var request = await GetUserRequest(captchaItem.RequestId);

                switch (request.Type)
                {
                    case RequestType.History:
                        GetHistory(request.RequestObject as Auto, items);
                        break;
                    case RequestType.Dtp:
                        GetDtp();
                        break;
                }

            }

            return true;
        }

        private void PreGetHistory(Auto auto, int userRequestId)
        {
            if (string.IsNullOrEmpty(auto.Vin))
            {
                var policyNumberCaptchaRequest = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl);
                var getPolicyNumberCacheItem = new CacheItem()
                {
                    RequestId = userRequestId,
                    CaptchaId = policyNumberCaptchaRequest.Id,
                    ActionType = ActionType.PolicyNumber,
                    JSessionId = null,
                    Date = DateTime.Now
                };
                _captchaCacheItems.Add(getPolicyNumberCacheItem);

                var policyInfoCaptchaRequest = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl);
                var getPolicyInfoCacheItem = new CacheItem()
                {
                    RequestId = userRequestId,
                    CaptchaId = policyInfoCaptchaRequest.Id,
                    ActionType = ActionType.PolicyInfo,
                    JSessionId = null,
                    Date = DateTime.Now
                };
                _captchaCacheItems.Add(getPolicyInfoCacheItem);
            }

            var captchaResult = _gibddManager.GetCaptcha();
            var historyCaptchaRequest = _rucaptchaManager.SendImageCaptcha(captchaResult.ImageBase64);
            var getHistoryCacheItem = new CacheItem()
            {
                RequestId = userRequestId,
                CaptchaId = historyCaptchaRequest.Id,
                ActionType = ActionType.History,
                JSessionId = captchaResult.JsessionId,
                Date = DateTime.Now
            };
            _captchaCacheItems.Add(getHistoryCacheItem);
        }

        private void GetHistory(Auto auto, IEnumerable<CacheItem> cacheItems)
        {
            var vin = auto.Vin;

            if (string.IsNullOrEmpty(auto.Vin))
            {
                var item1 = cacheItems.First(x => x.ActionType == ActionType.PolicyNumber);
                var policyResponse = _rsaManager.GetPolicy(item1.CaptchaWord, DateTime.Now, auto.LicensePlate);

                var policy = policyResponse.Policies.First();
                var item2 = cacheItems.First(x => x.ActionType == ActionType.PolicyInfo);
                var vechicleResponse = _rsaManager.GetPolicyInfo(policy.Serial, policy.Number, DateTime.Now, item2.CaptchaWord);

                vin = vechicleResponse.Vin;
            }
            var item3 = cacheItems.First(x => x.ActionType == ActionType.History);
            var gibddResponse = _gibddManager.GetHistory(vin, item3.CaptchaWord, item3.JSessionId);

            var messageParams = new SendMessageParams()
            {
                Message = HistoryToMessageText(gibddResponse.RequestResult),
                PeerId = auto.UserId,
                AccessToken = "374c755afe8164f66df13dc6105cf3091ecd42dfe98932cd4a606104dc23840882d45e8b56f0db59e1ec2"
        };

            Vk.Api.Messages.Send(messageParams);
        }

        private string HistoryToMessageText(HistoryResult history)
        {
            return $"Марка, модель:  {history.Vehicle.Model} \n" +
            $"Год выпуска: {history.Vehicle.Year} \n" +
            $"VIN:  {history.Vehicle.Vin} \n" +
            $"Кузов:  {history.Vehicle.BodyNumber} \n" +
            $"Цвет: {history.Vehicle.Color} \n" +
            $"Рабочий объем(см3):  {history.Vehicle.EngineVolume} \n" +
            $"Мощность(кВт/л.с.):  {history.Vehicle.PowerHp} \n" +
            $"Тип:  {history.Vehicle.TypeName} \n" +
            $"Категория: {history.Vehicle.Category}";
        }

        private void PreGetDtp(Auto auto)
        {
          //  if (string.IsNullOrEmpty(auto.Vin))
        }

        private void GetDtp()
        {

        }
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

        #endregion DBQueries


    }
}
