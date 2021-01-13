using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Config
{
    public class DutyConfig
    {
        public string POCAccount { get; set; }
        
        public string ClientUsername { get; set; }
        
        public string ClientPassword { get; set; }

        public string BalanceEnquiry { get; set; }

        public string NameEnquiry { get; set; }

        public string BaseUrl { get; set; }

        public string IntraTransfer { get; set; }

        public string TillAccount { get; set; }

        public string VATAccount { get; set; }

        public string CustomDutyApiUrl { get; set; }

        public string EmailEndpoint { get; set; }

        public string StaffDetailEndpoint { get; set; }

        public string AccountDetail { get; set; }

        public string ExcisePaymentPath { get; set; }

        public string ExciseResponsePath { get; set; }

        public string EmailWithAttachment { get; set; }
    }

    public enum TransactionStatus
    {
        Initiated = 1,
        Authorized = 2,
        Approved= 3,
        Completed = 4,
        Declined = 5,
        Accepted = 6,
        Pending = 7
    }

    enum AssessmentTypes
    {
        Excise = 1,
        SD = 2,
        SGD = 3
    }
}
