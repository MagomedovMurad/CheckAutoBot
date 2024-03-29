﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckAutoBot.Captcha;
using CheckAutoBot.Contracts;
using CheckAutoBot.Enums;
using CheckAutoBot.GbddModels;
using CheckAutoBot.Managers;
using CheckAutoBot.Storage;
using CheckAutoBot.Utils;

namespace CheckAutoBot.Handlers
{
    public class HistoryHandler : GibddHandler, IHandler
    {
        public ActionType SupportedActionType => ActionType.History;

        public HistoryHandler(GibddManager gibddManager, 
                              RucaptchaManager rucaptchaManager) : base(gibddManager, rucaptchaManager)
        {
        }

        public Dictionary<string, byte[]> Get(RequestObject requestObject, IEnumerable<CaptchaCacheItem> cacheItems)
        {
            var auto = requestObject as Auto;

            var historyCacheItem = cacheItems.First(x => x.CurrentActionType == ActionType.History);
            var historyResult = _gibddManager.GetHistory(auto.Vin, historyCacheItem.CaptchaWord, historyCacheItem.SessionId);
            return GenerateResponse(historyResult);
        }

        private Dictionary<string, byte[]> GenerateResponse(HistoryResult result)
        {
            string text = null;

            if(result == null)
                text = "В базе ГИБДД не найдены сведения о регистрации транспортного средства";
            else
                text = HistoryToMessageText(result);

            return new Dictionary<string, byte[]>() { { text, null } };
        }

        private string HistoryToMessageText(HistoryResult history)
        {
            var text = $"Марка, модель:  {history.Vehicle.Model}{Environment.NewLine}" +
            $"Год выпуска: {history.Vehicle.Year}{Environment.NewLine}" +
            $"VIN:  {history.Vehicle.Vin}{Environment.NewLine}" +
            $"Кузов:  {history.Vehicle.BodyNumber}{Environment.NewLine}" +
            $"Цвет: {history.Vehicle.Color}{Environment.NewLine}" +
            $"Рабочий объем(см3):  {history.Vehicle.EngineVolume}{Environment.NewLine}" +
            $"Мощность(кВт/л.с.):  {history.Vehicle.PowerKwt ?? "н.д."}/{history.Vehicle.PowerHp}{Environment.NewLine}" +
            $"Тип:  {history.Vehicle.TypeName}{Environment.NewLine}" +
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
    }
}
