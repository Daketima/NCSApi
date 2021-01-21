using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NCSApi.Implementation
{
    public class TransferService : ITransferService
    {
        BalanceObject deserialized = new BalanceObject();
        BalanceResponse balanceResponse = new BalanceResponse();
        private PostingRootObject postingRootObject;
        //private string balanceUrl;
        private string postUrl;
        private string username;
        private string password;

        public TransferService(IConfiguration config)
        {
            postingRootObject = new PostingRootObject();
            //balanceUrl = config.GetValue<string>("BalanceUrl");
            postUrl = config.GetValue<string>("PostUrl");
            username = config.GetValue<string>("ClientUsername");
            password = config.GetValue<string>("ClientPassword");
        }

        public async Task<PostingRootObject> PostTransferAsnyc(PostingDetailObj request)
        {
            //using (var client = new HttpClient())
            //{
            try
            {

                //string postUrl = ConfigurationManager.AppSettings["PostingApi"].ToString();

                //TODO: fetch posting url from app settings
                //string postUrl = ""; //appSetting.PostingApi;

                deserialized.response = balanceResponse;
                var client = new HttpClient();
                var byteContent = Configure_(request, out client);
                var res = client.PostAsync(postUrl, byteContent).GetAwaiter().GetResult();

                // return result;
                if (res.IsSuccessStatusCode)
                {
                    var PostJsonString = await res.Content.ReadAsStringAsync();
                    postingRootObject = JsonConvert.DeserializeObject<PostingRootObject>(PostJsonString);

                }


            }
            catch (Exception ex)
            {
                //log.Error("Error Message: " + ex.Message.ToString(), ex);
                // ex.ToString();
            }
            return postingRootObject;
            // }

        }
        public ByteArrayContent Configure_(object obj, out HttpClient _client)
        {
            var client = new HttpClient();

            var byteArray = Encoding.UTF8.GetBytes(username + ":" + password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            _client = client;
            var myContent = JsonConvert.SerializeObject(obj);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return byteContent;

        }

    }
    public class BalanceObject
    {
        public string ResponseMessage { get; set; }
        public string ResponseDescription { get; set; }
        public BalanceResponse response { get; set; }
    }
    public class BalanceResponse
    {
        public double AvailableBalance { get; set; }
        public double ReservedAmount { get; set; }
        public string AccountCurrency { get; set; }
    }
    public class PostingRootObject
    {
        public string ResponseMessage { get; set; }
        public string ResponseDescription { get; set; }
        public PostingResponse Response { get; set; }
    }
    public class PostingResponse
    {
        public string TranStatus { get; set; }
        public string TranStatusCode { get; set; }
        public string SopraReference { get; set; }
        public string ReasonCode { get; set; }
        public List<object> ReasonDescription { get; set; }
    }
    public class PostingRequestObj
    {
        public string RequestID { get; set; }
        public string TranCurrency { get; set; }
        public Accounts Accounts { get; set; }
    }
    public class PostingDetailObj
    {
        public string RequestID { get; set; }
        public string TranCurrency { get; set; }
        public Accounts Accounts { get; set; }
        public string SplitFee { get; internal set; }
        public string TranRemarks { get; internal set; }
        public string TranType { get; internal set; }
    }
    public class Accounts
    {
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public double TranAmount { get; set; }
        public string Narration { get; set; }
        public string CreditBranch { get;  set; }
        public string DebitBranch { get; set; }
    }
    public class AccountNo
    {
        public string Account { get; set; }
    }
}
