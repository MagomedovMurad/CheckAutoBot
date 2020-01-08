using CaptchaSolver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaptchaSolver
{
    public interface ICacheController
    {
        void Add(string id, byte[] page, string pingback);
        byte[] GetPage(string id);
        string GetPingback(string id);
    }
    public class CacheController: ICacheController
    {
        private List<Request> _requests;
        private uint _counter = 0;

        public CacheController()
        {
            _requests = new List<Request>();
        }

        public void Add(string id, byte[] page, string pingback)
        {
            var request = new Request()
            {
                Id = id,
                Page = page,
                Pingback = pingback
            };

            _requests.Add(request);
        }

        public byte[] GetPage(string id)
        {
            var request = _requests.Single(x => x.Id == id);
            return request.Page;
        }

        public string GetPingback(string id)
        {
            var request = _requests.Single(x => x.Id == id);
            return request.Pingback;
        }
    }
}
