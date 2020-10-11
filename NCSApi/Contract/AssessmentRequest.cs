using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class AssessmentRequest
    {
        [Display(Name ="SDG Assessment Serial")]
       
        public string SADAssessmentSerial { get; set; }
       
        [Display(Name = "SDG Assessment Number")]
        public string SADAssessmentNumber { get; set; }
        
        //[Display(Name = "SDG Assessment Date")]
        //public string SADAssessmentDate { get; set; }
        
        [Display(Name = "SDG Assessment Year")]
        public string SADAssessmentYear { get; set; }
        
        [Display(Name = "Area Command")]
        public string AreaCommand { get; set; }
    }
}
