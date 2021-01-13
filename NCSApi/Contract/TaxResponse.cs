using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class TaxResponse
    {
        public Guid Id { get; set; }

        //[XmlElement(ElementName = "TaxCode")]
        public string TaxCode { get; set; }
        //[XmlElement(ElementName = "TaxAmount")]
        public string TaxAmount { get; set; }

        public Guid AssessmentId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
