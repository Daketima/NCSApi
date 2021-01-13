using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class AccountDetailResponse
    {
        public string ResponseMessage { get; set; }
        public string ResponseDescription { get; set; }
        public Response Response { get; set; }

    }
    public class Response
    {
        public string CustomerID { get; set; }
        public string AccountName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Currency { get; set; }
        public float AvailableBalance { get; set; }
        public float ReservedAmount { get; set; }
        public object BVN { get; set; }
        public string IsClosed { get; set; }
        public DateTime AccountOpenDate { get; set; }
        public string CreatedBy { get; set; }
        public string IBAN { get; set; }
        public string IsJointAccount { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ProductCode { get; set; }
        public string ProductDesc { get; set; }
        public string AccountStatus { get; set; }
    }
}
