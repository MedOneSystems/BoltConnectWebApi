using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using Polly;
using RestSharp;

namespace MedOne.BoltConnectWebApi
{

  

    public static class BoltConnect
    {
        private class Result
        {
            public string Status { get; set; }
        }

        public static async Task<bool> SendMessages(
            string subscriberId, 
            string customerId,
            IEnumerable<string> messages,  
            string baseUrl = "https://boltconnect.azure-api.net/boltconnecthl7ingest/StoreMessage", 
            int retryCount = 10, 
            int readWriteTimeout = 0, 
            int timeout = 0)
        {
            var res =
                await Policy.HandleResult<IRestResponse<Result>>(r => r.ErrorException != null || !r.IsSuccessful)
                    .WaitAndRetryAsync(retryCount, rec => TimeSpan.FromSeconds(Math.Pow(2, rec)))
                    .ExecuteAsync(() =>
                    {
                        var rc = new RestClient(baseUrl)
                        {
                            AutomaticDecompression = true,
                            FollowRedirects = true,
                            CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache),
                        };

                        if (readWriteTimeout > 0) rc.ReadWriteTimeout = (int) TimeSpan.FromSeconds(readWriteTimeout).TotalMilliseconds;
                        if (timeout > 0) rc.Timeout = (int) TimeSpan.FromSeconds(timeout).TotalMilliseconds;

                        rc.AddDefaultHeader("Ocp-Apim-Subscription-Key", subscriberId);
                        rc.AddDefaultHeader("Accept", "application/json");

                        var rw = new RestRequest(Method.POST)
                        {
                           
                            RequestFormat = DataFormat.Json
                        };

                        rw.AddJsonBody(
                            new
                            {
                                CustomerID = customerId,
                                Data = messages.Select(x => Convert.ToBase64String(Encoding.ASCII.GetBytes(x))).ToList()
                            });

                        return rc.ExecutePostTaskAsync<Result>(rw);
                    });

            if (res.ErrorException != null)
            {
                throw new InvalidOperationException("Error sending messages, see inner exception for details", res.ErrorException);
            }

            return res.IsSuccessful && res.Data.Status.ToUpper() == "OK";
        }
    }

}
