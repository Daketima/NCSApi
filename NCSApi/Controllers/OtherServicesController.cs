using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NCSApi.Config;
using NCSApi.Contract;
using NCSApi.Core;
using Newtonsoft.Json;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NCSApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtherServicesController : ControllerBase
    {
        readonly DutyConfig _dutyConfig;
        readonly ICustomDutyClient _client;
        readonly IMapper _mapper;
        public OtherServicesController(ICustomDutyClient client, DutyConfig dutyConfig, IMapper mapper)
        {

            _client = client;
            _dutyConfig = dutyConfig;
            _mapper = mapper;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        [Route("NameEnquiry")]
        public async Task<IActionResult> Get(string AccountNumber)
        {

            Log.Information($"Attempting name enquiry @ {DateTime.Now} for Account number : {AccountNumber}");
            try
            {
                var msg = new { Account = AccountNumber };
                string formatBaseUrl = _dutyConfig.BaseUrl;

                Log.Information($"Name Enquiry request object { JsonConvert.SerializeObject(msg)}");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.NameEnquiry}"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json")
                };

                Log.Information($"Sending request name enquiry for account number {AccountNumber} to {_dutyConfig.BaseUrl}{_dutyConfig.NameEnquiry} ");
                HttpResponseMessage response = await _client.SendKaoshiRequest(requestMessage: request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    NameEnquiryResponse nameEnquiry = JsonConvert.DeserializeObject<NameEnquiryResponse>(responseString);
                    Log.Information($"Name Enquiry succeeded for account number {AccountNumber}, response object {responseString} ");

                    return Ok(new { Status = HttpStatusCode.OK, Message = "Request Successful", data = nameEnquiry });
                }
                string responseErr = await response.Content.ReadAsStringAsync();
                NameEnquiryResponse nameEnquiryErr = JsonConvert.DeserializeObject<NameEnquiryResponse>(responseErr);
                Log.Information($"Name Enquiry failed for account number {AccountNumber}, response object {responseErr} ");

                return BadRequest(new { Status = HttpStatusCode.BadRequest, Message = "Request Successful", data = nameEnquiryErr });
            }
            catch (Exception ex)
            {
                Log.Information($"An error occured while attemting name enquiry for account number {AccountNumber} error detail: {ex}");
                return BadRequest(new { Status = HttpStatusCode.InternalServerError, Message = "Request Successful", data = ex });
            }
        }

        //// GET api/<ValuesController>/5
        [HttpPost]
        [Route("EmailReciept")]
        public async Task<IActionResult> SendMail([FromForm] EmailCustomerRequest model)
        {
            Log.Information($"Attemping to send receipt to customer: {model.CustomerEmail}");
            try
            {
                if (!Request.Form.Files.Any()) return BadRequest(new { Status = HttpStatusCode.BadRequest, Message = "Request Successful", data = new { message = "No file is attached" } });

                if (Request.Form.Files.Count > 1) return BadRequest(new { Status = HttpStatusCode.BadRequest, Message = "Request failed", data = new { message = "You can not attach more one file" } });

                var files = Request.Form.Files;

                byte[] data;
                using (var br = new BinaryReader(Request.Form.Files[0].OpenReadStream()))
                    data = br.ReadBytes((int)Request.Form.Files[0].OpenReadStream().Length);

                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(data);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(fileContent, "Attachments", Path.GetFileName(Request.Form.Files[0].FileName));
                form.Add(new StringContent(model.CustomerEmail), "ToAddress");
                form.Add(new StringContent("Custom Duty Payment Receipt"), "Subject");
                form.Add(new StringContent(model.CustomerEmail), "Bcc");
                form.Add(new StringContent(model.CustomerEmail), "Cc");
                form.Add(new StringContent("Please download your receipt"), "MessageBody");

                Log.Information($"request form details: {JsonConvert.SerializeObject(form)}");

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_dutyConfig.EmailWithAttachment),
                    Content = form
                };

                Log.Information($"Emailing receipt to customer: {model.CustomerEmail}");
                HttpResponseMessage response = await _client.SendKaoshiRequest(requestMessage: request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    Log.Information($"Email sent to customer with message: {content} ");
                    return Ok(new { Status = HttpStatusCode.OK, Message = "Request Successful", data = content });
                }
                string responseString = await response.Content.ReadAsStringAsync();

                Log.Information($"Request failed with with message: {responseString} ");
                return BadRequest(new { Status = HttpStatusCode.BadRequest, Message = "Request Successful", data = responseString });
            }
            catch (Exception ex) {

                Log.Error($"An error occured: {ex}");
                return BadRequest(new { Status = HttpStatusCode.InternalServerError, Message = "Request failed", data = ex });
            }

        }

        // POST api/<ValuesController>
        [HttpGet]
        [Route("accountdetail/{AccountNumber}")]
        public async Task<IActionResult> FindAccountDetail(string AccountNumber)
        {
            Log.Information($"Attempting accounting detail @ {DateTime.Now} for Account number : {AccountNumber}");
            try
            {
                var msg = new { Account = AccountNumber };
                string formatBaseUrl = _dutyConfig.BaseUrl;

                Log.Information($"Account Detail request object { JsonConvert.SerializeObject(msg)}");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{_dutyConfig.BaseUrl}{_dutyConfig.AccountDetail}"),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json")
                };

                Log.Information($"sending Account Detail request for account number {AccountNumber} to {_dutyConfig.BaseUrl}{_dutyConfig.NameEnquiry} ");
                HttpResponseMessage response = await _client.SendKaoshiRequest(requestMessage: request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    AccountDetailResponse nameEnquiry = JsonConvert.DeserializeObject<AccountDetailResponse>(responseString);
                    Log.Information($"Account Detail  for account number {AccountNumber}, response object {responseString} ");

                    return Ok(new { Status = HttpStatusCode.OK, Message = "Request Successful", data = nameEnquiry });
                }
                string responseErr = await response.Content.ReadAsStringAsync();
                AccountDetailResponse nameEnquiryErr = JsonConvert.DeserializeObject<AccountDetailResponse>(responseErr);
                Log.Information($"Account detail failed for account number {AccountNumber}, response object {responseErr} ");

                return BadRequest(new { Status = HttpStatusCode.BadRequest, Message = "Request Successful", data = nameEnquiryErr });
            }
            catch (Exception ex)
            {
                Log.Information($"An error occured while attemting to get detail for account number {AccountNumber} error detail: {ex}");
                return BadRequest(new { Status = HttpStatusCode.InternalServerError, Message = "Request Successful", data = ex });
            }
        }

        //// PUT api/<ValuesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
