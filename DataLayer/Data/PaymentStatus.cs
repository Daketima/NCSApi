using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Data
{
    public class PaymentStatus
    {
        [Key]
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

        public Guid PaymentLogId {get; set;}

        public virtual PaymentLog PaymentLog{get; set;}        

    }
}