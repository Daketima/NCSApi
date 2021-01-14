using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class DeclineRequest
    {       
        public string PaymentRefernce { get; set; }

        public string Comment { get; set; }
        public string LoggedInUser { get; set; }
    }
}
