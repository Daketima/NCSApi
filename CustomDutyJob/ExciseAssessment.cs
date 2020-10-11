using DataLayer.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CustomDutyJob
{
	[XmlRoot(ElementName = "eExciseAssessmentNotice")]
	public class ExciseAssessment
    {
		[XmlElement(ElementName = "Year")]
		public string Year { get; set; }
		
		[XmlElement(ElementName = "CustomsCode")]
		public string CustomsCode { get; set; }	
		
		[XmlElement(ElementName = "AssessmentSerial")]
		public string AssessmentSerial { get; set; }
		
		[XmlElement(ElementName = "AssessmentNumber")]
		public string AssessmentNumber { get; set; }
		
		[XmlElement(ElementName = "AssessmentDate")]
		public string AssessmentDate { get; set; }
		
		[XmlElement(ElementName = "BankCode")]
		public string BankCode { get; set; }
		
		[XmlElement(ElementName = "BankBranchCode")]
		public string BankBranchCode { get; set; }

		[XmlElement(ElementName = "CompanyCode")]
		public string CompanyCode { get; set; }

		[XmlElement(ElementName = "Taxes")]		
		public Tax Taxes { get; set; }

		[XmlElement(ElementName = "TotalAmountToBePaid")]
		public string TotalAmountToBePaid { get; set; }
	}
}
