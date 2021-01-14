using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class DutyPaymentRequest
    {
        [Required]
        public string CustomerAccount { get; set; }

        [Required]
        public string DutyTotalAmount { get; set; }

        [Required]
        public string AssessmentId { get; set; } 
        
       
        public string LoggedInUser { get; set; }

       
    }
}
