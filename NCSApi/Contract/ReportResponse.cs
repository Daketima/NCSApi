using DataLayer.Data;
using DataLayer.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class ReportResponse
    {
        private readonly CustomContext _dbContext;
        public ReportResponse() { _dbContext = new CustomContext(); }
        public Guid Id { get; set; }
        public string Year { get; set; }
        public string CustomsCode { get; set; }
        public string CompanyCode { get; set; }
        public string DeclarantCode { get; set; }
        public string AssessmentSerial { get; set; }
        public string AssessmentNumber { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public DateTime DateCreated { get; set; }
        public string FormMNumber
        {
            get
            {
                PaymentLog pay = _dbContext.Payment.Find(PaymentLogId);
                Assessment ass = _dbContext.Assessment.Find(Guid.Parse(pay?.AssessmentId));

                return ass?.FormMNumber ?? "Form M not Available";
            }
        }

        public Guid PaymentLogId { get; set; }

        public PaymentResponse PaymentDetail { get; set; }
    }
}
