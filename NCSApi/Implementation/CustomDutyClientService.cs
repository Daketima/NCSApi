
using Microsoft.Extensions.Options;
using NCSApi.Config;
using NCSApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCSApi.Service
{
    public class CustomDutyClientService : ICustomDutyClient
    {

        readonly HttpClient kaoshiClient;
        readonly DutyConfig _dutyConfig;
        HttpClient CustomClient => kaoshiClient;

        HttpClient ICustomDutyClient.CustomClient => kaoshiClient;
        public CustomDutyClientService(HttpClient httpClient, DutyConfig dutyConfig)
        {
            _dutyConfig = dutyConfig;
           
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{_dutyConfig.ClientUsername}:{_dutyConfig.ClientPassword}");
            string val = System.Convert.ToBase64String(plainTextBytes);

            kaoshiClient = httpClient;
            kaoshiClient.BaseAddress = new Uri(_dutyConfig.BaseUrl);
            kaoshiClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
        }

        public Task<HttpResponseMessage> SendKaoshiRequest(HttpRequestMessage requestMessage)
        {
            return CustomClient.SendAsync(requestMessage);
        }

    }
}
