using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Core
{
    public class CheckDto
    {
        //Guid assessmentId, string user
        public string LoggedInUser { get; set; }
        public Guid AssessmentId { get; set; }
    }
    public class PaymentCheckDto
    {
        //Guid assessmentId, string user
        public string LoggedInUser { get; set; }
        public string PaymentReference { get; set; }
    }
}
