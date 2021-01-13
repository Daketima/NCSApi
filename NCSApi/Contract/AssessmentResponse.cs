using DataLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class AssessmentResponse
    {
		public Guid Id { get; set; }		
		public string Year { get; set; }	
		public string CustomsCode { get; set; }		
		public string DeclarantCode { get; set; }		
		public string DeclarantName { get; set; }
		public string AssessmentSerial { get; set; }	
		public string AssessmentNumber { get; set; }		
		public string AssessmentDate { get; set; }	
		public string CompanyCode { get; set; }	
		public string CompanyName { get; set; }	
		public string BankCode { get; set; }		
		public string BankBranchCode { get; set; }		
		public string FormMNumber { get; set; }
		public string AssessmentType { get; set; }

		public string AttachmentUrl { get; set; }

		public List<TaxResponse> Taxes { get; set; }		
		public string TotalAmountToBePaid { get; set; }	
		public string Version { get; set; }
	}
}
