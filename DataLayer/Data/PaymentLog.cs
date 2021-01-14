using System;
using System.Collections.Generic;
using System.Text;

namespace DataLayer.Data
{
    public class PaymentLog
    {
         public Guid Id { get; set; }

        public string CustomerAccount { get; set; }

        public string Amount { get; set; }

        public int StatusId { get; set; }

         public string PaymentReference { get; set; }        

        public DateTime DateCreated { get; set; }

        public string AssessmentId { get; set; }
       
        public int TransactionStatusId { get; set; }

        public string Comment { get; set; }
        public string InitiatedByBranchCode { get; set; }

        public virtual LOV Status { get; set; }
        
        public virtual AssessmentType PaymentType {get; set;}       


    }
}
