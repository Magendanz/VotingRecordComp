using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using VotingRecordComp.Models;

namespace VotingRecordComp.Services
{
    public class LegislationService
    {
        static public List<LegislationInfo> GetLegislationByYear(int year)
        {
            var response = GetResponse($"/LegislationService.asmx/GetLegislationByYear?year={year}");
            var xmlSer = new DataContractSerializer(typeof(List<LegislationInfo>));
            return xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<LegislationInfo>;
        }

        static public List<RollCall> GetRollCalls(string biennium, int billNumber)
        {
            var response = GetResponse($"/LegislationService.asmx/GetRollCalls?biennium={biennium}&billNumber={billNumber}");
            var xmlSer = new DataContractSerializer(typeof(List<RollCall>));
            return xmlSer.ReadObject(response.Content.ReadAsStreamAsync().Result) as List<RollCall>;
        }

        static public HttpResponseMessage GetResponse(string requestUri)
        {
            var retryCount = 3;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://wslwebservices.leg.wa.gov");
  retry:
                try
                {
                    return client.GetAsync(requestUri).Result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Thread.Sleep(10000);    // Wait for 10 seconds, just to give server a breath
                    if (retryCount-- > 0)
                        goto retry;

                    Debug.WriteLine("Retries exhausted! Request: {requestUri}");
                    return new HttpResponseMessage();
                }
            }
        }
    }
}
