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

namespace CheckAutoBot.DataSources
{
    public class RsaDataSource : IDataSourceWithCaptcha
    {
        private RucaptchaManager _rucaptchaManager;
        private RsaManager _rsaManager;

        public string Name => "RSA_VECHICLE_IDENTIFIERS";

        public DataType DataType => DataType.VechicleIdentifiersRSA;

        public int MaxRepeatCount => 3;

        public int Order => 2;

        public DataSourceResult GetData(object inputData, IEnumerable<CaptchaRequestData> captchaRequestItems)
        {
            var licensePlate = inputData as string;

            var pn = captchaRequestItems.Single(x => x.Key == "pn");
            var ov = captchaRequestItems.Single(x => x.Key == "ov");

            VechicleIdentifiersData vechicleIdentifiers = null;

            var policyResponse = _rsaManager.GetPolicy(pn.Value, DateTime.Now, lp: licensePlate);

            if (policyResponse is null)
                return new DataSourceResult(null);
              //  throw new DataNotFoundException();

            var policy = policyResponse.Policies.FirstOrDefault();
            var vechicleResponse = _rsaManager.GetVechicleInfo(policy.Serial, policy.Number, DateTime.Now, ov.Value);

            vechicleIdentifiers = new VechicleIdentifiersData() { Vin = vechicleResponse.Vin, FrameNumber = vechicleResponse.BodyNumber, LicensePlate = vechicleResponse.LicensePlate };
            return new DataSourceResult(vechicleIdentifiers);
        }

        public IEnumerable<CaptchaRequestData> RequestCaptcha()
        {
            var captchaRequestPN = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.policyUrl, Rucaptcha.LpPingbackUrl);
            var captchaRequestOV = _rucaptchaManager.SendReCaptcha2(Rsa.dataSiteKey, Rsa.osagoVehicleUrl, Rucaptcha.LpPingbackUrl);

            return new[]
            {
                new CaptchaRequestData(captchaRequestPN.Id, null, "pn" ),
                new CaptchaRequestData(captchaRequestOV.Id, null, "ov")
            };
        }
    }
}
