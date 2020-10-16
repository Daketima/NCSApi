using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Data
{
    public class Assessment
    {
		[Key]
        public Guid Id {get; set;}  
        
        //[XmlElement(ElementName = "SADYear")]
		public string Year { get; set; }
		//[XmlElement(ElementName = "CustomsCode")]
		public string CustomsCode { get; set; }
		//[XmlElement(ElementName = "DeclarantCode")]
		public string DeclarantCode { get; set; }
		//[XmlElement(ElementName = "DeclarantName")]
		public string DeclarantName { get; set; }
		//[XmlElement(ElementName = "SADAssessmentSerial")]
		public string AssessmentSerial { get; set; }
		//[XmlElement(ElementName = "SADAssessmentNumber")]
		public string AssessmentNumber { get; set; }
		//[XmlElement(ElementName = "SADAssessmentDate")]
		public string AssessmentDate { get; set; }
		public string PassportNumber { get; set; }
		//[XmlElement(ElementName = "CompanyCode")]
		public string CompanyCode { get; set; }
		//[XmlElement(ElementName = "CompanyName")]
		public string CompanyName { get; set; }
		//[XmlElement(ElementName = "BankCode")]
		public string BankCode { get; set; }
		//[XmlElement(ElementName = "BankBranchCode")]
		public string BankBranchCode { get; set; }
		//[XmlElement(ElementName = "FormMNumber")]
		public string FormMNumber { get; set; }

		public DateTime DateCreated {get; set;}
		
		//[XmlElement(ElementName = "Taxes")]
		//public Tax Taxes { get; set; }
		
		//[XmlElement(ElementName = "TotalAmountToBePaid")]
		public string TotalAmountToBePaid { get; set; }
		//[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }

		public int  AssessmentTypeId {get; set;}

		public virtual AssessmentType AssessmentType {get; set;}
	}
		
		   
}