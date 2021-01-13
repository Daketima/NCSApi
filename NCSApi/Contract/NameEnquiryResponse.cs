using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class NameEnquiryResponse
    {

       
            public string ResponseMessage { get; set; }
            public string ResponseDescription { get; set; }
            public NameEnResponse response { get; set; }
        

        public class NameEnResponse
        {
            public string AccountName { get; set; }
        }

    }
}
