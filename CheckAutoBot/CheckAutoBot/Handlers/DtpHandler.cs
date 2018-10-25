using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class DtpHandler : GibddHandler, IHandler
    {
        public ActionType SupportedActionType => ActionType.Dtp;

        public DtpHandler(GibddManager gibddManager,
                          RucaptchaManager rucaptchaManager) : base(gibddManager, rucaptchaManager)
        {
        }

        public Dictionary<string, byte[]> Get(RequestObject requestObject, IEnumerable<CacheItem> cacheItems)
        {
            var auto = requestObject as Auto;

            var dtpCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.Dtp);
            var dtpResult = _gibddManager.GetDtp(auto.Vin, dtpCacheItem.CaptchaWord, dtpCacheItem.SessionId);

            return GenerateResponse(dtpResult);
        }

        private Dictionary<string, byte[]> GenerateResponse(DtpResult result)
        {
            var messages = new Dictionary<string, byte[]>();

            if (result == null)
            {
                var text = "В базе ГИБДД не найдены сведения о дорожно-транспортных происшествиях";
                messages.Add(text, null);
            }
            else
            {
                foreach (var accident in result.Accidents)
                {
                    var incidentImage = _gibddManager.GetIncidentImage(accident.DamagePoints);
                    var text = AccidentToMessageText(accident);
                    messages.Add(text, incidentImage);
                }
            }

            return messages;
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





    }
}
