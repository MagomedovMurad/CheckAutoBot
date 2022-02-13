using CheckAutoBot.DataSources.Contracts;
using CheckAutoBot.DataSources.Models;
using CheckAutoBot.Exceptions;
using CheckAutoBot.Infrastructure.Enums;
using CheckAutoBot.Infrastructure.Models.DataSource;
using CheckAutoBot.Managers;
using CheckAutoBot.Models.Captcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CheckAutoBot.DataSources
{
    public class RsaDataSource : IDataSourceWithCaptcha
    {
        private RucaptchaManager _rucaptchaManager;
        private RsaManager _rsaManager;

        public RsaDataSource(RucaptchaManager rucaptchaManager,
                             RsaManager rsaManager)
        {
            _rucaptchaManager = rucaptchaManager;
            _rsaManager = rsaManager;
        }

        public string Name => "RSA_VECHICLE_IDENTIFIERS";

        public DataType DataType => DataType.VechicleIdentifiersRSA;

        public int MaxRepeatCount => 3;

        public int Order => 1;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = inputData as string;
            var pi = captchaRequestItems.Single(x => x.Key == "pi");
            //var pn = captchaRequestItems.Single(x => x.Key == "pn");
            //var ov = captchaRequestItems.Single(x => x.Key == "ov");
            var rsa = new Rsa();
            var policyTask = rsa.CreateTask(DateTime.Now, "asdfasdf", licensePlate: "Р928УТ26");
            Thread.Sleep(1000);
            var status = rsa.GetStatus(policyTask.ProcessId);
            var t = rsa.GetResult(policyTask.ProcessId);

           VechicleIdentifiersData vechicleIdentifiers = null;

            // var policyResponse = _rsaManager.GetPolicy(pn.Value, DateTime.Now, lp: licensePlate);

            //if (policyResponse is null)
            //    return new DataSourceResult(null);
            //  //  throw new DataNotFoundException();

            //var policy = policyResponse.Policies.FirstOrDefault();
            //var vechicleResponse = _rsaManager.GetVechicleInfo(policy.Serial, policy.Number, DateTime.Now, ov.Value);

            //vechicleIdentifiers = new VechicleIdentifiersData() { Vin = vechicleResponse.Vin, FrameNumber = vechicleResponse.BodyNumber, LicensePlate = vechicleResponse.LicensePlate };
            //return new DataSourceResult(vechicleIdentifiers);
            return null;
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequestPI = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyInfoUrl, Rucaptcha.LpPingbackUrl);
            //var captchaRequestPN = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl, Rucaptcha.LpPingbackUrl);
            //var captchaRequestOV = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl, Rucaptcha.LpPingbackUrl);

            return new[]
            {
                new CaptchaRequestData(captchaRequestPI.Id, null, "pi" ),
                //new CaptchaRequestData(captchaRequestPN.Id, null, "pn" ),
                //new CaptchaRequestData(captchaRequestOV.Id, null, "ov")
            };
        }
    }
}
