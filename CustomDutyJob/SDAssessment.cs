using DataLayer.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CustomDutyJob
{
	[XmlRoot(ElementName = "sdAssessmentNotice")]
	public class SDAssessment
    {
		[XmlElement(ElementName = "SDYear")]
		public string Year { get; set; }
		
		[XmlElement(ElementName = "CustomsCode")]
		public string CustomsCode { get; set; }
		
		[XmlElement(ElementName = "DeclarantCode")]
		public string DeclarantCode { get; set; }
		
		[XmlElement(ElementName = "DeclarantName")]
		public string DeclarantName { get; set; }
		
		[XmlElement(ElementName = "SDAssessmentSerial")]
		public string AssessmentSerial { get; set; }
		
		[XmlElement(ElementName = "SDAssessmentNumber")]
		public string AssessmentNumber { get; set; }
		
		[XmlElement(ElementName = "SDAssessmentDate")]
		public string AssessmentDate { get; set; }
		
		[XmlElement(ElementName = "PassportNumber")]
		public string PassportNumber { get; set; }	
		
		[XmlElement(ElementName = "BankCode")]
		public string BankCode { get; set; }
		[XmlElement(ElementName = "BankBranchCode")]
		public string BankBranchCode { get; set; }

		[XmlElement(ElementName = "Taxes")]
		public Tax Taxes { get; set; }

		[XmlElement(ElementName = "TotalAmountToBePaid")]
		public string TotalAmountToBePaid { get; set; }

		[XmlAttribute(AttributeName = "version")]
		public string version { get; set; }


	}
}
