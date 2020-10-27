using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using DataLayer;
using DataLayer.Data;
using DataLayer.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NCSApi.Config;
using NCSApi.Contract;
using NCSApi.Core;
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



        public PaymentController(ICustomDutyClient client, DutyConfig dutyConfig, CustomContext context)
        {

            _client = client;
            _dutyConfig = dutyConfig;
            _context = context;
        }

        // GET: api/<PaymentController>
        [HttpGet]
        //[Produces("application/json")]
        [Route("process/{PaymentReference}")]
        public async Task<IActionResult> Get(string PaymentReference)
        {
            //if (string.IsNullOrEmpty(PaymentReference)) return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "Payment Reference is required" });
            try
            {
                var getPaymentLog = await _context.Payment.Where(x => x.PaymentReference.Equals(PaymentReference)).FirstOrDefaultAsync();
                if (getPaymentLog == null) return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "Wrong Payment Reference" });

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

                    // int retry = 5;
                    // bool responseRecieve = false;
                    string content = await response.Content.ReadAsStringAsync();
                    bool suspenseIsCredited = await CreditSuspense(getPaymentLog.Amount, getPaymentLog.CustomerAccount, PaymentReference);

                    if (suspenseIsCredited)
                    {
                        string xmlBuilder = PaymentTypeFinder(assessmentType, getPaymentLog, getNotificaton);
                        string xmlSavePath = assessmentType == "Excise" ? _dutyConfig.ExcisePaymentPath : @"C:\tosser\inout\in\";
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(xmlBuilder);
                        //xDoc.Save(Path.Combine(@"C:\tosser\inout\out\", $"{DateTime.Now.Ticks}.xml"));
                        xDoc.Save(Path.Combine(xmlSavePath, $"{assessmentType}_{DateTime.Now.Ticks}.xml"));

                        //bool _responseReceived = false;                      

                        PaymentResponseFinder(getPaymentLog, out bool _responseReceived, out string Message, assessmentType);
                        if (_responseReceived)
                        {
                            return Ok(new { Status = HttpStatusCode.OK, Message = $"Transaction completed: NCS Message - {Message}" });
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
            catch (Exception ex)
            {
                Log.Error(ex, $"{ex.Message}");
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", Data = ex });
            }
        }

        [HttpPost]
        //[Produces("application/json")]
        [Route("initiate")]
        public async Task<IActionResult> LogPayment([FromBody] DutyPaymentRequest model)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(new { status = HttpStatusCode.BadRequest, Message = "One or more validation failed" });
            try
            {
                var newPaymentLog = new PaymentLog
                {
                    CustomerAccount = model.CustomerAccount,
                    Amount = model.DutyTotalAmount,
                    StatusId = (int)TransactionStatus.Initiated,
                    PaymentReference = RandomPassword(),
                    AssessmentId = model.AssessmentId,
                    DateCreated = DateTime.Now,
                    TransactionStatusId = (int)TransactionStatus.Pending

                };

                _context.Payment.Add(newPaymentLog);
                int added = await _context.SaveChangesAsync();

                if (added == 1)
                {
                    return Ok(new { HttpStatusCode.OK, Message = "Request completed", Data = new { PaymentReference = newPaymentLog.PaymentReference, PaymentId = newPaymentLog.Id } });
                }

                return BadRequest(new { HttpStatusCode.BadRequest, Message = "Request Unsuccessful" });
            }

            catch (Exception ex)
            {
                return BadRequest(new { HttpStatusCode.InternalServerError, Message = "An error occured", Data = ex });
            }
        }


        [HttpPut]
        [Route("accept/{PaymentReference}")]
        public async Task<IActionResult> thisAss(string PaymentReference)
        {
            try
            {
                PaymentLog getAss = await _context.Payment.Where(x => x.PaymentReference == PaymentReference).FirstOrDefaultAsync();
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
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }
        }

        [HttpPut]
        [Route("decline")]
        public async Task<IActionResult> thisAsses([FromBody] DeclineRequest model)
        {
            try
            {

                PaymentLog getAss = await _context.Payment.Where(x => x.PaymentReference == model.PaymentRefernce).FirstOrDefaultAsync();
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
            catch (Exception ex)
            {
                return BadRequest(new { status = HttpStatusCode.InternalServerError, Message = "An error occured", data = ex });
            }

        }

        [HttpGet]
        [Route("Reports")]
        public async Task<IActionResult> GetPayment()
        {
            try
            {

                List<PaymentStatus> paymentStatuses = await _context.PaymentStatus.ToListAsync();
                if (paymentStatuses.Any())
                {
                    paymentStatuses.ForEach(x => x.DateCreated.ToShortDateString());
                    //// Payment getPaymentLog = await _context.Payment.Where(x => x.AssessmentId == assessmentId.ToString()).FirstOrDefaultAsync();
                    //paymentStatuses.StatusId = (int)TransactionStatus.Accepted;
                    //_context.Update(paymentStatuses);
                    //await _context.SaveChangesAsync();

                    return Ok(new { status = HttpStatusCode.OK, Message = "Request Successful", Data = paymentStatuses });
                }
                return NotFound(new { status = HttpStatusCode.NotFound, Message = "No payment status found" });
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

        private async Task<bool> CreditSuspense(string Amount, string SourceAccount, string RequestId)
        {
            bool credited = false;

            var account = new Accounts { CreditAccount = _dutyConfig.SuspenseAccount, DebitAccount = SourceAccount, Narration = "", TranAmount = Convert.ToDecimal(Amount) };
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

        private async Task<bool> CreditTillAccount(string Amount, string SourceAccount, string RequestId)
        {
            bool credited = false;

            var account = new Accounts { CreditAccount = _dutyConfig.TillAccount, DebitAccount = _dutyConfig.SuspenseAccount, Narration = $"Duty payment for {RequestId}", TranAmount = Convert.ToDecimal(Amount) };
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

        void PaymentResponseFinder(PaymentLog PaymentLog, out bool ResponseReceived, out string Message, string assessmentType)
        {
            int retry = 50;
            PaymentStatus paymentStatus = null;
            ResponseReceived = false;
            Message = string.Empty;
            int counter = -50;
            string responsePath = assessmentType == "Excise" ? _dutyConfig.ExciseResponsePath : @"C:\tosser\inout\eresponse";

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
                                    Task.Run(() => CreditTillAccount(PaymentLog.Amount, _dutyConfig.SuspenseAccount, PaymentLog.PaymentReference));
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
            int retry = 50;
            int counter = -50;
            PaymentStatus paymentStatus = null;
            Message = string.Empty;

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

                                cust.PaymentStatus.Add(paymentStatus);
                                int saveOne = cust.SaveChanges();

                                if (saveOne == 1)
                                {
                                    //  await CreditTillAccount(getPaymentLog.Amount, _dutyConfig.SuspenseAccount, getPaymentLog.PaymentReference);
                                    counter = 0;
                                    ErrorIsReceived = counter == 0;

                                    cleaner.DeleteFile(@"C:\tosser\inout\err");

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

        //    void BalanceEquiry(string AccountNumber, string debitmount, out bool FundIsSufficient)
        //    {
        //        FundIsSufficient = false;

        //        var msg = new { Account = AccountNumber };

        //        HttpRequestMessage request = new HttpRequestMessage
        //        {
        //            RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.NameEnquiry}"),
        //            Method = HttpMethod.Post,
        //            Content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json")
        //        };

        //        Log.Information($"Starting balance enquiry for the account: {AccountNumber}");
        //        var response = Task.Run(() =>  _client.SendKaoshiRequest(request)).Result;

        //         if (response.IsSuccessStatusCode)
        //            {

        //              string response = response.con



        //            }



        //}
    }
}
