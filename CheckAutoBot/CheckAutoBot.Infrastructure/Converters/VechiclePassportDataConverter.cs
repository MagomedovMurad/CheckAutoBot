using CheckAutoBot.Infrastructure.Contracts;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models;
using CheckAutoBot.Infrastructure.Models.DataSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Converters
{
    public class VechiclePassportDataConverter : IDataConverter
    {
        public DataType SupportedDataType => DataType.VechiclePassportData;

        public IEnumerable<ConvertedDataBag> Convert(object sourceData)
        {
            var data = sourceData as VechiclePassportData;
            var message = ConvertToStringMessage(data);
            return new[] { new ConvertedDataBag()
            {
                Message = message
            }};
        }

        private string ConvertToStringMessage(VechiclePassportData vechiclePassport)
        {
            var text = $"📜 Данные по ПТС:{Environment.NewLine}" +
                       $"Марка, модель:  {vechiclePassport.Model}{Environment.NewLine}" +
                       $"Год выпуска: {vechiclePassport.Year}{Environment.NewLine}" +
                       $"VIN:  {vechiclePassport.Vin}{Environment.NewLine}" +
                       $"Кузов:  {vechiclePassport.FrameNumber}{Environment.NewLine}" +
                       $"Цвет: {vechiclePassport.Color}{Environment.NewLine}" +
                       $"Рабочий объем(см3):  {vechiclePassport.EngineVolume}{Environment.NewLine}" +
                       $"Мощность(кВт/л.с.):  {vechiclePassport.PowerKwt ?? "н.д."}/{vechiclePassport.PowerHp}{Environment.NewLine}" +
                       $"Тип:  {vechiclePassport.Type}{Environment.NewLine}" +
                       $"Категория: {vechiclePassport.Category}{Environment.NewLine}" +
                       $"Номер двигателя: {vechiclePassport.EngineNumber}{Environment.NewLine}" +
                       $"Номер ПТС: {vechiclePassport.PTSNumber}{Environment.NewLine}" +
                       $"Название организации, выдавшей ПТС: {vechiclePassport.CompanyName}";

            return text;
        } 
    }
}
