using DataLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CustomDutyJob
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    //[System.SerializableAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]

	[XmlRoot(ElementName = "eAssessmentNotice")]
	public class eAssessmentNotice
    {

       // public int Id { get; set; }
        
		[XmlElement(ElementName = "SADYear")]
		public string Year { get; set; }
		[XmlElement(ElementName = "CustomsCode")]
		public string CustomsCode { get; set; }
		[XmlElement(ElementName = "DeclarantCode")]
		public string DeclarantCode { get; set; }
		[XmlElement(ElementName = "DeclarantName")]
		public string DeclarantName { get; set; }
		[XmlElement(ElementName = "SADAssessmentSerial")]
		public string AssessmentSerial { get; set; }
		[XmlElement(ElementName = "SADAssessmentNumber")]
		public string AssessmentNumber { get; set; }
		[XmlElement(ElementName = "SADAssessmentDate")]
		public string AssessmentDate { get; set; }
		[XmlElement(ElementName = "CompanyCode")]
		public string CompanyCode { get; set; }
		[XmlElement(ElementName = "CompanyName")]
		public string CompanyName { get; set; }
		[XmlElement(ElementName = "BankCode")]
		public string BankCode { get; set; }
		[XmlElement(ElementName = "BankBranchCode")]
		public string BankBranchCode { get; set; }
		[XmlElement(ElementName = "FormMNumber")]
		public string FormMNumber { get; set; }
		[XmlElement(ElementName = "Taxes")]
		public Tax Taxes { get; set; }
		[XmlElement(ElementName = "TotalAmountToBePaid")]
		public string TotalAmountToBePaid { get; set; }
		
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }
	}

    }

