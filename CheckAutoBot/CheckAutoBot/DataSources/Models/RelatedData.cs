using CheckAutoBot.Enums;
using CheckAutoBot.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources.Models
{
    public class RelatedData
    {
        public RelatedData(object data, DataType dataType)
        {
            Data = data;
            DataType = dataType;
        }
        public object Data { get; set; }

        public DataType DataType { get; set; }
    }
}
