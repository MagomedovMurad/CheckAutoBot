﻿using CheckAutoBot.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.DataSources.Models
{
    public class DataSourceResult
    {
        public DataSourceResult(object data, IEnumerable<RelatedData> relatedData)
        {
            Data = data;
            RelatedData = relatedData;
        }

        public DataSourceResult(object data)
        {
            Data = data;
        }

        //public DataSourceResult()
        //{
        //}

        public object Data { get; set; }

        public IEnumerable<RelatedData> RelatedData { get; set; }
    }
}
