using System;

namespace DataLayer.Data
{
    public class Tax
    {
        public Guid Id {get; set;}

        //[XmlElement(ElementName = "TaxCode")]
        public string TaxCode { get; set; }
        //[XmlElement(ElementName = "TaxAmount")]
        public string TaxAmount { get; set; }

         public Guid AssessmentId { get; set; }

         public virtual Assessment Assessment {get; set;}

    }
}