using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCSApi.Core
{
    public interface ICustomDutyClient
    {

        HttpClient CustomClient { get; }
        Task<HttpResponseMessage> SendKaoshiRequest(HttpRequestMessage requestMessage);
    }
}
