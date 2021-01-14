using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AutoMapper;
using DataLayer;
using DataLayer.Data;
using DataLayer.DataContext;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NCSApi.Config;
using NCSApi.Contract;
using NCSApi.Core;
using NCSApi.Implementation;
using NCSApi.Service;
using Newtonsoft.Json;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NCSApi.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        readonly ICustomDutyClient _client;
        readonly DutyConfig _dutyConfig;
        readonly CustomContext _context;
        Random _random = new Random();
        readonly IMapper _mapper;
        readonly OpService _opService;
        private readonly string appName;

        public PaymentController(ICustomDutyClient client, DutyConfig dutyConfig, CustomContext context, IMapper mapper,IConfiguration config)
        {

            _client = client;
            _dutyConfig = dutyConfig;
            _context = context;
            _mapper = mapper;
            _opService = new OpService(_dutyConfig, _client);
            appName = config.GetValue<string>("ApplicationName");

        }

        // GET: api/<PaymentController>
        //initiator leg
        //[HttpGet()]
        //public async Task<IActionResult> Get( string PaymentReference)

        [HttpPost()]
        //[Produces("application/json")]
        [Route("process")]
        public async Task<IActionResult> Get(PaymentCheckDto model)
        {           
          
            try
            {
                var loggedinUser = await AuthService.GetLoggedinUserDetails(model.LoggedInUser);
                var loggedInUsername = loggedinUser.Username;
                var loggedInbranchCode = loggedinUser.BranchCode;

                if (!string.IsNullOrEmpty(loggedInUsername)
                    && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower() && !a.isSupervisor))
                {
                    var getPaymentLog = await _context.Payment.Where(x => x.PaymentReference.Equals(model.PaymentReference) && x.InitiatedByBranchCode == loggedInbranchCode).FirstOrDefaultAsync();

                    if (getPaymentLog == null) return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "Wrong Payment Reference | payment was not initiated in your branch" });

                    Assessment getNotificaton = await _context.Assessment.FindAsync(Guid.Parse(getPaymentLog.AssessmentId));
                    if (getNotificaton == null) return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "Assessment Notification not found" });

                    var msg = new { Account = getPaymentLog.CustomerAccount };
                    string formatBaseUrl = _dutyConfig.BaseUrl;

                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.NameEnquiry}"),
                        Method = HttpMethod.Post,
                        Content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json")
                    };
                    Log.Information($"Attempting Customer Name Enquiry for the account: {getPaymentLog.CustomerAccount}");
                    var response = await _client.SendKaoshiRequest(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string assessmentType = Enum.GetName(typeof(AssessmentTypes), getNotificaton.AssessmentTypeId);

                        string content = await response.Content.ReadAsStringAsync();
                        bool tillIsCredited = await CreditTillAccount(getPaymentLog.Amount, getPaymentLog.CustomerAccount, model.PaymentReference);

                        if (tillIsCredited)
                        {
                            string xmlBuilder = PaymentTypeFinder(assessmentType, getPaymentLog, getNotificaton);
                            string xmlSavePath = assessmentType == "Excise" ? _dutyConfig.ExcisePaymentPath : @"C:\tosser\inout\in\";
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.LoadXml(xmlBuilder);
                            xDoc.Save(Path.Combine(xmlSavePath, $"{assessmentType}_{DateTime.Now.Ticks}.xml"));

                            PaymentResponseFinder(getPaymentLog, out bool _responseReceived, out string Message, assessmentType, out string POCIsCredit, out string VATIsCredit);
                            if (_responseReceived)
                            {
                                return Ok(new
                                {
                                    Status = HttpStatusCode.OK,
                                    Message = "Request Successful",
                                    data = new { NCSResponse = Message, POSAccountStatus = POCIsCredit, VATAccountStatus = VATIsCredit }
                                });
                            }
                            else
                            {
                                ProcessNCSError(getPaymentLog.Id, out bool errConfirm, out string ErrorMessage);
                                if (errConfirm)
                                {
                                    cleaner.DeleteFile(@"C:\tosser\inout\err");
                                    return Ok(new { Status = HttpStatusCode.OK, Message = ErrorMessage });
                                }
                            }

                            getPaymentLog.StatusId = (int)TransactionStatus.Completed;
                            getPaymentLog.TransactionStatusId = (int)TransactionStatus.Pending;
                            _context.Update(getPaymentLog);
                            await _context.SaveChangesAsync();

                            return Ok(new { Status = HttpStatusCode.OK, Message = "Response yet to come from NCS" });
                        }
                    }
                    Log.Information($"Name enquiry response: {response.StatusCode}. client user name: {_dutyConfig.ClientUsername}, client password: {_dutyConfig.ClientPassword}");
                    return BadRequest(new { Status = HttpStatusCode.OK, Message = "Name enquired failed. No response from server" });
                }
                else
                {
                    return BadRequest("staff with supervisor role on this application, are not allowed to access this action");
                }
              
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ex.Message}");
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", Data = ex });
            }
        }

        // initiator leg
        [HttpPost()]
        //[Produces("application/json")]
        [Route("initiate")]
        public async Task<IActionResult> LogPayment([FromForm] DutyPaymentRequest model)
        {
           // newPaymentLog = null;
            //if (!ModelState.IsValid)
            //    return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "One or more validation failed" });
            try
            {

                //PaymentLog existingPaymentLog = await _context.Payment.FirstOrDefaultAsync(x => x.AssessmentId.Equals(model.AssessmentId));               

                //todo: implement check for maker
                var loggedinUser = await AuthService.GetLoggedinUserDetails(model.LoggedInUser);
                var loggedInUsername = loggedinUser.Username;
                var loggedInbranchCode = loggedinUser.BranchCode;

                if (!string.IsNullOrEmpty(loggedInUsername)
                    && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower() && !a.isSupervisor))
                {
                    var newPaymentLog = new PaymentLog
                    {
                        CustomerAccount = model.CustomerAccount,
                        Amount = model.DutyTotalAmount,
                        StatusId = (int)TransactionStatus.Initiated,
                        PaymentReference = RandomPassword(),
                        AssessmentId = model.AssessmentId,
                        DateCreated = DateTime.Now,
                        TransactionStatusId = (int)TransactionStatus.Pending,
                        InitiatedByBranchCode = loggedInbranchCode

                    };

                    string ImageFullPath = string.Empty;

                    if (Request.Form.Files.Any())
                    {
                        var file = Request.Form.Files[0];
                        var folderName = Path.Combine("Assessment_Attachment");
                        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                        if (file.Length > 0)
                        {
                            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            fileName = $"{DateTime.Now.Ticks}{fileName}";
                            var fullPath = Path.Combine(pathToSave, fileName);
                            var dbPath = Path.Combine(folderName, fileName);

                            ImageFullPath = $"{_dutyConfig.CustomDutyApiUrl}{fileName}";

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                        }
                    }

                    _context.Payment.Add(newPaymentLog);
                    int added = await _context.SaveChangesAsync();

                    if (added == 1)
                    {

                        Assessment assessmentToUpdate = await _context.Assessment.FindAsync(Guid.Parse(model.AssessmentId));
                        assessmentToUpdate.AttachmentPath = ImageFullPath;
                        assessmentToUpdate.InitiatedBy = loggedInUsername;
                        assessmentToUpdate.InitiatedByBranchCode = loggedInbranchCode;

                        _opService.GetStaffDetail(loggedInbranchCode, "customs", out string SupervisorMail, out string SupevisorName);
                        _opService.SendMailToSupervisor(SupervisorMail, SupevisorName, assessmentToUpdate.Id.ToString(), out string Message);

                        _context.Update(assessmentToUpdate);
                        await _context.SaveChangesAsync();

                        return Ok(new { HttpStatusCode.OK, Message = "Request completed", Data = new { PaymentReference = newPaymentLog.PaymentReference, PaymentId = newPaymentLog.Id } });
                    }
                    return BadRequest(new { HttpStatusCode.BadRequest, Message = "Request Unsuccessful" });
                }
                else
                {
                    return BadRequest("users with supervisor role are not allowed to perform this operation. ");
                }
            }

            catch (Exception ex)
            {
                return BadRequest(new { HttpStatusCode.InternalServerError, Message = "An error occured", Data = ex });
            }
        }


        //approval leg
        [HttpPost]        
        [Route("accept")]
        public async Task<IActionResult> thisAss([FromBody] DeclineRequest model)
        {
            try
            {
                PaymentLog getAss = await _context.Payment.Where(x => x.PaymentReference == model.PaymentRefernce).FirstOrDefaultAsync();
              
                var loggedinUser = await AuthService.GetLoggedinUserDetails(model.LoggedInUser);
                var loggedInUsername = loggedinUser.Username;
                var loggedInbranchCode = loggedinUser.BranchCode;

                if (!string.IsNullOrEmpty(loggedInUsername)
                   && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower() && a.isSupervisor) && getAss.InitiatedByBranchCode == loggedInbranchCode)
                { 
                    if (getAss != null)
                    {
                        // Payment getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == assessmentId.ToString()).FirstOrDefaultAsync();
                        getAss.StatusId = (int)TransactionStatus.Accepted;
                        _context.Update(getAss);
                        await _context.SaveChangesAsync();

                        return Ok(new { status = HttpStatusCode.OK, Message = "Payment accepted", });
                    }
                    return NotFound(new { status = HttpStatusCode.NotFound, Message = "Payment not found", data = "Resource not found" });

                }
                else
                {
                    return BadRequest("only staff with supervisor role can perform this action | this transaction was not initiated in your branch");
                }


            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }

        //approval leg
        [HttpPost]
        [Route("decline")]
        public async Task<IActionResult> thisAsses([FromBody] DeclineRequest model)
        {
            try
            {

                PaymentLog getAss = await _context.Payment.Where(x => x.PaymentReference == model.PaymentRefernce).FirstOrDefaultAsync();

                var loggedinUser = await AuthService.GetLoggedinUserDetails(model.LoggedInUser);
                var loggedInUsername = loggedinUser.Username;
                var loggedInbranchCode = loggedinUser.BranchCode;

                if (!string.IsNullOrEmpty(loggedInUsername)
                   && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower() && a.isSupervisor) && getAss.InitiatedByBranchCode == loggedInbranchCode)
                {
                    if (getAss != null)
                    {
                        // Payment getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == assessmentId.ToString()).FirstOrDefaultAsync();
                        getAss.StatusId = (int)TransactionStatus.Declined;
                        getAss.Comment = model.Comment;

                        _context.Update(getAss);
                        await _context.SaveChangesAsync();

                        return Ok(new { status = HttpStatusCode.OK, Message = "Payment declined", });
                    }
                    return NotFound(new { status = HttpStatusCode.NotFound, Message = "Payment not found", data = "Resource not found" });
                }
                else
                {
                    return BadRequest("only supervisor role can perform this action | this transaction was not initiated in your branch");
                }
               
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }

        //[HttpGet]
        //[Route("Reports")]
        
        [HttpPost]
        [Route("Reports")]
        public async Task<IActionResult> GetPayment(ReportDto model )
        {
            try
            {
                var loggedinUser = await AuthService.GetLoggedinUserDetails(model.LoggedInUser);
                var loggedInUsername = loggedinUser.Username;
                var loggedInbranchCode = loggedinUser.BranchCode;

                if (!string.IsNullOrEmpty(loggedInUsername)
                   && loggedinUser.Applications.Any(a => a.application.name.ToLower() == appName.ToLower() && a.isSupervisor))
                {
                    List<PaymentStatus> paymentStatuses = await _context.PaymentStatus
                    .Include(x => x.PaymentLog).ToListAsync();

                    List<ReportResponse> report = _mapper.Map<List<ReportResponse>>(paymentStatuses);
                    if (paymentStatuses.Any())
                    {
                        paymentStatuses.ForEach(x => x.DateCreated.ToShortDateString());
                        //// Payment getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == assessmentId.ToString()).FirstOrDefaultAsync();
                        //paymentStatuses.StatusId = (int)TransactionStatus.Accepted;
                        //_context.Update(paymentStatuses);
                        //await _context.SaveChangesAsync();

                        return Ok(new { status = HttpStatusCode.OK, Message = "Request Successful", Data = report });
                    }
                    return NotFound(new { status = HttpStatusCode.NotFound, Message = "No payment status found" });
                }
                else
                {
                    return BadRequest("report can only be accessed by supervisor");
                }


                
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }

        [HttpGet]
        [Route("Payment/log/{Id}")]
        public async Task<IActionResult> ThisPaymentLog(Guid Id)
        {
            try
            {
                PaymentLog getAss = await _context.Payment.FindAsync(Id);
                if (getAss != null)
                {
                    getAss.StatusId = (int)TransactionStatus.Declined;
                    _context.Update(getAss);
                    await _context.SaveChangesAsync();

                    return Ok(new { status = HttpStatusCode.OK, Message = "Payment declined" });
                }
                return NotFound(new { status = HttpStatusCode.NotFound, Message = "Payment not found", data = "Resource not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }

        private async Task<bool> CreditTillAccount(string Amount, string SourceAccount, string RequestId)
        {
            bool credited = false;

            var account = new Accounts { CreditAccount = _dutyConfig.TillAccount, DebitAccount = SourceAccount, Narration = "", TranAmount = Convert.ToDecimal(Amount) };
            var body = new IntraTransferRequest { RequestID = RequestId, TranCurrency = "NGN", Accounts = account };


            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.IntraTransfer}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var response = await _client.SendKaoshiRequest(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                credited = true;

                return credited;
            }

            return credited;
        }

        private void CreditPOCAccount(string Amount, string VATAmount, string RequestId, out string POCIsCredited, out string VATIsCredited)
        {            
            POCIsCredited = "POC not Credited";
            VATIsCredited = "VAT not credited";

            var account = new Accounts { CreditAccount = _dutyConfig.POCAccount, DebitAccount = _dutyConfig.TillAccount, Narration = $"Duty payment for {RequestId}", TranAmount = Convert.ToDecimal(Amount) };
            var body = new IntraTransferRequest { RequestID = RequestId, TranCurrency = "NGN", Accounts = account };

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.IntraTransfer}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var response =  _client.SendKaoshiRequest(request).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                POCIsCredited = "POC was successfully credited";
                CreditVATAccount(VATAmount, RequestId, out string VatCredited );
                VATIsCredited = VatCredited;                
            }
            
        }


        private void CreditVATAccount(string Amount, string RequestId, out string VatCredited)
        {          
            VatCredited = "VAT not Credited";

            var account = new Accounts { CreditAccount = _dutyConfig.VATAccount, DebitAccount = _dutyConfig.TillAccount, Narration = $"VAT payment for {RequestId}", TranAmount = Convert.ToDecimal(Amount) };
            var body = new IntraTransferRequest { RequestID = RequestId, TranCurrency = "NGN", Accounts = account };


            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.IntraTransfer}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var response = _client.SendKaoshiRequest(request).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                VatCredited = "VAT is credited";
            }
           
        }

        string PaymentTypeFinder(string AssessmentType, PaymentLog paymentLog, Assessment Assessment)
        {
            string xmlBuilder = xmlBuilder = System.IO.File.ReadAllText($"{Directory.GetCurrentDirectory()}\\PaymentFiles\\Payment\\{AssessmentType}.xml");
            if (AssessmentType.Equals("Excise"))
            {

                xmlBuilder = xmlBuilder.Replace("{Payment Date}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
                          .Replace("{Customs Code}", Assessment.CustomsCode ?? "Not Available")
                          .Replace("{Company Code}", Assessment.CompanyCode ?? "Not Available")
                          .Replace("{Bank Code}", Assessment.BankCode)
                          .Replace("{Serial}", Assessment.AssessmentSerial)
                          .Replace("{Number}", Assessment.AssessmentNumber)
                          .Replace("{Year}", Assessment.Year)
                          //.Replace("{Means of Payment}", "In-Branch")
                          .Replace("{Reference}", paymentLog.PaymentReference)
                          .Replace("{Amount}", paymentLog.Amount)
                          .Replace("{Total Amount}", Assessment.TotalAmountToBePaid);
            }

            if (AssessmentType.Equals("SD"))
            {

                xmlBuilder = xmlBuilder.Replace("{Document}", AssessmentType)
                          .Replace("{Payment Date}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
                          .Replace("{Customs Code}", Assessment.CustomsCode ?? "Not Available")
                          .Replace("{Declarant Code}", Assessment.DeclarantCode ?? "Not Available")
                          .Replace("{Bank Code}", Assessment.BankCode)
                          .Replace("{Serial}", Assessment.AssessmentSerial)
                          .Replace("{Number}", Assessment.AssessmentNumber)
                          .Replace("{Year}", Assessment.Year)
                          // .Replace("{Means Of Payment}", "In-Branch")
                          .Replace("{Reference}", paymentLog.PaymentReference)
                          .Replace("{Amount}", paymentLog.Amount)
                          .Replace("{Total Amount}", Assessment.TotalAmountToBePaid);

            }

            if (AssessmentType.Equals("SGD"))
            {
                xmlBuilder = xmlBuilder.Replace("{Payment Date}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"))
                          .Replace("{Customs Code}", Assessment.CustomsCode)
                          .Replace("{Declarant Code}", Assessment.DeclarantCode)
                          .Replace("{Bank Code}", Assessment.BankCode)
                          .Replace("{Serial}", Assessment.AssessmentSerial)
                          .Replace("{Number}", Assessment.AssessmentNumber)
                          .Replace("{Year}", Assessment.Year)
                          //.Replace("{Means Of Payment}", "In-Branch")
                          .Replace("{Reference}", paymentLog.PaymentReference)
                          .Replace("{Amount}", paymentLog.Amount)
                          .Replace("{Total Amount}", Assessment.TotalAmountToBePaid);
            }

            return xmlBuilder;

        }

        void PaymentResponseFinder(PaymentLog PaymentLog, out bool ResponseReceived, out string Message, string assessmentType, out string POCCredited, out string VATCredited)
        {
            POCCredited = string.Empty;
            VATCredited = string.Empty;
            int retry = 100;
            PaymentStatus paymentStatus = null;
            ResponseReceived = false;
            Message = string.Empty;
            int counter = -100;
            string responsePath = assessmentType == "Excise" ? _dutyConfig.ExciseResponsePath : @"C:\tosser\inout\eresponse";

            Thread.Sleep(10000);
            do
            {
                var all = Directory.GetFiles(responsePath);
                if (all.Any())
                {
                    foreach (string filename in all)
                    {
                        string readFile = System.IO.File.ReadAllText(filename);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(readFile);

                        var transactionStatusDetails = xmlDoc.SelectNodes("TransactionResponse");

                        using (CustomContext cust = new CustomContext())
                        {
                            foreach (XmlNode node in transactionStatusDetails)
                            {
                                if (readFile.Contains("PayExcise"))
                                {
                                    paymentStatus = new PaymentStatus
                                    {
                                        CustomsCode = node["CustomsCode"].InnerText,
                                        CompanyCode = node["CompanyCode"].InnerText,
                                        AssessmentSerial = node["Asmt"]["AssessmentSerial"].InnerText,
                                        AssessmentNumber = node["Asmt"]["AssessmentNumber"].InnerText,
                                        Year = node["Asmt"]["SADYear"].InnerText,
                                        Status = node["TransactionStatus"].InnerText,
                                        Message = node["Info"]["Message"].InnerText,
                                        DateCreated = DateTime.Now,
                                        PaymentLogId = PaymentLog.Id
                                    };
                                }
                                else
                                {
                                    paymentStatus = new PaymentStatus
                                    {
                                        CustomsCode = node["CustomsCode"].InnerText,
                                        DeclarantCode = node["DeclarantCode"].InnerText,
                                        AssessmentSerial = node["SadAsmt"]["SADAssessmentSerial"].InnerText,
                                        AssessmentNumber = node["SadAsmt"]["SADAssessmentNumber"].InnerText,
                                        Year = node["SadAsmt"]["SADYear"].InnerText,
                                        Status = node["TransactionStatus"].InnerText,
                                        Message = node["Info"]["Message"].InnerText,
                                        DateCreated = DateTime.Now,
                                        PaymentLogId = PaymentLog.Id
                                    };
                                }


                                cust.PaymentStatus.Add(paymentStatus);
                                int saveOne = cust.SaveChanges();

                                if (saveOne == 1)
                                {
                                    Tax vatOnAssessment = null;
                                    vatOnAssessment =  _context.Tax.Where(x => x.AssessmentId.Equals(Guid.Parse(PaymentLog.AssessmentId)) && x.TaxCode.Equals("VAT")).FirstOrDefault();

                                    CreditPOCAccount(PaymentLog.Amount, vatOnAssessment.TaxAmount, PaymentLog.PaymentReference, out POCCredited, out VATCredited);
                                    // retry = 0;
                                    counter = 0;
                                    ResponseReceived = (counter == 0);
                                    Message = paymentStatus.Message;

                                    cleaner.DeleteFile(assessmentType == "Excise" ? _dutyConfig.ExciseResponsePath : @"C:\tosser\inout\eresponse");

                                    Log.Information("Updation transaction status");
                                    PaymentLog.StatusId = (int)TransactionStatus.Completed;
                                    PaymentLog.TransactionStatusId = (int)TransactionStatus.Completed;
                                    _context.Update(PaymentLog);
                                    _context.SaveChanges();
                                    //return Ok(new { Status = HttpStatusCode.OK, Message = $"Transaction completed: NCS Message - {paymentStatus?.Message}" });
                                }
                            }
                        }
                    }                    
                }
                counter = (counter == 0 ? counter : counter += 1); retry = counter;

            }
            while (retry != 0);

        }


        #region response and requet classed


        private class IntraTransferRequest
        {
            public string RequestID { get; set; }
            public string TranCurrency { get; set; }
            public Accounts Accounts { get; set; }
        }

        private class Accounts
        {
            public string DebitAccount { get; set; }
            public string CreditAccount { get; set; }
            public decimal TranAmount { get; set; }
            public string Narration { get; set; }
        }


        public class BalanceEnquiryResponse
        {
            public string ResponseMessage { get; set; }
            public string ResponseDescription { get; set; }
            public ResponseDetail response { get; set; }
        }

        public class ResponseDetail
        {
            public float AvailableBalance { get; set; }
            public float ReservedAmount { get; set; }
            public string AccountCurrency { get; set; }
        }


        #endregion

        private int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        private string RandomString(int size, bool lowerCase = false)
        {
            //Random _random = new Random();
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):   
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        private string RandomPassword()
        {
            var passwordBuilder = new StringBuilder();

            // 4-Letters lower case   
            passwordBuilder.Append(RandomString(4, false));

            // 4-Digits between 1000 and 9999  
            passwordBuilder.Append(RandomNumber(1000, 9999));

            // 2-Letters upper case  
            passwordBuilder.Append(RandomString(2));
            return passwordBuilder.ToString();
        }

        private void ProcessNCSError(Guid paymentId, out bool ErrorIsReceived, out string Message)
        {
            ErrorIsReceived = false;
            var getPaymentLog = _context.Payment.Find(paymentId);
            int retry = 100;
            int counter = -100;
            PaymentStatus paymentStatus = null;
            Message = string.Empty;


            Thread.Sleep(10000);
            do
            {
                //var all = Directory.GetFiles(@"C:\tosser\inout\in");
                var all = Directory.GetFiles(@"C:\tosser\inout\err");

                if (all.Any())
                {
                    foreach (string filename in all)
                    {
                        string readFile = System.IO.File.ReadAllText(filename);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(readFile);

                        var transactionStatusDetails = xmlDoc.SelectNodes("TransactionResponse");

                        using (CustomContext cust = new CustomContext())
                        {
                            foreach (XmlNode node in transactionStatusDetails)
                            {
                                if (readFile.Contains("PayExcise"))
                                {
                                    paymentStatus = new PaymentStatus
                                    {
                                        CustomsCode = node["CustomsCode"].InnerText,
                                        CompanyCode = node["CompanyCode"].InnerText,
                                        AssessmentSerial = node["Asmt"]["AssessmentSerial"].InnerText,
                                        AssessmentNumber = node["Asmt"]["AssessmentNumber"].InnerText,
                                        Year = node["Asmt"]["SADYear"].InnerText,
                                        Status = node["TransactionStatus"].InnerText,
                                        Message = node["Info"]["Message"].InnerText,
                                        DateCreated = DateTime.Now,
                                        PaymentLogId = getPaymentLog.Id,
                                        ErrorCode = node["Info"]["Message"].Attributes["errorCode"].Value

                                    };
                                }
                                else
                                {
                                    paymentStatus = new PaymentStatus
                                    {
                                        CustomsCode = node["CustomsCode"].InnerText,
                                        DeclarantCode = node["DeclarantCode"].InnerText,
                                        AssessmentSerial = node["SadAsmt"]["SADAssessmentSerial"].InnerText,
                                        AssessmentNumber = node["SadAsmt"]["SADAssessmentNumber"].InnerText,
                                        Year = node["SadAsmt"]["SADYear"].InnerText,
                                        Status = node["TransactionStatus"].InnerText,
                                        Message = node["Info"]["Message"].InnerText,
                                        DateCreated = DateTime.Now,
                                        PaymentLogId = getPaymentLog.Id,
                                        ErrorCode = node["Info"]["Message"].Attributes["errorCode"].Value
                                    };
                                }

                                var getPaymentStatus = _context.PaymentStatus.Where(x => x.PaymentLogId.Equals(getPaymentLog.Id)).FirstOrDefault();

                                if (getPaymentStatus != null)
                                {
                                    getPaymentStatus.ErrorCode = paymentStatus.ErrorCode;
                                    getPaymentStatus.Message = paymentStatus.Message;

                                    counter = 0;
                                    ErrorIsReceived = counter == 0;

                                    //cleaner.DeleteFile(@"C:\tosser\inout\err");

                                    Message = paymentStatus.Message;
                                    Log.Information("Error received");
                                    getPaymentLog.StatusId = (int)TransactionStatus.Completed;
                                    getPaymentLog.TransactionStatusId = (int)TransactionStatus.Pending;
                                    _context.Update(getPaymentStatus);
                                    _context.SaveChangesAsync();
                                    break;
                                }
                                else
                                {
                                    cust.PaymentStatus.Add(paymentStatus);
                                    int saveOne = cust.SaveChanges();

                                    if (saveOne == 1)
                                    {
                                        //  await CreditTillAccount(getPaymentLog.Amount, _dutyConfig.SuspenseAccount, getPaymentLog.PaymentReference);
                                        counter = 0;
                                        ErrorIsReceived = counter == 0;

                                        // cleaner.DeleteFile(@"C:\tosser\inout\err");

                                        Message = paymentStatus.Message;
                                        Log.Information("Error received");
                                        getPaymentLog.StatusId = (int)TransactionStatus.Completed;
                                        getPaymentLog.TransactionStatusId = (int)TransactionStatus.Pending;
                                        _context.Update(getPaymentLog);
                                        _context.SaveChangesAsync();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                counter = (counter == 0 ? counter : counter += 1); retry = counter;
            }
            while (retry != 0);

        }

        class cleaner
        {
            public static void DeleteFile(string folderPath)
            {
                DirectoryInfo all = new DirectoryInfo(folderPath);


                foreach (FileInfo file in all.GetFiles())
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    file.IsReadOnly = false;
                    file.Delete();
                }

                //    all.Delete(true);
                //    string pathString2 = @"C:\tosser\inout\callback";
                //System.IO.Directory.CreateDirectory(pathString2) ;

            }
        }

        //void BalanceEquiry(string AccountNumber, string debitmount, out bool FundIsSufficient)
        //{
        //    FundIsSufficient = false;

        //    var msg = new { Account = AccountNumber };

        //    HttpRequestMessage request = new HttpRequestMessage
        //    {
        //        RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.NameEnquiry}"),
        //        Method = HttpMethod.Post,
        //        Content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json")
        //    };

        //    Log.Information($"Starting balance enquiry for the account: {AccountNumber}");
        //    var response = Task.Run(() => _client.SendKaoshiRequest(request)).Result;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string stringresponseContent = response.Content.ReadAsStringAsync().Result;
        //        BalanceEnquiryResponse customerBalance = JsonConvert.DeserializeObject<BalanceEnquiryResponse>(stringresponseContent);
        //    }
        //}
    }
    
}
