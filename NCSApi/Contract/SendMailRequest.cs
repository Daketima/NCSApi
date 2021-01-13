using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class SendMailRequest
    {        
            public string ToAddress { get; set; }
            public string Subject { get; set; }
            public string MessageBody { get; set; }
            public string Bcc { get; set; }
            public string Cc { get; set; }
       

    }
}
