using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Infrastructure.Contracts
{
    public interface IDataConverter
    {
        DataType SupportedDataType { get; }

        IEnumerable<ConvertedDataBag> Convert(object sourceData);
    }
}
