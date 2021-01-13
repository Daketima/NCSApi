using NCSApi.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class PaymentResponse
    {

        public int StatusId { get; set; }
        public int TransactionStatusId { get; set; }

        public Guid Id { get; set; }

        public string CustomerAccount { get; set; }

        public string Amount { get; set; }

        public string Status => Enum.GetName(typeof(TransactionStatus), StatusId);
       

        public string PaymentReference { get; set; }

        public DateTime DateCreated { get; set; }

        public string AssessmentId { get; set; }

        public string TransactionStatus  => Enum.GetName(typeof(TransactionStatus), TransactionStatusId);

        public string Comment { get; set; }


    }
}
