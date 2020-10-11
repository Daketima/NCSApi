using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Data
{
    public class XMLArchive
    {
        [Key]
        public Guid Id { get; set; }

        //[XmlElement(ElementName = "TaxCode")]
        public string RawXML { get; set; }
        //[XmlElement(ElementName = "TaxAmount")]

        public Guid AssessmentId { get; set; }

        public DateTime DateCreated { get; set; }

        public string Path { get; set; }

        public virtual Assessment Assessment { get; set; }

    }
}