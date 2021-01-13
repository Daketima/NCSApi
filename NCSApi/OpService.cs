using NCSApi.Config;
using NCSApi.Contract;
using NCSApi.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NCSApi
{
    public class OpService
    {
        readonly DutyConfig _dutyConfig;
        readonly ICustomDutyClient _client;

        public OpService(DutyConfig DutyConfig, ICustomDutyClient client) { _dutyConfig = DutyConfig; _client = client; }

        public void SendMailToSupervisor(string SupervisorMail, string AuthorizerName, string AssessmentId, out string MailSentMessage)
        {
            MailSentMessage = null;

            string mailTemplate = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\EmailTemplates\\AuthorizerEmail.txt");
            mailTemplate = mailTemplate.Replace("{Authoriser}", AuthorizerName).Replace("{AssessmentId}", AssessmentId);

            SendMailRequest model = new SendMailRequest { ToAddress = SupervisorMail, Subject = "New Custom Duty Payment", MessageBody = mailTemplate };

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.EmailEndpoint}"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
            };

            var response = _client.SendKaoshiRequest(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                MailSentMessage = responseString;

            }
        }

        public void GetStaffDetail(string Branchcode,  string ApplicationName, out string SupervisorMail, out string SupervisorName)
        {
            SupervisorMail = null;
            SupervisorName = null;

             HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_dutyConfig.StaffDetailEndpoint}?branchCode={Branchcode}&appName={ApplicationName}"),
                Method = HttpMethod.Get,
               // Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
            };

            var response = _client.SendKaoshiRequest(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var allStaffDetail = JsonConvert.DeserializeObject<SupervisorDetail>(responseString);

                var supervisorDetail = allStaffDetail.users.FirstOrDefault();
                SupervisorMail = supervisorDetail.user.email;
                SupervisorName = $"{supervisorDetail.user.firstName} {supervisorDetail.user.lastName}";
            }

        }


    }
}

