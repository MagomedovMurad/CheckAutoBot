using HtmlAgilityPack;
using CheckAutoBot.Infrastructure.Extensions;
using CheckAutoBot.RsaModels;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Linq;
using CheckAutoBot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace CheckAutoBot.Managers
{
    public class Rsa
    {
        public const string dataSiteKey = "6Lf2uycUAAAAALo3u8D10FqNuSpUvUXlfP7BzHOk";

        public const string policyInfoUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policyInfo.htm";
        public const string policyInfoStatusUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/checkPolicyInfoStatus.htm";
        public const string policyInfoDataUrl = "https://dkbm-web.autoins.ru/dkbm-web-1.0/policyInfoData.htm";

        private string Test(string url, string method, WebHeaderCollection headers, string data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Headers = headers;

            if (data != null)
            {
                var binaryData = Encoding.ASCII.GetBytes(data);
                request.AddContent(binaryData);
            }

            var response = request.GetResponse();
            var result = response.ReadDataAsString();
            response.Close();

            return result;
        }
        

        public PolicyTask CreateTask(DateTime requestDate,
                         string captcha,
                         string bsoseries = null, 
                         string bsonumber = null, 
                         string vin = null, 
                         string licensePlate = null, 
                         string bodyNumber = null,
                         string chassisNumber = null)
        {
            var data = $"bsoseries={bsoseries}&" +
                       $"bsonumber={bsonumber}&" +
                       $"isBsoRequest={bsonumber!=null}&" +
                       $"requestDate={requestDate.ToShortDateString()}&" +
                       $"vin={vin}&" +
                       $"licensePlate={WebUtility.UrlEncode(licensePlate)}&" +
                       $"bodyNumber={bodyNumber}&" +
                       $"chassisNumber={chassisNumber}&" +
                       $"captcha={captcha}";

            #region Headers
            var headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "dkbm-web.autoins.ru");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.Accept, "application/json");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add("Origin", "https://dkbm-web.autoins.ru");
            headers.Add("Referer", policyInfoUrl);
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add("X-Requested-With", "XMLHttpRequest");
            #endregion

            var result = Test(policyInfoUrl, "POST", headers, data);
            return JsonConvert.DeserializeObject<PolicyTask>(result);
        }

        public PolicyTaskStatus GetStatus(string processId)
        {
            var url = policyInfoStatusUrl + $"?processId={processId}";

            var headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "dkbm-web.autoins.ru");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add("Origin", "https://dkbm-web.autoins.ru");
            headers.Add("Referer", policyInfoUrl);
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add("X-Requested-With", "XMLHttpRequest");

            var result = Test(url, "GET", headers, null);
            return JsonConvert.DeserializeObject<PolicyTaskStatus>(result);
        }

        public IEnumerable<Policy> GetResult(string processId)
        {
            var data = $"processId={processId}";

            var headers = new WebHeaderCollection();
            headers.Add(HttpRequestHeader.Host, "dkbm-web.autoins.ru");
            headers.Add(HttpRequestHeader.Connection, "keep-alive");
            headers.Add(HttpRequestHeader.ContentLength, data.Length.ToString());
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add("Origin", "https://dkbm-web.autoins.ru");
            headers.Add("Referer", policyInfoUrl);
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            headers.Add("X-Requested-With", "XMLHttpRequest");

            var result = Test(policyInfoDataUrl, "POST", headers, data);
            return Parse(result);
        }



        public PolicyResponse GetPolicy(string captcha, DateTime date, string lp = "", string vin = "", string bodyNumber = "", string chassisNumber = "")
        {
            var stringData = $"vin={vin}&lp={lp.UrlEncode()}&date={date.Date.ToString("dd.MM.yyyy")}&bodyNumber={bodyNumber}&chassisNumber={chassisNumber}&captcha={captcha}";
            //string json = ExecuteRequest(stringData, policyUrl, "POST");
            return JsonConvert.DeserializeObject<PolicyResponse>(null);
        }

        public VechicleResponse GetVechicleInfo(string serial, string number, DateTime date, string captcha)
        {
            var stringData = $"serialOsago={serial.UrlEncode()}&numberOsago={number}&dateRequest={date.Date.ToString("dd.MM.yyyy")}&captcha={captcha}";

            //string json = ExecuteRequest(stringData, osagoVehicleUrl, "POST");

            return JsonConvert.DeserializeObject<VechicleResponse>(null);
        }

        private string ParseVehicle(HtmlNodeCollection nodes, string header)
        {
            var vehicleField = nodes.FirstOrDefault(x => x.SelectSingleNode("td[@class='table-td-header']")
                                                         .InnerText
                                                         .StartsWith(header));
            return vehicleField?.SelectSingleNode("td[2]")?.InnerText;
        }
        private IEnumerable<Policy> Parse(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection policyNodes = doc.DocumentNode.SelectNodes(".//tr[@class='data-row']");
            foreach (var node in policyNodes)
            {
                var fields = node.SelectNodes("td");

                var vehicleNodes = fields[5].SelectNodes("div/table/tr");

                var vehicle = new Vehicle()
                {
                    ModelAndCategory = ParseVehicle(vehicleNodes, "Марка и модель транспортного средства"),
                    LicensePlate = ParseVehicle(vehicleNodes, "Государственный регистрационный знак"),
                    Vin = ParseVehicle(vehicleNodes, "VIN"),
                    EnginePower = ParseVehicle(vehicleNodes, "Мощность двигателя для категории")
                };

                yield return new Policy()
                {
                    Id = int.Parse(fields[0].InnerText),
                    Number = fields[1].InnerText,
                    Company = fields[2].InnerText,
                    OsagoContractStatus = fields[3].InnerText,
                    ActiveOnRequestDate = fields[4].InnerText,
                    Vehicle = vehicle,
                    FollowsToRegistrationPlace = fields[6].InnerText,
                    HasTrailer = fields[7].InnerText,
                    UsePpurpose = fields[8].InnerText,
                    HasRestricted = fields[9].InnerText,
                    Policyholder = fields[10].InnerText,
                    Owner = fields[11].InnerText,
                    KBM = fields[12].InnerText,
                    Region = fields[13].InnerText,
                    InsurancePremium = fields[14].InnerText
                };
            }
        }
    }
}

public class RsaManager1
{
    private Rsa _rsa;
    private Dictionary<string, TaskCompletionSource<IEnumerable<Policy>>> _processes;
    private Timer _timer;

    public RsaManager1()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();
    }
    /// <summary>
    /// Запрос списка полисов
    /// </summary>
    /// <param name="licensePlate">Гос. номер автомобиля</param>
    /// <param name="captcha">Каптча</param>
    /// <returns></returns>
    public Task<IEnumerable<Policy>> GetPolicies(string licensePlate, string captcha)
    {
        var tcs = new TaskCompletionSource<IEnumerable<Policy>>();
        var task = _rsa.CreateTask(DateTime.Now, captcha, licensePlate: licensePlate);
        if (!task.ValidCaptcha)
            throw new Exception("Неправильно решена каптча (переделать этот механизм)");
            _processes.Add(task.ProcessId, tcs);
        return tcs.Task;
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        foreach (var process in _processes)
        {
            var status = _rsa.GetStatus(process.Key);
            if (status.RequestStatusInfo.RequestStatusCode == (int)RequestStatus.Success)
                RequestAndSetResult(process.Key, process.Value);
            else if (status.RequestStatusInfo.RequestStatusCode == (int)RequestStatus.NotFound)
                process.Value.SetResult(null);
            else
                process.Value.SetException(new Exception(status.ErrorList.First()));
        }
    }

    private void RequestAndSetResult(string processId, TaskCompletionSource<IEnumerable<Policy>> task)
    {
        try
        {
            var result = _rsa.GetResult(processId);
            task.SetResult(result);
        }
        catch (Exception ex)
        {
            task.SetException(ex);
        }
    }
}

    public class Policy
    {
        /// <summary>
        /// Порядковый номер
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Серия и номер договора ОСАГО
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Наименование страховой организации
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Статус договора ОСАГО
        /// </summary>
        public string OsagoContractStatus { get; set; }

        /// <summary>
        /// Срок действия и период использования транспортного средства договора ОСАГО
        /// </summary>
        public string ActiveOnRequestDate { get; set; }

        /// <summary>
        /// Сведения о транспортном средстве
        /// </summary>
        public Vehicle Vehicle { get; set; }

        /// <summary>
        /// Транспортное средство следует к месту регистрации или к месту проведения технического осмотра
        /// </summary>
        public string FollowsToRegistrationPlace { get; set; }

        /// <summary>
        /// Управление транспортным средством с прицепом
        /// </summary>
        public string HasTrailer { get; set; }

        /// <summary>
        /// Цель использования транспортного средства
        /// </summary>
        public string UsePpurpose { get; set; }

        /// <summary>
        /// Договор ОСАГО с ограничениями/без ограничений лиц, допущенных к управлению транспортным средством
        /// </summary>
        public string HasRestricted { get; set; }

        /// <summary>
        /// Сведения о страхователе транспортного средства
        /// </summary>
        public string Policyholder { get; set; }

        /// <summary>
        /// Сведения о собственнике транспортного средства
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// КБМ по договору ОСАГО
        /// </summary>
        public string KBM { get; set; }

        /// <summary>
        /// Транспортное средство используется в регионе
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Страховая премия
        /// </summary>
        public string InsurancePremium { get; set; }
    }
    public class Vehicle
    {
        /// <summary>
        /// Марка и модель транспортного средства (категория "Х")
        /// </summary>
        public string ModelAndCategory { get; set; }

        /// <summary>
        /// Государственный регистрационный знак
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// VIN
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// Мощность двигателя для категории B, л.с.
        /// </summary>
        public string EnginePower { get; set; }
    }

    public class PolicyTask
    {
        [JsonProperty(PropertyName = "validCaptcha")]
        public bool ValidCaptcha { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "warningMessage")]
        public string WarningMessage { get; set; }

        [JsonProperty(PropertyName = "errorId")]
        public int ErrorId { get; set; }

        [JsonProperty(PropertyName = "processId")]
        public string ProcessId { get; set; }

        [JsonProperty(PropertyName = "invalidFields")]
        public string[] InvalidFields { get; set; }
    }
    public class PolicyTaskStatus
    {
        [JsonProperty(PropertyName = "RequestId")]
        public ulong RequestId { get; set; }

        [JsonProperty(PropertyName = "RequestStatusInfo")]
        public RequestStatusInfo RequestStatusInfo { get; set; }

        [JsonProperty(PropertyName = "ErrorList")]
        public string[] ErrorList { get; set; }
    }
    public class RequestStatusInfo
    {
        [JsonProperty(PropertyName = "RequestStatusCode")]
        public int RequestStatusCode { get; set; }

        [JsonProperty(PropertyName = "RequestStatusType")]
        public string RequestStatusType { get; set; }
    }
    public enum RequestStatus
{ 
    Success = 3,
    NotFound = 14
}

