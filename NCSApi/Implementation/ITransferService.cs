using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCSApi.Implementation
{
     public interface ITransferService
    {
        Task<PostingRootObject> PostTransferAsnyc(PostingDetailObj request);
        ByteArrayContent Configure_(object obj, out HttpClient _client);
    }
}
